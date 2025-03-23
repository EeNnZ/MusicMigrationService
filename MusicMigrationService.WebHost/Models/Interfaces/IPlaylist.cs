namespace MusicMigrationService.WebHost.Models.Interfaces;

public interface IPlaylist
{
    string Id { get; }
    string Title { get; }
    IEnumerable<ITrack> Tracks { get; }
}