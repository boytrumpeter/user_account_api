namespace DocumentProcessing.Functions.Models.Domain;

public class Communication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SubmissionId { get; set; }
    public string DocumentContent { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public CommunicationStatus Status { get; set; } = CommunicationStatus.Extracted;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public List<ValidationError> ValidationErrors { get; set; } = new();
    
    // Navigation property
    public Submission? Submission { get; set; }
    
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
        Status = CommunicationStatus.Validated;
        ProcessedAt = DateTime.UtcNow;
        AddDomainEvent(new CommunicationValidatedEvent(Id, SubmissionId));
    }
    
    public void MarkAsValidationFailed(List<ValidationError> errors)
    {
        Status = CommunicationStatus.ValidationFailed;
        ValidationErrors = errors;
        ProcessedAt = DateTime.UtcNow;
        AddDomainEvent(new CommunicationValidationFailedEvent(Id, SubmissionId, errors));
    }
    
    public void MarkAsStored()
    {
        Status = CommunicationStatus.Stored;
        ProcessedAt = DateTime.UtcNow;
        AddDomainEvent(new CommunicationStoredEvent(Id, SubmissionId));
    }
}

public enum CommunicationStatus
{
    Extracted,
    Validating,
    Validated,
    ValidationFailed,
    Storing,
    Stored,
    Failed
}