using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using DocumentProcessing.Functions.Commands;
using DocumentProcessing.Functions.Infrastructure;
using DocumentProcessing.Functions.Orchestrators;

namespace DocumentProcessing.Functions.Activities;

public class DocumentProcessingActivities
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly HttpClient _httpClient;
    private readonly ILogger<DocumentProcessingActivities> _logger;

    public DocumentProcessingActivities(
        ICommandDispatcher commandDispatcher,
        HttpClient httpClient, 
        ILogger<DocumentProcessingActivities> logger)
    {
        _commandDispatcher = commandDispatcher;
        _httpClient = httpClient;
        _logger = logger;
    }

    [Function("ValidateDocumentActivity")]
    public async Task<ValidationResult> ValidateDocument([ActivityTrigger] ValidateDocumentInput input)
    {
        _logger.LogInformation("Validating document {DocumentId}", input.DocumentId);

        var command = new ValidateDocumentCommand
        {
            DocumentId = input.DocumentId,
            XmlContent = input.XmlContent
        };

        var result = await _commandDispatcher.DispatchAsync(command);
        
        _logger.LogInformation("Document {DocumentId} validation completed. IsValid: {IsValid}", 
            input.DocumentId, result.IsValid);

        return result;
    }

    [Function("SendToPrintingActivity")]
    public async Task<PrintingResult> SendToPrinting([ActivityTrigger] SendToPrintingInput input)
    {
        _logger.LogInformation("Sending document {DocumentId} to printing", input.DocumentId);

        var command = new SendToPrintingCommand
        {
            DocumentId = input.DocumentId,
            DocumentName = input.DocumentName,
            XmlContent = input.XmlContent
        };

        var result = await _commandDispatcher.DispatchAsync(command);
        
        _logger.LogInformation("Document {DocumentId} printing submission completed. IsSuccessful: {IsSuccessful}, JobId: {JobId}", 
            input.DocumentId, result.IsSuccessful, result.JobId);

        return result;
    }

    [Function("SendCallbackNotificationActivity")]
    public async Task SendCallbackNotification([ActivityTrigger] CallbackNotificationInput input)
    {
        _logger.LogInformation("Sending callback notification to {CallbackUrl}", input.CallbackUrl);

        try
        {
            var json = JsonSerializer.Serialize(input.Response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(input.CallbackUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Callback notification sent successfully to {CallbackUrl}", input.CallbackUrl);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to send callback notification to {CallbackUrl}. Status: {StatusCode}, Error: {Error}",
                    input.CallbackUrl, response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending callback notification to {CallbackUrl}", input.CallbackUrl);
        }
    }
}