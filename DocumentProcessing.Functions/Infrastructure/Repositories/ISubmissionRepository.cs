using DocumentProcessing.Functions.Models.Domain;
using DocumentProcessing.Functions.Models.Domain.Aggregates;

namespace DocumentProcessing.Functions.Infrastructure.Repositories;

public interface ISubmissionRepository
{
    Task<SubmissionAggregate?> GetByIdAsync(Guid id);
    Task<SubmissionAggregate> AddAsync(SubmissionAggregate aggregate);
    Task UpdateAsync(SubmissionAggregate aggregate);
    Task<List<SubmissionStatusEntry>> GetStatusHistoryAsync(Guid submissionId);
    Task AddStatusEntryAsync(SubmissionStatusEntry statusEntry);
    Task SaveChangesAsync();
}