using System.Runtime.CompilerServices;
using MusicMigrationService.WebHost.Models;
using MusicMigrationService.WebHost.Models.Interfaces;
using MusicMigrationService.WebHost.Models.Playlists;
using MusicMigrationService.WebHost.Models.ResultTypes;
using MusicMigrationService.WebHost.Services.Interfaces;
using SpotifyAPI.Web;

namespace MusicMigrationService.WebHost.Services;

public class SpotifyService(ISpotifyClient spotifyClient) : ISpotifyService
{
    public async IAsyncEnumerable<IPlaylist?> GetUserPlaylistsAsync([EnumeratorCancellation] CancellationToken token)
    {
        Paging<FullPlaylist> response = await spotifyClient.Playlists.CurrentUsers(token);

        if (response.Items == null)
            yield break;
        
        foreach (FullPlaylist item in response.Items)
            yield return new SpotifyPlaylist(item);
    }

    public async Task<IPlaylist?> GetPlaylistAsync(string id, CancellationToken token)
    {
        FullPlaylist playlist = await spotifyClient.Playlists.Get(id, token);
        return new SpotifyPlaylist(playlist);
    }

    public async IAsyncEnumerable<string> GetFavoriteIdsAsync([EnumeratorCancellation] CancellationToken token)
    {
        await foreach (FullTrack track in GetFavoritesAsync(token))
            yield return track.Id;
    }

    public async IAsyncEnumerable<ITrack> GetFavoriteTracksAsync([EnumeratorCancellation] CancellationToken token)
    {
        await foreach (FullTrack fullTrack in GetFavoritesAsync(token))
        {
            var track = new Track(fullTrack);
            yield return track;
        }
    }

    private async IAsyncEnumerable<FullTrack> GetFavoritesAsync([EnumeratorCancellation] CancellationToken token)
    {
        Paging<SavedTrack> response = await spotifyClient.Library.GetTracks(token);
        
        if (response.Items == null)
            yield break;

        foreach (SavedTrack savedTrack in response.Items)
        {
            token.ThrowIfCancellationRequested();
            yield return savedTrack.Track;
        }
    }

    public async Task<ITrack?> GetTrackAsync(string id)
    {
        FullTrack response = await spotifyClient.Tracks.Get(id);
        return new Track(response);
    }

    public async IAsyncEnumerable<ITrack> SearchTrackAsync(string title, string artist, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (FullTrack fullTrack in SearchFullTracksAsync(title, artist, token))
            yield return new Track(fullTrack);
    }

    private async IAsyncEnumerable<FullTrack> SearchFullTracksAsync(string title, string artist, [EnumeratorCancellation] CancellationToken token)
    {
        var searchRequest = new SearchRequest(SearchRequest.Types.Track, string.Join(" ", title, artist));
        SearchResponse response = await spotifyClient.Search.Item(searchRequest, CancellationToken.None);

        if (response.Tracks.Items == null)
            yield break;
        
        IEnumerable<FullTrack> tracks = response.Tracks.Items;

        foreach (FullTrack fullTrack in tracks)
            yield return fullTrack;
    }

    public async Task<IPlaylist> CreatePlaylistAsync(string userId, string name, string description)
    {
        var playlistCreateRequest = new PlaylistCreateRequest(name) { Description = description, };
        FullPlaylist result = await spotifyClient.Playlists.Create(userId, playlistCreateRequest);
        return new SpotifyPlaylist(result);
    }

    public async Task<bool> AddTracksToPlaylistAsync(string playlistId, IEnumerable<ITrack> tracks,
                                                     CancellationToken token)
    {
        FullPlaylist playlist = await spotifyClient.Playlists.Get(playlistId, token);
        
        if (playlist.Id == null)
            return false;
        
        ITrack[] trackArray = tracks.ToArray();

        foreach (ITrack trackToFind in trackArray)
        {
            token.ThrowIfCancellationRequested();
            
            bool tracksFound = await SearchFullTracksAsync(trackToFind.Title, trackToFind.Artist, token).AnyAsync(token);
            if (!tracksFound)
                return false;
            
            await foreach(FullTrack foundTrack in SearchFullTracksAsync(trackToFind.Title, trackToFind.Artist, token))
            {
                var candidate = new Track(foundTrack);
                if (candidate.LooksLike(trackToFind))
                {
                    var addRequest = new PlaylistAddItemsRequest(new [] { playlistId });
                    await spotifyClient.Playlists.AddItems(playlist.Id, addRequest, token);
                }
            }
        }
        return true;
    }

    public async Task<OperationResult> AddToFavoritesAsync(IEnumerable<ITrack> tracks, CancellationToken token)
    {
        ITrack[] tracksArray = tracks as ITrack[] ?? tracks.ToArray();
        
        if (tracksArray.Length > 50)
            return new OperationResult(SuccessStatus.Error, "Too many favorite tracks (max 50)");
        
        var idsToSave = new List<string>();
        foreach (ITrack track in tracksArray)
        {
            await foreach (FullTrack foundTrack in SearchFullTracksAsync(track.Title, track.Artist, token))
            {
                var candidate = new Track(foundTrack);
                if (candidate.LooksLike(track))
                {
                    idsToSave.Add(foundTrack.Id);
                }
            }
        }

        bool success = await spotifyClient.Library.SaveTracks(new LibrarySaveTracksRequest(idsToSave), token);
        return success ? new OperationResult(SuccessStatus.Full) : new OperationResult(SuccessStatus.Error, "Failed to save tracks");
    }

    public async Task<bool> AddToFavoritesAsync(ITrack track, CancellationToken token)
    {
        var idToSave = new List<string>();
        await foreach (FullTrack foundTrack in SearchFullTracksAsync(track.Title, track.Artist, token))
        {
            var candidate = new Track(foundTrack);
            if (candidate.LooksLike(track))
            {
                idToSave.Add(foundTrack.Id);
                break;
            }
        }
        
        return await spotifyClient.Library.SaveTracks(new LibrarySaveTracksRequest(idToSave), token);
    }

    public async Task<string> GetCurrentUserIdAsync()
    {
        PrivateUser privateUser = await spotifyClient.UserProfile.Current();
        return privateUser.Id;
    }
}