namespace MusicMigrationService.WebHost.Models.ResultTypes;

public record OperationResult(SuccessStatus IsSuccess, string? Message = null);