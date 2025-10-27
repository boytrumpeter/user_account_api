using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using DocumentProcessing.Functions.Models.Domain;

namespace DocumentProcessing.Functions.Orchestrators;

public static class SubmissionProcessingOrchestrator
{
    [Function("SubmissionProcessingOrchestrator")]
    public static async Task<SubmissionProcessingResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("SubmissionProcessingOrchestrator");
        var submissionId = context.GetInput<Guid>();
        
        logger.LogInformation("Starting submission processing orchestration for ID: {SubmissionId}", submissionId);

        var result = new SubmissionProcessingResult
        {
            SubmissionId = submissionId,
            StartedAt = context.CurrentUtcDateTime
        };

        try
        {
            // Step 1: Download and validate XML from blob
            logger.LogInformation("Step 1: Downloading and validating XML blob");
            var downloadResult = await context.CallActivityAsync<BlobDownloadResult>("DownloadXmlFromBlobActivity", submissionId);
            
            if (!downloadResult.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = downloadResult.ErrorMessage;
                result.CompletedAt = context.CurrentUtcDateTime;
                return result;
            }

            // Step 2: Validate XML content
            logger.LogInformation("Step 2: Validating XML content");
            var validationResult = await context.CallActivityAsync<XmlValidationActivityResult>("ValidateXmlActivity", 
                new XmlValidationActivityInput { SubmissionId = submissionId, XmlContent = downloadResult.XmlContent });

            if (!validationResult.IsValid)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "XML validation failed";
                result.ValidationErrors = validationResult.ValidationErrors;
                result.CompletedAt = context.CurrentUtcDateTime;
                return result;
            }

            // Step 3: Extract communications from XML
            logger.LogInformation("Step 3: Extracting communications from XML");
            var extractionResult = await context.CallActivityAsync<CommunicationExtractionActivityResult>("ExtractCommunicationsActivity",
                new CommunicationExtractionActivityInput { SubmissionId = submissionId, XmlContent = downloadResult.XmlContent });

            if (!extractionResult.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = extractionResult.ErrorMessage;
                result.CompletedAt = context.CurrentUtcDateTime;
                return result;
            }

            // Step 4: Process communications in parallel (fan-out/fan-in)
            logger.LogInformation("Step 4: Processing {Count} communications in parallel", extractionResult.CommunicationIds.Count);
            
            var communicationTasks = extractionResult.CommunicationIds
                .Select(commId => context.CallActivityAsync<CommunicationProcessingResult>("ProcessCommunicationActivity", commId))
                .ToArray();

            var communicationResults = await Task.WhenAll(communicationTasks);

            // Step 5: Aggregate results and finalize submission
            logger.LogInformation("Step 5: Finalizing submission processing");
            var finalizeResult = await context.CallActivityAsync<bool>("FinalizeSubmissionActivity", 
                new FinalizeSubmissionActivityInput 
                { 
                    SubmissionId = submissionId, 
                    CommunicationResults = communicationResults.ToList() 
                });

            // Step 6: Raise domain event for communications stored
            if (finalizeResult)
            {
                logger.LogInformation("Step 6: Raising communications stored event");
                await context.CallActivityAsync("RaiseCommunicationsStoredEventActivity", submissionId);
            }

            result.IsSuccess = finalizeResult;
            result.CommunicationCount = extractionResult.CommunicationIds.Count;
            result.SuccessfulCommunications = communicationResults.Count(r => r.IsSuccess);
            result.FailedCommunications = communicationResults.Count(r => !r.IsSuccess);
            result.CompletedAt = context.CurrentUtcDateTime;

            logger.LogInformation("Submission processing orchestration completed for ID: {SubmissionId}. Success: {IsSuccess}", 
                submissionId, result.IsSuccess);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in submission processing orchestration");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.CompletedAt = context.CurrentUtcDateTime;
        }

        return result;
    }
}

public class SubmissionProcessingResult
{
    public Guid SubmissionId { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ValidationError> ValidationErrors { get; set; } = new();
    public int CommunicationCount { get; set; }
    public int SuccessfulCommunications { get; set; }
    public int FailedCommunications { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class BlobDownloadResult
{
    public bool IsSuccess { get; set; }
    public string XmlContent { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class XmlValidationActivityInput
{
    public Guid SubmissionId { get; set; }
    public string XmlContent { get; set; } = string.Empty;
}

public class XmlValidationActivityResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> ValidationErrors { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class CommunicationExtractionActivityInput
{
    public Guid SubmissionId { get; set; }
    public string XmlContent { get; set; } = string.Empty;
}

public class CommunicationExtractionActivityResult
{
    public bool IsSuccess { get; set; }
    public List<Guid> CommunicationIds { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class CommunicationProcessingResult
{
    public Guid CommunicationId { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

public class FinalizeSubmissionActivityInput
{
    public Guid SubmissionId { get; set; }
    public List<CommunicationProcessingResult> CommunicationResults { get; set; } = new();
}