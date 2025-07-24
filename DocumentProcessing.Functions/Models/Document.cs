namespace DocumentProcessing.Functions.Models;

public class Document
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string XmlContent { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ValidationError> ValidationErrors { get; set; } = new();
}

public enum DocumentStatus
{
    Pending,
    Validating,
    ValidationFailed,
    Validated,
    Printing,
    PrintingFailed,
    Completed
}

public class ValidationError
{
    public string Message { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
    public int? ColumnNumber { get; set; }
    public string? Severity { get; set; }
}