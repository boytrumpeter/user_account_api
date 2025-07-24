using MediatR;
using Microsoft.DurableTask.Client;
using DocumentProcessing.Functions.Commands;
using DocumentProcessing.Functions.Models;

namespace DocumentProcessing.Functions.Commands.Handlers;

public class ProcessDocumentsCommandHandler : IRequestHandler<ProcessDocumentsCommand, ProcessDocumentsResponse>
{
    private readonly DurableTaskClient _durableTaskClient;

    public ProcessDocumentsCommandHandler(DurableTaskClient durableTaskClient)
    {
        _durableTaskClient = durableTaskClient;
    }

    public async Task<ProcessDocumentsResponse> Handle(ProcessDocumentsCommand request, CancellationToken cancellationToken)
    {
        var orchestrationInput = new ProcessDocumentsRequest
        {
            BatchId = request.BatchId,
            Documents = request.Documents,
            CallbackUrl = request.CallbackUrl,
            Metadata = request.Metadata
        };

        // Start the durable function orchestration
                var instanceId = await _durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
            "DocumentProcessingOrchestrator", 
            orchestrationInput);

        return new ProcessDocumentsResponse
        {
            BatchId = request.BatchId,
            Status = "Started",
            StartedAt = DateTime.UtcNow,
            Results = request.Documents.Select(d => new DocumentProcessingResult
            {
                DocumentId = d.Id,
                DocumentName = d.Name,
                Status = DocumentStatus.Pending
            }).ToList()
        };
    }
}