using MediatR;

namespace DocumentProcessing.Functions.Commands;

public class ProcessSubmissionCommand : IRequest<ProcessSubmissionResponse>
{
    public string BlobUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class ProcessSubmissionResponse
{
    public Guid SubmissionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}