namespace DocumentProcessing.Functions.Services;

public interface IPrintingApiService
{
    Task<string> SubmitDocumentForPrintingAsync(string documentId, string documentName, string xmlContent);
    Task<PrintingJobStatus> GetJobStatusAsync(string jobId);
}

public class PrintingJobStatus
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}