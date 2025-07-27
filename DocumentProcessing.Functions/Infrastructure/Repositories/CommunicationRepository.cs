using Microsoft.EntityFrameworkCore;
using DocumentProcessing.Functions.Models.Domain;
using DocumentProcessing.Functions.Models.Domain.Aggregates;

namespace DocumentProcessing.Functions.Infrastructure.Repositories;

public class CommunicationRepository : ICommunicationRepository
{
    private readonly DocumentProcessingDbContext _context;

    public CommunicationRepository(DocumentProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<CommunicationAggregate?> GetByIdAsync(Guid id)
    {
        var communication = await _context.Communications
            .FirstOrDefaultAsync(c => c.Id == id);

        return communication != null ? new CommunicationAggregate(communication) : null;
    }

    public async Task<List<CommunicationAggregate>> GetBySubmissionIdAsync(Guid submissionId)
    {
        var communications = await _context.Communications
            .Where(c => c.SubmissionId == submissionId)
            .ToListAsync();

        return communications.Select(c => new CommunicationAggregate(c)).ToList();
    }

    public async Task<CommunicationAggregate> AddAsync(CommunicationAggregate aggregate)
    {
        var communication = aggregate.Communication;
        await _context.Communications.AddAsync(communication);
        
        // Add status entries
        foreach (var statusEntry in aggregate.StatusEntries)
        {
            await _context.CommunicationStatusEntries.AddAsync(statusEntry);
        }

        return aggregate;
    }

    public async Task UpdateAsync(CommunicationAggregate aggregate)
    {
        var communication = aggregate.Communication;
        _context.Communications.Update(communication);
        
        // Add new status entries
        foreach (var statusEntry in aggregate.StatusEntries)
        {
            // Check if status entry already exists to avoid duplicates
            var existsInDb = await _context.CommunicationStatusEntries
                .AnyAsync(s => s.Id == statusEntry.Id);
            
            if (!existsInDb)
            {
                await _context.CommunicationStatusEntries.AddAsync(statusEntry);
            }
        }
    }

    public async Task<List<CommunicationStatusEntry>> GetStatusHistoryAsync(Guid communicationId)
    {
        return await _context.CommunicationStatusEntries
            .Where(s => s.CommunicationId == communicationId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task AddStatusEntryAsync(CommunicationStatusEntry statusEntry)
    {
        await _context.CommunicationStatusEntries.AddAsync(statusEntry);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}