using System.Runtime.CompilerServices;
using MusicMigrationService.WebHost.Models;
using MusicMigrationService.WebHost.Models.Interfaces;
using MusicMigrationService.WebHost.Models.Playlists;
using MusicMigrationService.WebHost.Services.Interfaces;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Models.Account;
using Yandex.Music.Api.Models.Common;
using Yandex.Music.Api.Models.Library;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Track;

namespace MusicMigrationService.WebHost.Services;

public class YandexMusicService : IMusicService
{
    private readonly YandexMusicApi  _api;
    private readonly AuthStorage _storage;

    public YandexMusicService(YandexMusicApi api, string token)
    {
        _api = api;
        _storage = new AuthStorage()
        {
            AccessToken = new YAccessToken() { AccessToken = token }
        };
    }


    public async IAsyncEnumerable<IPlaylist?> GetUserPlaylistsAsync([EnumeratorCancellation] CancellationToken token)
    {
        List<YResponse<YPlaylist>>? response = await _api.Playlist.GetPersonalPlaylistsAsync(_storage);
        
        if (response == null)
            yield break;
        
        if (response.Count == 0)
            yield break;
        
        foreach (var pl in response)
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
            token.ThrowIfCancellationRequested();
            if (playlist?.Id == id)
                return playlist;
        }
        return null;
    }

    public async IAsyncEnumerable<string> GetFavoriteIdsAsync([EnumeratorCancellation] CancellationToken token)
    {
        YResponse<YLibraryTracks>? response = await _api.Library.GetLikedTracksAsync(_storage);

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

    public async Task<ITrack?> GetTrackAsync(string id)
    {
        YResponse<List<YTrack>>? response = await _api.Track.GetAsync(_storage, id);

        YTrack? track = response?.Result?.FirstOrDefault();
        
        return track != null ? new Track(track) : null;
    }
}