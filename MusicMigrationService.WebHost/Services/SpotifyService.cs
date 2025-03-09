using System.Runtime.CompilerServices;
using MusicMigrationService.WebHost.Models;
using MusicMigrationService.WebHost.Models.Interfaces;
using MusicMigrationService.WebHost.Models.Playlists;
using MusicMigrationService.WebHost.Services.Interfaces;
using SpotifyAPI.Web;
using Swan;

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
        Paging<SavedTrack> response = await spotifyClient.Library.GetTracks(token);

        if (response.Items == null)
            yield break;

        foreach (SavedTrack track in response.Items)
            yield return track.Track.Id;
    }

    public async Task<ITrack?> GetTrackAsync(string id)
    {
        FullTrack response = await spotifyClient.Tracks.Get(id);
        return new Track(response);
    }

    public async IAsyncEnumerable<ITrack> SearchTrackAsync(string title, string artist, [EnumeratorCancellation] CancellationToken token)
    {
        var searchRequest = new SearchRequest(SearchRequest.Types.Track, string.Join(" ", title, artist));
        SearchResponse response = await spotifyClient.Search.Item(searchRequest, CancellationToken.None);

        if (response.Tracks.Items == null)
            yield break;

        IEnumerable<Track> tracks = response.Tracks.Items.Select(t => new Track(t));
        
        foreach (Track track in tracks)
            yield return track;
    }

    public async Task<IPlaylist> CreatePlaylistAsync(string userId, string name, string description)
    {
        var playlistCreateRequest = new PlaylistCreateRequest(name) { Description = description, };
        FullPlaylist result = await spotifyClient.Playlists.Create(userId, playlistCreateRequest);
        return new SpotifyPlaylist(result);
    }

    public async Task AddTracksToPlaylistAsync(string playlistId, IEnumerable<string> uris)
    {
        var playlistAddItemsRequest = new PlaylistAddItemsRequest(uris.ToList());
        await spotifyClient.Playlists.AddItems(playlistId, playlistAddItemsRequest);
    }

    public async Task<string> GetCurrentUserIdAsync()
    {
        PrivateUser privateUser = await spotifyClient.UserProfile.Current();
        return privateUser.Id;
    }
}