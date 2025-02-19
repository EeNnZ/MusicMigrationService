using System.Collections.Concurrent;
using MusicMigrationService.WebHost.Models;

namespace MusicMigrationService.WebHost.BackgroundJobs;

public class MigrationQueue : IMigrationQueue
{
    private readonly ConcurrentQueue<MigrationJob> _jobs = new();
    private readonly ConcurrentDictionary<string, MigrationStatus> _migrationStatuses = new();
    
    public void EnqueueJob(MigrationJob job)
    {
        _jobs.Enqueue(job);
        _migrationStatuses[job.JobId] = new MigrationStatus(job.JobId, "Queued", 0);
    }

    public MigrationStatus? GetMigrationStatus(string jobId) =>
        _migrationStatuses.TryGetValue(jobId, out var status) ? status : null;
}