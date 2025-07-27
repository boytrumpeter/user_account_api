namespace DocumentProcessing.Functions.Models.Domain;

public class Submission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BlobUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string XmlContent { get; set; } = string.Empty;
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Received;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ValidationError> ValidationErrors { get; set; } = new();
    public List<Communication> Communications { get; set; } = new();
    
    // Domain Events
    public List<DomainEvent> DomainEvents { get; private set; } = new();
    
    public void AddDomainEvent(DomainEvent domainEvent)
    {
        DomainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        DomainEvents.Clear();
    }
    
    public void MarkAsValidated()
    {
        Status = SubmissionStatus.Validated;
        ProcessedAt = DateTime.UtcNow;
        AddDomainEvent(new SubmissionValidatedEvent(Id));
    }
    
    public void MarkAsValidationFailed(List<ValidationError> errors)
    {
        Status = SubmissionStatus.ValidationFailed;
        ValidationErrors = errors;
        ProcessedAt = DateTime.UtcNow;
        AddDomainEvent(new SubmissionValidationFailedEvent(Id, errors));
    }
}

public enum SubmissionStatus
{
    Received,
    Processing,
    Validated,
    ValidationFailed,
    CommunicationsExtracted,
    Completed,
    Failed
}

public class ValidationError
{
    public string Message { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
    public int? ColumnNumber { get; set; }
    public string Severity { get; set; } = "Error";
}