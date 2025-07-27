namespace DocumentProcessing.Functions.Models.Domain.Aggregates;

public class CommunicationAggregate
{
    private readonly Communication _communication;
    private readonly List<CommunicationStatusEntry> _statusEntries;

    public CommunicationAggregate(Communication communication)
    {
        _communication = communication ?? throw new ArgumentNullException(nameof(communication));
        _statusEntries = new List<CommunicationStatusEntry>();
    }

    public Guid Id => _communication.Id;
    public Communication Communication => _communication;
    public IReadOnlyList<CommunicationStatusEntry> StatusEntries => _statusEntries.AsReadOnly();

    public void ValidateContent()
    {
        _communication.Status = CommunicationStatus.Validating;
        AddStatusEntry(CommunicationStatus.Validating, "Started communication validation");
    }

    public void MarkAsValidated()
    {
        _communication.MarkAsValidated();
        AddStatusEntry(CommunicationStatus.Validated, "Communication validation completed successfully");
    }

    public void MarkAsValidationFailed(List<ValidationError> errors)
    {
        _communication.MarkAsValidationFailed(errors);
        AddStatusEntry(CommunicationStatus.ValidationFailed, "Communication validation failed",
            $"Validation errors: {string.Join(", ", errors.Select(e => e.Message))}");
    }

    public void Store()
    {
        _communication.Status = CommunicationStatus.Storing;
        AddStatusEntry(CommunicationStatus.Storing, "Storing communication in database");
    }

    public void MarkAsStored()
    {
        _communication.MarkAsStored();
        AddStatusEntry(CommunicationStatus.Stored, "Communication stored successfully");
    }

    public void MarkAsFailed(string errorMessage)
    {
        _communication.Status = CommunicationStatus.Failed;
        AddStatusEntry(CommunicationStatus.Failed, "Communication processing failed", errorMessage);
    }

    private void AddStatusEntry(CommunicationStatus status, string description, string? details = null)
    {
        var statusEntry = new CommunicationStatusEntry
        {
            CommunicationId = _communication.Id,
            SubmissionId = _communication.SubmissionId,
            Status = status,
            StatusDescription = description,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };
        
        _statusEntries.Add(statusEntry);
    }

    public List<DomainEvent> GetDomainEvents()
    {
        return _communication.DomainEvents.ToList();
    }

    public void ClearDomainEvents()
    {
        _communication.ClearDomainEvents();
    }
}