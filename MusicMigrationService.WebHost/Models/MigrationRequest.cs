namespace MusicMigrationService.WebHost.Models;

public record MigrationRequest(string UserId, string SourceName, string DestinationName);