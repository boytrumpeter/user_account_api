using Microsoft.EntityFrameworkCore;
using DocumentProcessing.Functions.Models.Domain.Aggregates;

namespace DocumentProcessing.Functions.Infrastructure.Repositories;

/// <summary>
/// Service responsible for reconstructing aggregates from persistence.
/// Uses individual repositories internally but provides a unified interface for aggregate reconstruction.
/// </summary>
public class AggregateRepositoryService : IAggregateRepositoryService
{
    private readonly DocumentProcessingDbContext _context;
    private readonly ILogger<AggregateRepositoryService> _logger;

    public AggregateRepositoryService(
        DocumentProcessingDbContext context,
        ILogger<AggregateRepositoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SubmissionAggregate?> ReconstructSubmissionAggregateAsync(Guid submissionId)
    {
        try
        {
            _logger.LogDebug("Reconstructing submission aggregate for ID: {SubmissionId}", submissionId);

            var submission = await _context.Submissions
                .Include(s => s.Communications)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                _logger.LogWarning("Submission not found for ID: {SubmissionId}", submissionId);
                return null;
            }

            return new SubmissionAggregate(submission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconstructing submission aggregate for ID: {SubmissionId}", submissionId);
            throw;
        }
    }

    public async Task<CommunicationAggregate?> ReconstructCommunicationAggregateAsync(Guid communicationId)
    {
        try
        {
            _logger.LogDebug("Reconstructing communication aggregate for ID: {CommunicationId}", communicationId);

            var communication = await _context.Communications
                .FirstOrDefaultAsync(c => c.Id == communicationId);

            if (communication == null)
            {
                _logger.LogWarning("Communication not found for ID: {CommunicationId}", communicationId);
                return null;
            }

            return new CommunicationAggregate(communication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconstructing communication aggregate for ID: {CommunicationId}", communicationId);
            throw;
        }
    }
}