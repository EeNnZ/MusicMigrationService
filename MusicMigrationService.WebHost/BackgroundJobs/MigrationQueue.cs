using System.Collections.Concurrent;
using MusicMigrationService.WebHost.Models;
using MusicMigrationService.WebHost.Models.Enums;

namespace MusicMigrationService.WebHost.BackgroundJobs;

public class MigrationQueue : IMigrationQueue
{
    private readonly ConcurrentQueue<MigrationJob?> _jobs = new();
    private readonly ConcurrentDictionary<string, MigrationStatus> _migrationStatuses = new();
    
    public void EnqueueJob(MigrationJob? job)
    {
        ArgumentNullException.ThrowIfNull(job);

        _jobs.Enqueue(job);
        _migrationStatuses[job.JobId] = new MigrationStatus(job.JobId, JobStatuses.Queued, 0);
    }

    public bool TryDequeueJob(out MigrationJob? job)
    {
        return _jobs.TryDequeue(out job);
    }
    public MigrationStatus? GetMigrationStatus(string jobId) =>
        _migrationStatuses.TryGetValue(jobId, out var status) ? status : null;
    
    public void SetMigrationStatus(string jobId, MigrationStatus migrationStatus) =>
        _migrationStatuses[jobId] = migrationStatus;
}