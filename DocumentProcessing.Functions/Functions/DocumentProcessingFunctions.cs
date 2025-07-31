using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using DocumentProcessing.Functions.Commands;
using DocumentProcessing.Functions.Commands.Handlers;
using DocumentProcessing.Functions.Queries;
using DocumentProcessing.Functions.Queries.Handlers;
using DocumentProcessing.Functions.Models;

namespace DocumentProcessing.Functions.Functions;

public class DocumentProcessingFunctions
{
    private readonly ProcessDocumentsCommandHandler _processDocumentsHandler;
    private readonly ValidateDocumentCommandHandler _validateDocumentHandler;
    private readonly GetProcessingStatusQueryHandler _getProcessingStatusHandler;
    private readonly GetDocumentStatusQueryHandler _getDocumentStatusHandler;
    private readonly ILogger<DocumentProcessingFunctions> _logger;

    public DocumentProcessingFunctions(
        ProcessDocumentsCommandHandler processDocumentsHandler,
        ValidateDocumentCommandHandler validateDocumentHandler,
        GetProcessingStatusQueryHandler getProcessingStatusHandler,
        GetDocumentStatusQueryHandler getDocumentStatusHandler,
        ILogger<DocumentProcessingFunctions> logger)
    {
        _processDocumentsHandler = processDocumentsHandler;
        _validateDocumentHandler = validateDocumentHandler;
        _getProcessingStatusHandler = getProcessingStatusHandler;
        _getDocumentStatusHandler = getDocumentStatusHandler;
        _logger = logger;
    }

    [Function("ProcessDocuments")]
    public async Task<HttpResponseData> ProcessDocuments(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "documents/process")] HttpRequestData req)
    {
        _logger.LogInformation("ProcessDocuments HTTP trigger executed");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<ProcessDocumentsRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (request == null || !request.Documents.Any())
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request: No documents provided");
                return badRequestResponse;
            }

            var command = new ProcessDocumentsCommand
            {
                BatchId = request.BatchId,
                Documents = request.Documents,
                CallbackUrl = request.CallbackUrl,
                Metadata = request.Metadata
            };

            var result = await _processDocumentsHandler.ExecuteAsync(command);

            var response = req.CreateResponse(HttpStatusCode.Accepted);
            response.Headers.Add("Content-Type", "application/json");
            
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await response.WriteStringAsync(json);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing documents request");
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Internal server error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("GetProcessingStatus")]
    public async Task<HttpResponseData> GetProcessingStatus(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "documents/batch/{batchId}/status")] HttpRequestData req,
        string batchId)
    {
        _logger.LogInformation("GetProcessingStatus HTTP trigger executed for batch {BatchId}", batchId);

        try
        {
            var query = new GetProcessingStatusQuery { BatchId = batchId };
            var result = await _getProcessingStatusHandler.ExecuteAsync(query);

            if (result == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Batch {batchId} not found");
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await response.WriteStringAsync(json);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processing status for batch {BatchId}", batchId);
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Internal server error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("GetDocumentStatus")]
    public async Task<HttpResponseData> GetDocumentStatus(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "documents/{documentId}/status")] HttpRequestData req,
        string documentId)
    {
        _logger.LogInformation("GetDocumentStatus HTTP trigger executed for document {DocumentId}", documentId);

        try
        {
            var query = new GetDocumentStatusQuery { DocumentId = documentId };
            var result = await _getDocumentStatusHandler.ExecuteAsync(query);

            if (result == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Document {documentId} not found");
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await response.WriteStringAsync(json);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document status for document {DocumentId}", documentId);
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Internal server error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("ValidateXml")]
    public async Task<HttpResponseData> ValidateXml(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "documents/validate")] HttpRequestData req)
    {
        _logger.LogInformation("ValidateXml HTTP trigger executed");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<ValidateXmlRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (request == null || string.IsNullOrWhiteSpace(request.XmlContent))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request: XML content is required");
                return badRequestResponse;
            }

            var command = new ValidateDocumentCommand
            {
                DocumentId = request.DocumentId ?? Guid.NewGuid().ToString(),
                XmlContent = request.XmlContent
            };

            var result = await _validateDocumentHandler.ExecuteAsync(command);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await response.WriteStringAsync(json);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating XML");
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Internal server error: {ex.Message}");
            return errorResponse;
        }
    }
}

public class ValidateXmlRequest
{
    public string? DocumentId { get; set; }
    public string XmlContent { get; set; } = string.Empty;
}