using MusicMigrationService.WebHost.BackgroundJobs;
using MusicMigrationService.WebHost.Models;
using MusicMigrationService.WebHost.Models.Enums;
using MusicMigrationService.WebHost.Models.Interfaces;
using MusicMigrationService.WebHost.Services.Interfaces;

namespace MusicMigrationService.WebHost.Services;

public class MigrationBackgroundService(
    IMigrationQueue queue,
    IMusicService sourceService,
    IMusicService destinationService,
    ILogger<MigrationBackgroundService> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (queue.TryDequeueJob(out MigrationJob? job))
            {
                try
                {
                    queue.SetMigrationStatus(job!.JobId, new MigrationStatus(job.JobId, JobStatuses.Processing, 0));
                    await ProcessMigrationJob(job, stoppingToken);
                    queue.SetMigrationStatus(job.JobId, new MigrationStatus(job.JobId, JobStatuses.Completed, 100));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, message:"Failed to process migration job {job} with id: {id}", job, job?.JobId);
                    queue.SetMigrationStatus(job!.JobId, new MigrationStatus(job.JobId, JobStatuses.Failed, 0));
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task ProcessMigrationJob(MigrationJob job, CancellationToken token)
    {
        IAsyncEnumerable<ITrack> favoriteTracksAsync = sourceService.GetFavoriteTracksAsync(token);
        
        ITrack[] array = await favoriteTracksAsync.ToArrayAsync(token);
        
        await destinationService.AddToFavoritesAsync(array, token);
    }
}