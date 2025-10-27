namespace DocumentProcessing.Functions.Models.Domain.Aggregates;

public class SubmissionAggregate
{
    private readonly Submission _submission;
    private readonly List<SubmissionStatusEntry> _statusEntries;

    public SubmissionAggregate(Submission submission)
    {
        _submission = submission ?? throw new ArgumentNullException(nameof(submission));
        _statusEntries = new List<SubmissionStatusEntry>();
    }

    public Guid Id => _submission.Id;
    public Submission Submission => _submission;
    public IReadOnlyList<SubmissionStatusEntry> StatusEntries => _statusEntries.AsReadOnly();

    public void ValidateXmlContent()
    {
        _submission.Status = SubmissionStatus.Processing;
        AddStatusEntry(SubmissionStatus.Processing, "Started XML validation");
        
        // Domain logic for validation will be handled by the service
    }

    public void MarkAsValidated()
    {
        _submission.MarkAsValidated();
        AddStatusEntry(SubmissionStatus.Validated, "XML validation completed successfully");
    }

    public void MarkAsValidationFailed(List<ValidationError> errors)
    {
        _submission.MarkAsValidationFailed(errors);
        AddStatusEntry(SubmissionStatus.ValidationFailed, "XML validation failed", 
            $"Validation errors: {string.Join(", ", errors.Select(e => e.Message))}");
    }

    public void ExtractCommunications(List<Communication> communications)
    {
        _submission.Communications.AddRange(communications);
        _submission.Status = SubmissionStatus.CommunicationsExtracted;
        AddStatusEntry(SubmissionStatus.CommunicationsExtracted, 
            $"Extracted {communications.Count} communications from XML");
    }

    public void MarkAsCompleted()
    {
        _submission.Status = SubmissionStatus.Completed;
        _submission.ProcessedAt = DateTime.UtcNow;
        AddStatusEntry(SubmissionStatus.Completed, "Submission processing completed successfully");
    }

    public void MarkAsFailed(string errorMessage)
    {
        _submission.Status = SubmissionStatus.Failed;
        _submission.ErrorMessage = errorMessage;
        _submission.ProcessedAt = DateTime.UtcNow;
        AddStatusEntry(SubmissionStatus.Failed, "Submission processing failed", errorMessage);
    }

    private void AddStatusEntry(SubmissionStatus status, string description, string? details = null)
    {
        var statusEntry = new SubmissionStatusEntry
        {
            SubmissionId = _submission.Id,
            Status = status,
            StatusDescription = description,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };
        
        _statusEntries.Add(statusEntry);
    }

    public List<DomainEvent> GetDomainEvents()
    {
        return _submission.DomainEvents.ToList();
    }

    public void ClearDomainEvents()
    {
        _submission.ClearDomainEvents();
    }
}