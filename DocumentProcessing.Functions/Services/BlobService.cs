using Azure.Storage.Blobs;

namespace DocumentProcessing.Functions.Services;

public class BlobService : IBlobService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BlobService> _logger;

    public BlobService(HttpClient httpClient, ILogger<BlobService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> DownloadXmlFromBlobAsync(string blobUrl)
    {
        try
        {
            _logger.LogInformation("Downloading XML from blob URL: {BlobUrl}", blobUrl);

            // If it's an Azure blob URL, use BlobClient
            if (blobUrl.Contains("blob.core.windows.net"))
            {
                return await DownloadFromAzureBlobAsync(blobUrl);
            }
            
            // For other third-party blob storage (e.g., AWS S3, Google Cloud Storage)
            return await DownloadFromHttpAsync(blobUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading XML from blob URL: {BlobUrl}", blobUrl);
            throw new InvalidOperationException($"Failed to download XML from blob: {ex.Message}", ex);
        }
    }

    public async Task<bool> ValidateBlobAccessAsync(string blobUrl)
    {
        try
        {
            if (blobUrl.Contains("blob.core.windows.net"))
            {
                var blobClient = new BlobClient(new Uri(blobUrl));
                var exists = await blobClient.ExistsAsync();
                return exists.Value;
            }
            
            // For HTTP-based blob storage, try HEAD request
            var request = new HttpRequestMessage(HttpMethod.Head, blobUrl);
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating blob access: {BlobUrl}", blobUrl);
            return false;
        }
    }

    private async Task<string> DownloadFromAzureBlobAsync(string blobUrl)
    {
        var blobClient = new BlobClient(new Uri(blobUrl));
        
        var response = await blobClient.DownloadContentAsync();
        return response.Value.Content.ToString();
    }

    private async Task<string> DownloadFromHttpAsync(string blobUrl)
    {
        var response = await _httpClient.GetAsync(blobUrl);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync();
    }
}