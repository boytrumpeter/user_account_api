using DocumentProcessing.Functions.Models;

namespace DocumentProcessing.Functions.Commands;

public class ProcessDocumentsCommand
{
    public string BatchId { get; set; } = string.Empty;
    public List<Document> Documents { get; set; } = new();
    public string? CallbackUrl { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ValidateDocumentCommand
{
    public string DocumentId { get; set; } = string.Empty;
    public string XmlContent { get; set; } = string.Empty;
}

public class SendToPrintingCommand
{
    public string DocumentId { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string XmlContent { get; set; } = string.Empty;
}

public class ValidationResult
{
    public string DocumentId { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

public class PrintingResult
{
    public string DocumentId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public bool IsSuccessful { get; set; }
    public string? JobId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}