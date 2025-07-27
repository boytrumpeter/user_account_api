using DocumentProcessing.Functions.Models.Domain.Aggregates;

namespace DocumentProcessing.Functions.Models.Domain;

public interface IAggregateFactory
{
    SubmissionAggregate CreateSubmissionAggregate(string blobUrl, string fileName);
    SubmissionAggregate CreateSubmissionAggregate(Submission submission);
    CommunicationAggregate CreateCommunicationAggregate(Communication communication);
}

public class AggregateFactory : IAggregateFactory
{
    public SubmissionAggregate CreateSubmissionAggregate(string blobUrl, string fileName)
    {
        var submission = new Submission
        {
            BlobUrl = blobUrl,
            FileName = fileName,
            Status = SubmissionStatus.Received,
            CreatedAt = DateTime.UtcNow
        };

        return new SubmissionAggregate(submission);
    }

    public SubmissionAggregate CreateSubmissionAggregate(Submission submission)
    {
        return new SubmissionAggregate(submission);
    }

    public CommunicationAggregate CreateCommunicationAggregate(Communication communication)
    {
        return new CommunicationAggregate(communication);
    }
}