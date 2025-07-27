using MediatR;
using DocumentProcessing.Functions.Models.Domain;

namespace DocumentProcessing.Functions.Queries;

public class GetSubmissionStatusQuery : IRequest<SubmissionStatusResponse?>
{
    public Guid SubmissionId { get; set; }
}

public class GetSubmissionStatusHistoryQuery : IRequest<List<SubmissionStatusEntry>>
{
    public Guid SubmissionId { get; set; }
}

public class GetCommunicationsQuery : IRequest<List<CommunicationResponse>>
{
    public Guid SubmissionId { get; set; }
}

public class SubmissionStatusResponse
{
    public Guid Id { get; set; }
    public string BlobUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public SubmissionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ValidationError> ValidationErrors { get; set; } = new();
    public int CommunicationCount { get; set; }
}

public class CommunicationResponse
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public CommunicationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public List<ValidationError> ValidationErrors { get; set; } = new();
}