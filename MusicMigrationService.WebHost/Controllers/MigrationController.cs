using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MusicMigrationService.WebHost.BackgroundJobs;
using MusicMigrationService.WebHost.Models;

namespace MusicMigrationService.WebHost.Controllers
{
    [Route("api/migration")]
    [ApiController]
    public class MigrationController : ControllerBase
    {
        private readonly ILogger<MigrationController> _logger;
        private readonly IMigrationQueue _queue;

        public MigrationController(IMigrationQueue queue, ILogger<MigrationController> logger)
        {
            _logger = logger;
            _queue = queue;
        }

        [HttpPost("start")]
        public IActionResult StartMigration([FromBody] MigrationRequest request)
        {
            var jobId = Guid.NewGuid().ToString();
            _queue.EnqueueJob(new MigrationJob(jobId, request.UserId));
            
            _logger.LogInformation($"Started migration: {jobId}");
            return Accepted(new { JobId = jobId });
        }

        [HttpGet("status/{jobId}")]
        public IActionResult GetStatus(string jobId)
        {
            MigrationStatus? status = _queue.GetMigrationStatus(jobId);
            
            return status != null ? Ok(status) : NotFound();
        }
    }
}
