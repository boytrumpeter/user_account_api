using MediatR;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using DocumentProcessing.Functions.Queries;
using DocumentProcessing.Functions.Models;

namespace DocumentProcessing.Functions.Queries.Handlers;

public class GetDocumentStatusQueryHandler : IRequestHandler<GetDocumentStatusQuery, DocumentProcessingResult?>
{
    private readonly DurableTaskClient _durableTaskClient;
    private readonly ILogger<GetDocumentStatusQueryHandler> _logger;

    public GetDocumentStatusQueryHandler(DurableTaskClient durableTaskClient, ILogger<GetDocumentStatusQueryHandler> logger)
    {
        _durableTaskClient = durableTaskClient;
        _logger = logger;
    }

    public async Task<DocumentProcessingResult?> Handle(GetDocumentStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting status for document {DocumentId}", request.DocumentId);

            // This is a simplified implementation
            // In a real scenario, you might store document status in a database or state store
            // For now, we'll return a placeholder response
            return new DocumentProcessingResult
            {
                DocumentId = request.DocumentId,
                DocumentName = "Unknown",
                Status = DocumentStatus.Validating,
                ProcessedAt = DateTime.UtcNow,
                IsSuccess = false,
                ErrorMessage = "Status lookup not fully implemented - use batch status instead"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document status for {DocumentId}", request.DocumentId);
            return null;
        }
    }
}