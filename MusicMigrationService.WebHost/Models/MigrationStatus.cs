namespace MusicMigrationService.WebHost.Models;

public record MigrationStatus(string JobId, string Status, int Progress);