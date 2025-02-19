using MusicMigrationService.WebHost.Models.Interfaces;

namespace MusicMigrationService.WebHost.Services.Interfaces;

public interface IMusicService
{
    IAsyncEnumerable<IPlaylist?> GetUserPlaylistsAsync(CancellationToken token);
    Task<IPlaylist?> GetPlaylistAsync(string id, CancellationToken token);
    IAsyncEnumerable<string> GetFavoriteIdsAsync(CancellationToken token);
    Task<ITrack?> GetTrackAsync(string id);
}