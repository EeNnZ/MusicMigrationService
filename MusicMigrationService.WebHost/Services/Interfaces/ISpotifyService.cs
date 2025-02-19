namespace MusicMigrationService.WebHost.Services.Interfaces;

public interface ISpotifyService : IMusicService
{
    Task<string> SearchTrackAsync(string title, string artist);
    Task<string> CreatePlaylistAsync(string name, string description);
    Task AddTracksToPlaylistAsync(string playlistId, IEnumerable<string> tracks);
    Task<string> GetCurrentUserIdAsync();
}