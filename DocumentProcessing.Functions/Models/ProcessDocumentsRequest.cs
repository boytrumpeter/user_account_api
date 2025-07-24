namespace DocumentProcessing.Functions.Models;

public class ProcessDocumentsRequest
{
    public string BatchId { get; set; } = Guid.NewGuid().ToString();
    public List<Document> Documents { get; set; } = new();
    public string? CallbackUrl { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ProcessDocumentsResponse
{
    public string BatchId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<DocumentProcessingResult> Results { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class DocumentProcessingResult
{
    public string DocumentId { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public bool IsValid { get; set; }
    public List<ValidationError> ValidationErrors { get; set; } = new();
    public bool PrintingSuccessful { get; set; }
    public string? PrintingJobId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime? ProcessedAt { get; set; }
}