using DocumentProcessing.Functions.Models;
using DocumentProcessing.Functions.Infrastructure;

namespace DocumentProcessing.Functions.Queries;

public class GetProcessingStatusQuery : IQuery<ProcessingStatusResponse>
{
    public string BatchId { get; set; } = string.Empty;
}

public class GetDocumentStatusQuery : IQuery<DocumentProcessingResult?>
{
    public string DocumentId { get; set; } = string.Empty;
}

public class ProcessingStatusResponse
{
    public string BatchId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalDocuments { get; set; }
    public int ProcessedDocuments { get; set; }
    public int ValidDocuments { get; set; }
    public int InvalidDocuments { get; set; }
    public int SuccessfulPrints { get; set; }
    public int FailedPrints { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<DocumentProcessingResult> DocumentResults { get; set; } = new();
}