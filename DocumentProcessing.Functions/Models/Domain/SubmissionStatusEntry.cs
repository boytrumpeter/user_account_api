namespace DocumentProcessing.Functions.Models.Domain;

public class SubmissionStatusEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SubmissionId { get; set; }
    public SubmissionStatus Status { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Navigation property
    public Submission? Submission { get; set; }
}

public class CommunicationStatusEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CommunicationId { get; set; }
    public Guid SubmissionId { get; set; }
    public CommunicationStatus Status { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public Communication? Communication { get; set; }
    public Submission? Submission { get; set; }
}