using MusicMigrationService.WebHost.Models;

namespace MusicMigrationService.WebHost.BackgroundJobs;

public interface IMigrationQueue
{
    void EnqueueJob(MigrationJob? job);
    MigrationStatus? GetMigrationStatus(string jobId);
    bool TryDequeueJob(out MigrationJob? job);
    void SetMigrationStatus(string jobId, MigrationStatus migrationStatus);
}