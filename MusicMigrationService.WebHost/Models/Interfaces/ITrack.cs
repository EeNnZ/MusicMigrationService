namespace MusicMigrationService.WebHost.Models.Interfaces;

public interface ITrack
{
    string Id { get; }
    string Title { get; }
    string Artist { get; }
    uint Duration { get; }
}