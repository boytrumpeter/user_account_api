using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using MediatR;
using DocumentProcessing.Functions.Commands;
using DocumentProcessing.Functions.Queries;

namespace DocumentProcessing.Functions.Functions;

public class SubmissionFunctions
{
    private readonly IMediator _mediator;
    private readonly ILogger<SubmissionFunctions> _logger;

    public SubmissionFunctions(IMediator mediator, ILogger<SubmissionFunctions> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [Function("ProcessSubmission")]
    public async Task<HttpResponseData> ProcessSubmission(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "submissions/process")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Processing submission request");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<ProcessSubmissionRequest>(requestBody, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null || string.IsNullOrWhiteSpace(request.BlobUrl))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid request. BlobUrl is required.");
                return badResponse;
            }

            var command = new ProcessSubmissionCommand
            {
                BlobUrl = request.BlobUrl,
                FileName = request.FileName ?? Path.GetFileName(new Uri(request.BlobUrl).LocalPath)
            };

            var result = await _mediator.Send(command);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            
            var jsonResponse = JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            await response.WriteStringAsync(jsonResponse);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing submission");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Internal server error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("GetSubmissionStatus")]
    public async Task<HttpResponseData> GetSubmissionStatus(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "submissions/{submissionId}/status")] HttpRequestData req,
        string submissionId)
    {
        try
        {
            _logger.LogInformation("Getting submission status for ID: {SubmissionId}", submissionId);

            if (!Guid.TryParse(submissionId, out var submissionGuid))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid submission ID format.");
                return badResponse;
            }

            var query = new GetSubmissionStatusQuery { SubmissionId = submissionGuid };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Submission not found.");
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            
            var jsonResponse = JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            await response.WriteStringAsync(jsonResponse);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submission status");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Internal server error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("GetSubmissionStatusHistory")]
    public async Task<HttpResponseData> GetSubmissionStatusHistory(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "submissions/{submissionId}/status/history")] HttpRequestData req,
        string submissionId)
    {
        try
        {
            _logger.LogInformation("Getting submission status history for ID: {SubmissionId}", submissionId);

            if (!Guid.TryParse(submissionId, out var submissionGuid))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid submission ID format.");
                return badResponse;
            }

            var query = new GetSubmissionStatusHistoryQuery { SubmissionId = submissionGuid };
            var result = await _mediator.Send(query);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            
            var jsonResponse = JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            await response.WriteStringAsync(jsonResponse);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submission status history");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Internal server error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("GetCommunications")]
    public async Task<HttpResponseData> GetCommunications(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "submissions/{submissionId}/communications")] HttpRequestData req,
        string submissionId)
    {
        try
        {
            _logger.LogInformation("Getting communications for submission ID: {SubmissionId}", submissionId);

            if (!Guid.TryParse(submissionId, out var submissionGuid))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid submission ID format.");
                return badResponse;
            }

            var query = new GetCommunicationsQuery { SubmissionId = submissionGuid };
            var result = await _mediator.Send(query);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            
            var jsonResponse = JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            await response.WriteStringAsync(jsonResponse);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting communications");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Internal server error: {ex.Message}");
            return errorResponse;
        }
    }
}

public class ProcessSubmissionRequest
{
    public string BlobUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
}