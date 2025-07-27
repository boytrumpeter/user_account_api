namespace DocumentProcessing.Functions.Models.Domain;

public abstract class DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Submission Events
public class SubmissionValidatedEvent : DomainEvent
{
    public Guid SubmissionId { get; }
    
    public SubmissionValidatedEvent(Guid submissionId)
    {
        SubmissionId = submissionId;
    }
}

public class SubmissionValidationFailedEvent : DomainEvent
{
    public Guid SubmissionId { get; }
    public List<ValidationError> ValidationErrors { get; }
    
    public SubmissionValidationFailedEvent(Guid submissionId, List<ValidationError> validationErrors)
    {
        SubmissionId = submissionId;
        ValidationErrors = validationErrors;
    }
}

// Communication Events
public class CommunicationValidatedEvent : DomainEvent
{
    public Guid CommunicationId { get; }
    public Guid SubmissionId { get; }
    
    public CommunicationValidatedEvent(Guid communicationId, Guid submissionId)
    {
        CommunicationId = communicationId;
        SubmissionId = submissionId;
    }
}

public class CommunicationValidationFailedEvent : DomainEvent
{
    public Guid CommunicationId { get; }
    public Guid SubmissionId { get; }
    public List<ValidationError> ValidationErrors { get; }
    
    public CommunicationValidationFailedEvent(Guid communicationId, Guid submissionId, List<ValidationError> validationErrors)
    {
        CommunicationId = communicationId;
        SubmissionId = submissionId;
        ValidationErrors = validationErrors;
    }
}

public class CommunicationStoredEvent : DomainEvent
{
    public Guid CommunicationId { get; }
    public Guid SubmissionId { get; }
    
    public CommunicationStoredEvent(Guid communicationId, Guid submissionId)
    {
        CommunicationId = communicationId;
        SubmissionId = submissionId;
    }
}

public class CommunicationsStoredEvent : DomainEvent
{
    public Guid SubmissionId { get; }
    public List<Guid> CommunicationIds { get; }
    
    public CommunicationsStoredEvent(Guid submissionId, List<Guid> communicationIds)
    {
        SubmissionId = submissionId;
        CommunicationIds = communicationIds;
    }
}