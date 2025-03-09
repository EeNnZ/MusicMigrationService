using MusicMigrationService.WebHost.Models.Interfaces;

namespace MusicMigrationService.WebHost.Services.Interfaces;

public interface IMusicService
{
    IAsyncEnumerable<IPlaylist?> GetUserPlaylistsAsync(CancellationToken token);
    Task<IPlaylist?> GetPlaylistAsync(string id, CancellationToken token);
    IAsyncEnumerable<string> GetFavoriteIdsAsync(CancellationToken token);
    IAsyncEnumerable<ITrack> GetFavoriteTracksAsync(CancellationToken token);
    Task<ITrack?> GetTrackAsync(string id);
    
    IAsyncEnumerable<ITrack> SearchTrackAsync(string title, string artist, CancellationToken token);
    Task<IPlaylist> CreatePlaylistAsync(string userId, string name, string description);
    Task AddTracksToPlaylistAsync(string playlistId, IEnumerable<string> uris);
    Task<string> GetCurrentUserIdAsync();
}