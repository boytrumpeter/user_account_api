using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using DocumentProcessing.Functions.Models;
using DocumentProcessing.Functions.Commands;

namespace DocumentProcessing.Functions.Orchestrators;

public static class DocumentProcessingOrchestrator
{
    [Function("DocumentProcessingOrchestrator")]
    public static async Task<ProcessDocumentsResponse> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("DocumentProcessingOrchestrator");
        var input = context.GetInput<ProcessDocumentsRequest>()!;
        
        logger.LogInformation("Starting document processing orchestration for batch {BatchId} with {DocumentCount} documents", 
            input.BatchId, input.Documents.Count);

        var response = new ProcessDocumentsResponse
        {
            BatchId = input.BatchId,
            Status = "Processing",
            StartedAt = DateTime.UtcNow,
            Results = new List<DocumentProcessingResult>()
        };

        try
        {
            // Fan-out: Process all documents in parallel using activities
            var validationTasks = new List<Task<ValidationResult>>();
            
            foreach (var document in input.Documents)
            {
                var validationTask = context.CallActivityAsync<ValidationResult>(
                    "ValidateDocumentActivity", 
                    new ValidateDocumentInput { DocumentId = document.Id, XmlContent = document.XmlContent });
                validationTasks.Add(validationTask);
            }

            // Fan-in: Wait for all validation tasks to complete
            var validationResults = await Task.WhenAll(validationTasks);
            
            logger.LogInformation("Validation completed for {DocumentCount} documents. Valid: {ValidCount}, Invalid: {InvalidCount}",
                input.Documents.Count,
                validationResults.Count(r => r.IsValid),
                validationResults.Count(r => !r.IsValid));

            // Process valid documents for printing
            var printingTasks = new List<Task<PrintingResult>>();
            var documentProcessingResults = new List<DocumentProcessingResult>();

            foreach (var validationResult in validationResults)
            {
                var document = input.Documents.First(d => d.Id == validationResult.DocumentId);
                var documentResult = new DocumentProcessingResult
                {
                    DocumentId = document.Id,
                    DocumentName = document.Name,
                    IsValid = validationResult.IsValid,
                    ValidationErrors = validationResult.Errors
                };

                if (validationResult.IsValid)
                {
                    documentResult.Status = DocumentStatus.Validated;
                    
                    // Fan-out: Send valid documents to printing in parallel
                    var printingTask = context.CallActivityAsync<PrintingResult>(
                        "SendToPrintingActivity",
                        new SendToPrintingInput 
                        { 
                            DocumentId = document.Id,
                            DocumentName = document.Name,
                            XmlContent = document.XmlContent 
                        });
                    printingTasks.Add(printingTask);
                }
                else
                {
                    documentResult.Status = DocumentStatus.ValidationFailed;
                    documentResult.PrintingSuccessful = false;
                }

                documentProcessingResults.Add(documentResult);
            }

            // Fan-in: Wait for all printing tasks to complete
            if (printingTasks.Count > 0)
            {
                var printingResults = await Task.WhenAll(printingTasks);
                
                logger.LogInformation("Printing completed for {DocumentCount} documents. Successful: {SuccessfulCount}, Failed: {FailedCount}",
                    printingTasks.Count,
                    printingResults.Count(r => r.IsSuccessful),
                    printingResults.Count(r => !r.IsSuccessful));

                // Update results with printing information
                foreach (var printingResult in printingResults)
                {
                    var documentResult = documentProcessingResults.First(r => r.DocumentId == printingResult.DocumentId);
                    documentResult.PrintingSuccessful = printingResult.IsSuccessful;
                    documentResult.PrintingJobId = printingResult.JobId;
                    documentResult.Status = printingResult.IsSuccessful 
                        ? DocumentStatus.Completed 
                        : DocumentStatus.PrintingFailed;
                    
                    if (!printingResult.IsSuccessful)
                    {
                        documentResult.ErrorMessage = printingResult.ErrorMessage;
                    }
                }
            }

            response.Results = documentProcessingResults;
            response.Status = "Completed";
            response.CompletedAt = DateTime.UtcNow;

            logger.LogInformation("Document processing orchestration completed for batch {BatchId}. " +
                "Total: {TotalCount}, Valid: {ValidCount}, Printed: {PrintedCount}",
                input.BatchId,
                documentProcessingResults.Count,
                documentProcessingResults.Count(r => r.IsValid),
                documentProcessingResults.Count(r => r.PrintingSuccessful));

            // Send callback notification if configured
            if (!string.IsNullOrEmpty(input.CallbackUrl))
            {
                await context.CallActivityAsync("SendCallbackNotificationActivity", 
                    new CallbackNotificationInput { CallbackUrl = input.CallbackUrl, Response = response });
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in document processing orchestration for batch {BatchId}", input.BatchId);
            
            response.Status = "Failed";
            response.CompletedAt = DateTime.UtcNow;
            
            return response;
        }
    }
}

public class ValidateDocumentInput
{
    public string DocumentId { get; set; } = string.Empty;
    public string XmlContent { get; set; } = string.Empty;
}

public class SendToPrintingInput
{
    public string DocumentId { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string XmlContent { get; set; } = string.Empty;
}

public class CallbackNotificationInput
{
    public string CallbackUrl { get; set; } = string.Empty;
    public ProcessDocumentsResponse Response { get; set; } = new();
}