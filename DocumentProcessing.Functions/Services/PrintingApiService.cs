using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocumentProcessing.Functions.Services;

public class PrintingApiService : IPrintingApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PrintingApiService> _logger;

    public PrintingApiService(HttpClient httpClient, IConfiguration configuration, ILogger<PrintingApiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> SubmitDocumentForPrintingAsync(string documentId, string documentName, string xmlContent)
    {
        try
        {
            var baseUrl = _configuration["PrintingApiBaseUrl"];
            var apiKey = _configuration["PrintingApiKey"];

            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("PrintingApiBaseUrl configuration is missing");
            }

            var request = new PrintingRequest
            {
                DocumentId = documentId,
                DocumentName = documentName,
                Content = xmlContent,
                Format = "XML",
                Priority = "Normal"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add API key if configured
            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            }

            _logger.LogInformation("Submitting document {DocumentId} for printing", documentId);

            var response = await _httpClient.PostAsync($"{baseUrl}/api/print", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var printingResponse = JsonSerializer.Deserialize<PrintingResponse>(responseContent);
                
                _logger.LogInformation("Document {DocumentId} submitted successfully. JobId: {JobId}", 
                    documentId, printingResponse?.JobId);
                
                return printingResponse?.JobId ?? Guid.NewGuid().ToString();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to submit document {DocumentId} for printing. Status: {StatusCode}, Error: {Error}", 
                    documentId, response.StatusCode, errorContent);
                
                throw new HttpRequestException($"Printing API returned {response.StatusCode}: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting document {DocumentId} for printing", documentId);
            throw;
        }
    }

    public async Task<PrintingJobStatus> GetJobStatusAsync(string jobId)
    {
        try
        {
            var baseUrl = _configuration["PrintingApiBaseUrl"];
            var apiKey = _configuration["PrintingApiKey"];

            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("PrintingApiBaseUrl configuration is missing");
            }

            // Add API key if configured
            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            }

            var response = await _httpClient.GetAsync($"{baseUrl}/api/print/{jobId}/status");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var status = JsonSerializer.Deserialize<PrintingJobStatusResponse>(content);
                
                return new PrintingJobStatus
                {
                    JobId = jobId,
                    Status = status?.Status ?? "Unknown",
                    CreatedAt = status?.CreatedAt ?? DateTime.UtcNow,
                    CompletedAt = status?.CompletedAt,
                    ErrorMessage = status?.ErrorMessage
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to get status for job {JobId}. Status: {StatusCode}, Error: {Error}", 
                    jobId, response.StatusCode, errorContent);
                
                return new PrintingJobStatus
                {
                    JobId = jobId,
                    Status = "Unknown",
                    CreatedAt = DateTime.UtcNow,
                    ErrorMessage = $"Failed to retrieve status: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status for job {JobId}", jobId);
            return new PrintingJobStatus
            {
                JobId = jobId,
                Status = "Error",
                CreatedAt = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }
}

public class PrintingRequest
{
    public string DocumentId { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

public class PrintingResponse
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PrintingJobStatusResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}