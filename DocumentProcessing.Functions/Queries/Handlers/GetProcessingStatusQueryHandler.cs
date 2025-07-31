using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using DocumentProcessing.Functions.Queries;
using DocumentProcessing.Functions.Models;
using DocumentProcessing.Functions.Infrastructure;

namespace DocumentProcessing.Functions.Queries.Handlers;

public class GetProcessingStatusQueryHandler : IQueryHandler<GetProcessingStatusQuery, ProcessingStatusResponse?>
{
    private readonly DurableTaskClient _durableTaskClient;
    private readonly ILogger<GetProcessingStatusQueryHandler> _logger;

    public GetProcessingStatusQueryHandler(DurableTaskClient durableTaskClient, ILogger<GetProcessingStatusQueryHandler> logger)
    {
        _durableTaskClient = durableTaskClient;
        _logger = logger;
    }

    public async Task<ProcessingStatusResponse?> HandleAsync(GetProcessingStatusQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting processing status for batch {BatchId}", request.BatchId);

            // In a real implementation, you would query a database or storage
            // For this example, we'll simulate the response
            
            // Try to get orchestration instance by batch ID
            // Note: In production, you'd need to store the mapping between BatchId and InstanceId
            var orchestrationInstanceId = request.BatchId; // Assuming BatchId is used as instance ID
            
            var orchestrationMetadata = await _durableTaskClient.GetInstanceAsync(orchestrationInstanceId);
            
            if (orchestrationMetadata == null)
            {
                _logger.LogWarning("No orchestration found for batch {BatchId}", request.BatchId);
                return null;
            }

            var response = new ProcessingStatusResponse
            {
                BatchId = request.BatchId,
                Status = MapRuntimeStatusToString(orchestrationMetadata.RuntimeStatus),
                StartedAt = orchestrationMetadata.CreatedAt.DateTime,
                CompletedAt = IsOrchestrationCompleted(orchestrationMetadata.RuntimeStatus) ? orchestrationMetadata.LastUpdatedAt.DateTime : null
            };

            // If the orchestration is completed, try to get the output
            if (IsOrchestrationCompleted(orchestrationMetadata.RuntimeStatus))
            {
                try
                {
                    var output = orchestrationMetadata.ReadOutputAs<ProcessDocumentsResponse>();
                    if (output != null)
                    {
                        response.TotalDocuments = output.Results.Count;
                        response.ProcessedDocuments = output.Results.Count;
                        response.ValidDocuments = output.Results.Count(r => r.IsValid);
                        response.InvalidDocuments = output.Results.Count(r => !r.IsValid);
                        response.SuccessfulPrints = output.Results.Count(r => r.PrintingSuccessful);
                        response.FailedPrints = output.Results.Count(r => !r.PrintingSuccessful && r.IsValid);
                        response.DocumentResults = output.Results;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not read orchestration output for batch {BatchId}", request.BatchId);
                }
            }

            _logger.LogInformation("Retrieved processing status for batch {BatchId}: {Status}", request.BatchId, response.Status);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processing status for batch {BatchId}", request.BatchId);
            throw;
        }
    }

    private static bool IsOrchestrationCompleted(OrchestrationRuntimeStatus status)
    {
        return status == OrchestrationRuntimeStatus.Completed ||
               status == OrchestrationRuntimeStatus.Failed ||
               status == OrchestrationRuntimeStatus.Terminated;
    }

    private static string MapRuntimeStatusToString(OrchestrationRuntimeStatus status)
    {
        return status switch
        {
            OrchestrationRuntimeStatus.Running => "Processing",
            OrchestrationRuntimeStatus.Completed => "Completed",
            OrchestrationRuntimeStatus.Failed => "Failed",
            OrchestrationRuntimeStatus.Terminated => "Terminated",
            OrchestrationRuntimeStatus.Pending => "Pending",
            OrchestrationRuntimeStatus.Suspended => "Suspended",
            _ => "Unknown"
        };
    }
}