using MusicMigrationService.WebHost.Models;
using MusicMigrationService.WebHost.Models.Interfaces;
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
    
    
    public async IAsyncEnumerable<IPlaylist?> GetUserPlaylistsAsync(CancellationToken token)
    {
        Paging<FullPlaylist> response = await _spotify.Playlists.CurrentUsers(token);

        if (response.Items != null)
            foreach (FullPlaylist item in response.Items)
            {
                yield return new SpotifyPlaylist(item);
            }
    }

    public Task<IPlaylist?> GetPlaylistAsync(string id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<string> GetFavoriteIdsAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<ITrack?> GetTrackAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<string> SearchTrackAsync(string title, string artist)
    {
        throw new NotImplementedException();
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