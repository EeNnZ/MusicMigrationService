using System.Runtime.CompilerServices;
using MusicMigrationService.WebHost.Models;
using MusicMigrationService.WebHost.Models.Interfaces;
using MusicMigrationService.WebHost.Models.Playlists;
using MusicMigrationService.WebHost.Services.Interfaces;
using SpotifyAPI.Web;

namespace MusicMigrationService.WebHost.Services;

public class SpotifyService : ISpotifyService
{
    private readonly ISpotifyClient _spotify;

    public SpotifyService(ISpotifyClient spotify)
    {
        _spotify = spotify;
    }
    
    
    public async IAsyncEnumerable<IPlaylist?> GetUserPlaylistsAsync([EnumeratorCancellation] CancellationToken token)
    {
        Paging<FullPlaylist> response = await _spotify.Playlists.CurrentUsers(token);

        if (response.Items == null)
            yield break;
        
        foreach (FullPlaylist item in response.Items)
            yield return new SpotifyPlaylist(item);
    }

    public async Task<IPlaylist?> GetPlaylistAsync(string id, CancellationToken token)
    {
        FullPlaylist playlist = await _spotify.Playlists.Get(id, token);
        return new SpotifyPlaylist(playlist);
    }

    public async IAsyncEnumerable<string> GetFavoriteIdsAsync([EnumeratorCancellation] CancellationToken token)
    {
        Paging<SavedTrack> response = await _spotify.Library.GetTracks(token);

        if (response.Items == null)
            yield break;

        foreach (SavedTrack track in response.Items)
            yield return track.Track.Id;
    }

    public async Task<ITrack?> GetTrackAsync(string id)
    {
        FullTrack response = await _spotify.Tracks.Get(id);
        return new Track(response);
    }

    public async Task<IEnumerable<ITrack>> SearchTrackAsync(string title, string artist, CancellationToken token)
    {
        SearchRequest request = new SearchRequest(SearchRequest.Types.Track, string.Join(" ", title, artist));
        SearchResponse response = await _spotify.Search.Item(request, CancellationToken.None);

        if (response.Tracks.Items == null)
            return Enumerable.Empty<ITrack>();

        var tracks = response.Tracks.Items.Select(t => new Track(t));
        return tracks;
    }

    public Task<string> CreatePlaylistAsync(string name, string description)
    {
        throw new NotImplementedException();
    }

    public Task AddTracksToPlaylistAsync(string playlistId, IEnumerable<string> tracks)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetCurrentUserIdAsync()
    {
        throw new NotImplementedException();
    }
}