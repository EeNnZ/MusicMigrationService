using MusicMigrationService.WebHost.Models;

namespace MusicMigrationService.WebHost.BackgroundJobs;

public interface IMigrationQueue
{
    void EnqueueJob(MigrationJob job);
    MigrationStatus? GetMigrationStatus(string jobId);
}