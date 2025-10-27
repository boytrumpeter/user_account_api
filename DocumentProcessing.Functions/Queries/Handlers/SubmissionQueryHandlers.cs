using DocumentProcessing.Functions.Models.Domain;
using DocumentProcessing.Functions.Infrastructure.Repositories;
using DocumentProcessing.Functions.Infrastructure.QueryDispatcher;

namespace DocumentProcessing.Functions.Queries.Handlers;

public class GetSubmissionStatusQueryHandler : IQueryHandler<GetSubmissionStatusQuery, SubmissionStatusResponse?>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly ILogger<GetSubmissionStatusQueryHandler> _logger;

    public GetSubmissionStatusQueryHandler(
        ISubmissionRepository submissionRepository,
        ILogger<GetSubmissionStatusQueryHandler> logger)
    {
        _submissionRepository = submissionRepository;
        _logger = logger;
    }

    public async Task<SubmissionStatusResponse?> HandleAsync(GetSubmissionStatusQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting submission status for ID: {SubmissionId}", query.SubmissionId);

            var submissionAggregate = await _submissionRepository.GetByIdAsync(query.SubmissionId);
            if (submissionAggregate == null)
            {
                return null;
            }

            var submission = submissionAggregate.Submission;

            return new SubmissionStatusResponse
            {
                Id = submission.Id,
                BlobUrl = submission.BlobUrl,
                FileName = submission.FileName,
                Status = submission.Status,
                CreatedAt = submission.CreatedAt,
                ProcessedAt = submission.ProcessedAt,
                ErrorMessage = submission.ErrorMessage,
                ValidationErrors = submission.ValidationErrors,
                CommunicationCount = submission.Communications.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submission status");
            throw;
        }
    }
}

public class GetSubmissionStatusHistoryQueryHandler : IQueryHandler<GetSubmissionStatusHistoryQuery, List<SubmissionStatusEntry>>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly ILogger<GetSubmissionStatusHistoryQueryHandler> _logger;

    public GetSubmissionStatusHistoryQueryHandler(
        ISubmissionRepository submissionRepository,
        ILogger<GetSubmissionStatusHistoryQueryHandler> logger)
    {
        _submissionRepository = submissionRepository;
        _logger = logger;
    }

    public async Task<List<SubmissionStatusEntry>> HandleAsync(GetSubmissionStatusHistoryQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting submission status history for ID: {SubmissionId}", query.SubmissionId);

            return await _submissionRepository.GetStatusHistoryAsync(query.SubmissionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submission status history");
            throw;
        }
    }
}

public class GetCommunicationsQueryHandler : IQueryHandler<GetCommunicationsQuery, List<CommunicationResponse>>
{
    private readonly ICommunicationRepository _communicationRepository;
    private readonly ILogger<GetCommunicationsQueryHandler> _logger;

    public GetCommunicationsQueryHandler(
        ICommunicationRepository communicationRepository,
        ILogger<GetCommunicationsQueryHandler> logger)
    {
        _communicationRepository = communicationRepository;
        _logger = logger;
    }

    public async Task<List<CommunicationResponse>> HandleAsync(GetCommunicationsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting communications for submission ID: {SubmissionId}", query.SubmissionId);

            var communicationAggregates = await _communicationRepository.GetBySubmissionIdAsync(query.SubmissionId);

            return communicationAggregates.Select(ca => new CommunicationResponse
            {
                Id = ca.Communication.Id,
                SubmissionId = ca.Communication.SubmissionId,
                DocumentType = ca.Communication.DocumentType,
                Recipient = ca.Communication.Recipient,
                Subject = ca.Communication.Subject,
                Status = ca.Communication.Status,
                CreatedAt = ca.Communication.CreatedAt,
                ProcessedAt = ca.Communication.ProcessedAt,
                ValidationErrors = ca.Communication.ValidationErrors
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting communications");
            throw;
        }
    }
}