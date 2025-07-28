using DocumentProcessing.Functions.Models.Domain.Aggregates;

namespace DocumentProcessing.Functions.Models.Domain;

public interface IAggregateFactory
{
    // Factory methods - create new aggregates from primitive values
    SubmissionAggregate CreateNewSubmission(string blobUrl, string fileName);
    CommunicationAggregate CreateNewCommunication(Guid submissionId, string recipient, string subject, string content, string documentType);
}

public interface IAggregateRepository
{
    // Reconstruction methods - rebuild aggregates from persistence
    Task<SubmissionAggregate?> ReconstructSubmissionAggregateAsync(Guid submissionId);
    Task<CommunicationAggregate?> ReconstructCommunicationAggregateAsync(Guid communicationId);
}

public class AggregateFactory : IAggregateFactory
{
    public SubmissionAggregate CreateNewSubmission(string blobUrl, string fileName)
    {
        if (string.IsNullOrWhiteSpace(blobUrl))
            throw new ArgumentException("Blob URL cannot be empty", nameof(blobUrl));
        
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            BlobUrl = blobUrl,
            FileName = fileName,
            Status = SubmissionStatus.Received,
            CreatedAt = DateTime.UtcNow
        };

        return new SubmissionAggregate(submission);
    }

    public CommunicationAggregate CreateNewCommunication(Guid submissionId, string recipient, string subject, string content, string documentType)
    {
        if (submissionId == Guid.Empty)
            throw new ArgumentException("Submission ID cannot be empty", nameof(submissionId));
        
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient cannot be empty", nameof(recipient));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        var communication = new Communication
        {
            Id = Guid.NewGuid(),
            SubmissionId = submissionId,
            Recipient = recipient,
            Subject = subject ?? string.Empty,
            DocumentContent = content,
            DocumentType = documentType ?? "document",
            Status = CommunicationStatus.Extracted,
            CreatedAt = DateTime.UtcNow
        };

        return new CommunicationAggregate(communication);
    }
}