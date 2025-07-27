using DocumentProcessing.Functions.Models.Domain;
using DocumentProcessing.Functions.Models.Domain.Aggregates;

namespace DocumentProcessing.Functions.Infrastructure.Repositories;

public interface ICommunicationRepository
{
    Task<CommunicationAggregate?> GetByIdAsync(Guid id);
    Task<List<CommunicationAggregate>> GetBySubmissionIdAsync(Guid submissionId);
    Task<CommunicationAggregate> AddAsync(CommunicationAggregate aggregate);
    Task UpdateAsync(CommunicationAggregate aggregate);
    Task<List<CommunicationStatusEntry>> GetStatusHistoryAsync(Guid communicationId);
    Task AddStatusEntryAsync(CommunicationStatusEntry statusEntry);
    Task SaveChangesAsync();
}