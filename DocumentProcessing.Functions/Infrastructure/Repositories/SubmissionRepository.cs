using Microsoft.EntityFrameworkCore;
using DocumentProcessing.Functions.Models.Domain;
using DocumentProcessing.Functions.Models.Domain.Aggregates;

namespace DocumentProcessing.Functions.Infrastructure.Repositories;

public class SubmissionRepository : ISubmissionRepository, IAggregateRepository
{
    private readonly DocumentProcessingDbContext _context;

    public SubmissionRepository(DocumentProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<SubmissionAggregate?> GetByIdAsync(Guid id)
    {
        return await ReconstructSubmissionAggregateAsync(id);
    }

    public async Task<SubmissionAggregate?> ReconstructSubmissionAggregateAsync(Guid submissionId)
    {
        var submission = await _context.Submissions
            .Include(s => s.Communications)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        return submission != null ? new SubmissionAggregate(submission) : null;
    }

    public async Task<CommunicationAggregate?> ReconstructCommunicationAggregateAsync(Guid communicationId)
    {
        var communication = await _context.Communications
            .FirstOrDefaultAsync(c => c.Id == communicationId);

        return communication != null ? new CommunicationAggregate(communication) : null;
    }

    public async Task<SubmissionAggregate> AddAsync(SubmissionAggregate aggregate)
    {
        var submission = aggregate.Submission;
        await _context.Submissions.AddAsync(submission);
        
        // Add status entries
        foreach (var statusEntry in aggregate.StatusEntries)
        {
            await _context.SubmissionStatusEntries.AddAsync(statusEntry);
        }

        return aggregate;
    }

    public async Task UpdateAsync(SubmissionAggregate aggregate)
    {
        var submission = aggregate.Submission;
        _context.Submissions.Update(submission);
        
        // Add new status entries
        foreach (var statusEntry in aggregate.StatusEntries)
        {
            // Check if status entry already exists to avoid duplicates
            var existsInDb = await _context.SubmissionStatusEntries
                .AnyAsync(s => s.Id == statusEntry.Id);
            
            if (!existsInDb)
            {
                await _context.SubmissionStatusEntries.AddAsync(statusEntry);
            }
        }
    }

    public async Task<List<SubmissionStatusEntry>> GetStatusHistoryAsync(Guid submissionId)
    {
        return await _context.SubmissionStatusEntries
            .Where(s => s.SubmissionId == submissionId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task AddStatusEntryAsync(SubmissionStatusEntry statusEntry)
    {
        await _context.SubmissionStatusEntries.AddAsync(statusEntry);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}