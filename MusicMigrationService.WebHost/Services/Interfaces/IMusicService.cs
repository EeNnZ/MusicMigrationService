using MusicMigrationService.WebHost.Models.Interfaces;
using MusicMigrationService.WebHost.Models.ResultTypes;

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
    Task<bool> AddTracksToPlaylistAsync(string playlistId, IEnumerable<ITrack> tracks, CancellationToken token);
    Task<OperationResult> AddToFavoritesAsync(IEnumerable<ITrack> tracks, CancellationToken token);
    Task<bool> AddToFavoritesAsync(ITrack track, CancellationToken token);
    Task<string> GetCurrentUserIdAsync();
}