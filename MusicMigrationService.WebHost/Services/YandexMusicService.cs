using System.Runtime.CompilerServices;
using MusicMigrationService.WebHost.Models;
using MusicMigrationService.WebHost.Models.Interfaces;
using MusicMigrationService.WebHost.Models.Playlists;
using MusicMigrationService.WebHost.Models.ResultTypes;
using MusicMigrationService.WebHost.Services.Interfaces;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Extensions.API;
using Yandex.Music.Api.Models.Account;
using Yandex.Music.Api.Models.Common;
using Yandex.Music.Api.Models.Library;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Search;
using Yandex.Music.Api.Models.Search.Track;
using Yandex.Music.Api.Models.Track;

namespace MusicMigrationService.WebHost.Services;

public class YandexMusicService(YandexMusicApi api, string token) : IMusicService
{
    private readonly AuthStorage _storage = new()
    {
        AccessToken = new YAccessToken() { AccessToken = token }
    };


    public async IAsyncEnumerable<IPlaylist?> GetUserPlaylistsAsync([EnumeratorCancellation] CancellationToken token)
    {
        List<YResponse<YPlaylist>>? response = await api.Playlist.GetPersonalPlaylistsAsync(_storage).ConfigureAwait(false);
        
        if (response == null)
            yield break;
        
        if (response.Count == 0)
            yield break;
        
        foreach (YResponse<YPlaylist> pl in response)
        {
            token.ThrowIfCancellationRequested();
            yield return new YandexPlaylist(pl.Result);
        }
    }

    public async Task<IPlaylist?> GetPlaylistAsync(string id, CancellationToken token)
    {
        IAsyncEnumerable<IPlaylist?> playlists = GetUserPlaylistsAsync(token);
        await foreach (IPlaylist? playlist in playlists)
        {
            if (playlist?.Id == id)
                return playlist;
        }
        return null;
    }

    public async IAsyncEnumerable<string> GetFavoriteIdsAsync([EnumeratorCancellation] CancellationToken token)
    {
        YResponse<YLibraryTracks>? response = await api.Library.GetLikedTracksAsync(_storage).ConfigureAwait(false);

        if (response?.Result == null)
            yield break;
        
        if(response.Result.Library.Tracks.Count == 0)
            yield break;
        
        foreach (YLibraryTrack lbTrack in response.Result.Library.Tracks)
        {
            token.ThrowIfCancellationRequested();
            yield return lbTrack.Id;
        }
    }

    public async IAsyncEnumerable<ITrack> GetFavoriteTracksAsync([EnumeratorCancellation] CancellationToken token)
    {
        YResponse<YLibraryTracks>? response = await api.Library.GetLikedTracksAsync(_storage).ConfigureAwait(false);
        
        if (response?.Result == null)
            yield break;
        
        if(response.Result.Library.Tracks.Count == 0)
            yield break;
        
        string[] ids = response.Result.Library.Tracks.Select(t => t.Id).ToArray();
        YResponse<List<YTrack>>? tracks = await  api.Track.GetAsync(_storage, ids).ConfigureAwait(false);

        foreach (YTrack yTrack in tracks.Result)
        {
            token.ThrowIfCancellationRequested();
            yield return new Track(yTrack);
        }
    }


    public async Task<ITrack?> GetTrackAsync(string id)
    {
        YResponse<List<YTrack>>? response = await api.Track.GetAsync(_storage, id).ConfigureAwait(false);

        YTrack? track = response?.Result?.FirstOrDefault();
        
        return track != null ? new Track(track) : null;
    }

    public async IAsyncEnumerable<ITrack> SearchTrackAsync(string title, string artist, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (YTrack yTrack in SearchYTrackAsync(title, artist, token))
        {
            yield return new Track(yTrack);
        }
    }

    private async IAsyncEnumerable<YTrack> SearchYTrackAsync(string title, string artist, [EnumeratorCancellation] CancellationToken token)
    {
        YResponse<YSearch>? response = await api.Search.TrackAsync(_storage, string.Join(" ", title, artist)).ConfigureAwait(false);

        if (response?.Result == null || response.Result.Tracks.Total == 0)
            yield break;

        foreach (YSearchTrackModel trackModel in response.Result.Tracks.Results)
        {
            token.ThrowIfCancellationRequested();
            if (trackModel is YTrack track)
                yield return track;
        }
    }

    public async Task<IPlaylist> CreatePlaylistAsync(string userId, string name, string description)
    {
        YResponse<YPlaylist> plCreated = await api.Playlist.CreateAsync(_storage, name).ConfigureAwait(false);
        return new YandexPlaylist(plCreated.Result);
    }

    public async Task<bool> AddTracksToPlaylistAsync(string playlistId, IEnumerable<ITrack> tracks, CancellationToken token)
    {
        YResponse<YPlaylist> yResponse = await api.Playlist.GetAsync(_storage, _storage.User.Uid, playlistId).ConfigureAwait(false);
        YPlaylist? yPlaylist = yResponse.Result;

        ITrack[] trackArray = tracks.ToArray();

        foreach (ITrack trackToFind in trackArray)
        {
            token.ThrowIfCancellationRequested();

            bool tracksFound = await SearchTrackAsync(trackToFind.Title, trackToFind.Artist, token).AnyAsync(token);
            
            if (!tracksFound)
                return false;
            
            await foreach (YTrack foundTrack in SearchYTrackAsync(trackToFind.Title, trackToFind.Artist, token))
            {
                var candidate = new Track(foundTrack);
                if (candidate.LooksLike(trackToFind))
                    await yPlaylist.InsertTracksAsync(foundTrack).ConfigureAwait(false);
            }
        }
        
        return true;
    }

    public async Task<OperationResult> AddToFavoritesAsync(IEnumerable<ITrack> tracks, CancellationToken token)
    {
        IEnumerable<ITrack> enumerated = tracks as ITrack[] ?? tracks.ToArray();
        
        int inputCount = enumerated.Count();
        int addedCount = 0;
        foreach (ITrack track in enumerated)
        {
            bool ok = await AddToFavoritesAsync(track, token);
            if (ok) 
                addedCount++;
        }

        if (addedCount == inputCount)
        {
            return new OperationResult(SuccessStatus.Full);
        }
        if (addedCount > 0 && addedCount < inputCount)
        {
            string message = $"Added {addedCount} of {inputCount} requested track(s).";
            return new OperationResult(SuccessStatus.Partial, message);
        }
        
        return new OperationResult(SuccessStatus.Error);
    }

    public async Task<bool> AddToFavoritesAsync(ITrack track, CancellationToken token)
    {
        await foreach (YTrack yTrack in SearchYTrackAsync(track.Title, track.Artist, token))
        {
            var candidate = new Track(yTrack);
            if (candidate.LooksLike(track) && await LikeTrackAsync(yTrack))
                return true;
        }
        return false;
    }

    private async Task<bool> LikeTrackAsync(YTrack yTrack)
    {
        YResponse<YPlaylist>? response = await api.Library.AddTrackLikeAsync(_storage, yTrack).ConfigureAwait(false);
                
        if (response?.Result == null)
            return false;
        
        return response.Result.Revision > 0;
    }

    public Task<string> GetCurrentUserIdAsync() => Task.FromResult(_storage.User.Uid);
}