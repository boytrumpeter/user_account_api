using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using DocumentProcessing.Functions.Models.Domain;
using DocumentProcessing.Functions.Infrastructure.Repositories;
using DocumentProcessing.Functions.Infrastructure.CommandDispatcher;

namespace DocumentProcessing.Functions.Commands.Handlers;

public class ProcessSubmissionCommandHandler : ICommandHandler<ProcessSubmissionCommand, ProcessSubmissionResponse>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IAggregateFactory _aggregateFactory;
    private readonly DurableTaskClient _durableTaskClient;
    private readonly ILogger<ProcessSubmissionCommandHandler> _logger;

    public ProcessSubmissionCommandHandler(
        ISubmissionRepository submissionRepository,
        IAggregateFactory aggregateFactory,
        DurableTaskClient durableTaskClient,
        ILogger<ProcessSubmissionCommandHandler> logger)
    {
        _submissionRepository = submissionRepository;
        _aggregateFactory = aggregateFactory;
        _durableTaskClient = durableTaskClient;
        _logger = logger;
    }

    public async Task<ProcessSubmissionResponse> HandleAsync(ProcessSubmissionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing submission for blob URL: {BlobUrl}", command.BlobUrl);

            // Create NEW submission aggregate using proper factory method
            var submissionAggregate = _aggregateFactory.CreateNewSubmission(command.BlobUrl, command.FileName);
            
            // Save initial submission
            await _submissionRepository.AddAsync(submissionAggregate);
            await _submissionRepository.SaveChangesAsync();

            // Start durable orchestration
            var instanceId = await _durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
                "SubmissionProcessingOrchestrator",
                submissionAggregate.Id);

            _logger.LogInformation("Started orchestration for submission {SubmissionId} with instance ID: {InstanceId}", 
                submissionAggregate.Id, instanceId);

            return new ProcessSubmissionResponse
            {
                SubmissionId = submissionAggregate.Id,
                Status = "Processing",
                IsSuccess = true,
                StartedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing submission command");
            return new ProcessSubmissionResponse
            {
                Status = "Failed",
                IsSuccess = false,
                ErrorMessage = ex.Message,
                StartedAt = DateTime.UtcNow
            };
        }
    }
}