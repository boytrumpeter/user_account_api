namespace DocumentProcessing.Functions.Services;

public interface IBlobService
{
    Task<string> DownloadXmlFromBlobAsync(string blobUrl);
    Task<bool> ValidateBlobAccessAsync(string blobUrl);
}

public class BlobDownloadResult
{
    public string XmlContent { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}