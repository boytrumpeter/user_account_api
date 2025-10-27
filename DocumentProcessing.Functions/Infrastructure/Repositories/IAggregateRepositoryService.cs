using DocumentProcessing.Functions.Models.Domain.Aggregates;

namespace DocumentProcessing.Functions.Infrastructure.Repositories;

/// <summary>
/// Service responsible for reconstructing aggregates from persistence.
/// This is separate from individual repositories to maintain SRP.
/// </summary>
public interface IAggregateRepositoryService
{
    Task<SubmissionAggregate?> ReconstructSubmissionAggregateAsync(Guid submissionId);
    Task<CommunicationAggregate?> ReconstructCommunicationAggregateAsync(Guid communicationId);
}