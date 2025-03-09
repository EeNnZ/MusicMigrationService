using MusicMigrationService.WebHost.Models.Enums;

namespace MusicMigrationService.WebHost.Models;

public record MigrationStatus(string JobId, JobStatuses Status, int Progress);