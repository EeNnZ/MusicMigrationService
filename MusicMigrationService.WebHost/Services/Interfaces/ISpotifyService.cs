using MusicMigrationService.WebHost.Models.Interfaces;

namespace MusicMigrationService.WebHost.Services.Interfaces;

public interface ISpotifyService : IMusicService
{
    Task<IEnumerable<ITrack>> SearchTrackAsync(string title, string artist, CancellationToken token);
    Task<string> CreatePlaylistAsync(string name, string description);
    Task AddTracksToPlaylistAsync(string playlistId, IEnumerable<string> tracks);
    Task<string> GetCurrentUserIdAsync();
}