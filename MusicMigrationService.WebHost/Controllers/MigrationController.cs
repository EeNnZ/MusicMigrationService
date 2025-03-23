using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using MusicMigrationService.WebHost.BackgroundJobs;
using MusicMigrationService.WebHost.Models;

namespace MusicMigrationService.WebHost.Controllers
{
    [Route("api/migration")]
    [ApiController]
    public class MigrationController(
        IMigrationQueue queue,
        IHostedService migrationBackgroundService,
        ILogger<MigrationController> logger)
        : ControllerBase
    {
        [HttpPost("start")]
        public IActionResult StartMigration([FromBody] MigrationRequest request)
        {
            var jobId = Guid.NewGuid().ToString();
            queue.EnqueueJob(new MigrationJob(jobId, request.UserId));
            
            // migrationBackgroundService.StartAsync(CancellationToken.None); //todo
            
            logger.LogInformation($"Started migration: {jobId}");
            return Accepted(new { JobId = jobId });
        }

        [HttpGet("status/{jobId}")]
        public IActionResult GetStatus(string jobId)
        {
            MigrationStatus? status = queue.GetMigrationStatus(jobId);
            
            return status != null ? Ok(status) : NotFound();
        }
    }
}
