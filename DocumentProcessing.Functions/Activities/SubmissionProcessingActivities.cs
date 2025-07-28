using Microsoft.Azure.Functions.Worker;
using DocumentProcessing.Functions.Models.Domain;
using DocumentProcessing.Functions.Models.Domain.Aggregates;
using DocumentProcessing.Functions.Infrastructure.Repositories;
using DocumentProcessing.Functions.Services;
using DocumentProcessing.Functions.Orchestrators;

namespace DocumentProcessing.Functions.Activities;

public class SubmissionProcessingActivities
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly ICommunicationRepository _communicationRepository;
    private readonly IBlobService _blobService;
    private readonly IXmlProcessingService _xmlProcessingService;
    private readonly IAggregateFactory _aggregateFactory;
    private readonly IAggregateRepository _aggregateRepository;
    private readonly ILogger<SubmissionProcessingActivities> _logger;

    public SubmissionProcessingActivities(
        ISubmissionRepository submissionRepository,
        ICommunicationRepository communicationRepository,
        IBlobService blobService,
        IXmlProcessingService xmlProcessingService,
        IAggregateFactory aggregateFactory,
        IAggregateRepository aggregateRepository,
        ILogger<SubmissionProcessingActivities> logger)
    {
        _submissionRepository = submissionRepository;
        _communicationRepository = communicationRepository;
        _blobService = blobService;
        _xmlProcessingService = xmlProcessingService;
        _aggregateFactory = aggregateFactory;
        _aggregateRepository = aggregateRepository;
        _logger = logger;
    }

    [Function("DownloadXmlFromBlobActivity")]
    public async Task<BlobDownloadResult> DownloadXmlFromBlobActivity([ActivityTrigger] Guid submissionId)
    {
        try
        {
            _logger.LogInformation("Downloading XML from blob for submission: {SubmissionId}", submissionId);

            // Reconstruct aggregate from persistence
            var submissionAggregate = await _aggregateRepository.ReconstructSubmissionAggregateAsync(submissionId);
            if (submissionAggregate == null)
            {
                return new BlobDownloadResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "Submission not found" 
                };
            }

            var xmlContent = await _blobService.DownloadXmlFromBlobAsync(submissionAggregate.Submission.BlobUrl);
            
            // Update submission with XML content
            submissionAggregate.Submission.XmlContent = xmlContent;
            await _submissionRepository.UpdateAsync(submissionAggregate);
            await _submissionRepository.SaveChangesAsync();

            return new BlobDownloadResult
            {
                IsSuccess = true,
                XmlContent = xmlContent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading XML from blob for submission: {SubmissionId}", submissionId);
            return new BlobDownloadResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    [Function("ValidateXmlActivity")]
    public async Task<XmlValidationActivityResult> ValidateXmlActivity([ActivityTrigger] XmlValidationActivityInput input)
    {
        try
        {
            _logger.LogInformation("Validating XML for submission: {SubmissionId}", input.SubmissionId);

            // Reconstruct aggregate from persistence
            var submissionAggregate = await _aggregateRepository.ReconstructSubmissionAggregateAsync(input.SubmissionId);
            if (submissionAggregate == null)
            {
                return new XmlValidationActivityResult 
                { 
                    IsValid = false, 
                    ErrorMessage = "Submission not found" 
                };
            }

            submissionAggregate.ValidateXmlContent();

            var validationResult = await _xmlProcessingService.ValidateXmlAsync(input.XmlContent);

            if (validationResult.IsValid)
            {
                submissionAggregate.MarkAsValidated();
            }
            else
            {
                submissionAggregate.MarkAsValidationFailed(validationResult.ValidationErrors);
            }

            await _submissionRepository.UpdateAsync(submissionAggregate);
            await _submissionRepository.SaveChangesAsync();

            return new XmlValidationActivityResult
            {
                IsValid = validationResult.IsValid,
                ValidationErrors = validationResult.ValidationErrors,
                ErrorMessage = validationResult.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating XML for submission: {SubmissionId}", input.SubmissionId);
            return new XmlValidationActivityResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }

    [Function("ExtractCommunicationsActivity")]
    public async Task<CommunicationExtractionActivityResult> ExtractCommunicationsActivity([ActivityTrigger] CommunicationExtractionActivityInput input)
    {
        try
        {
            _logger.LogInformation("Extracting communications for submission: {SubmissionId}", input.SubmissionId);

            // Extract communications from XML (returns Communication entities)
            var communications = await _xmlProcessingService.ExtractCommunicationsFromXmlAsync(input.XmlContent, input.SubmissionId);

            var communicationIds = new List<Guid>();

            // Create aggregates for each communication and save
            foreach (var communication in communications)
            {
                // Wrap the entity in an aggregate (this is reconstruction, not creation)
                var communicationAggregate = new CommunicationAggregate(communication);
                await _communicationRepository.AddAsync(communicationAggregate);
                communicationIds.Add(communication.Id);
            }

            await _communicationRepository.SaveChangesAsync();

            // Update submission status
            var submissionAggregate = await _aggregateRepository.ReconstructSubmissionAggregateAsync(input.SubmissionId);
            if (submissionAggregate != null)
            {
                submissionAggregate.ExtractCommunications(communications);
                await _submissionRepository.UpdateAsync(submissionAggregate);
                await _submissionRepository.SaveChangesAsync();
            }

            return new CommunicationExtractionActivityResult
            {
                IsSuccess = true,
                CommunicationIds = communicationIds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting communications for submission: {SubmissionId}", input.SubmissionId);
            return new CommunicationExtractionActivityResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    [Function("ProcessCommunicationActivity")]
    public async Task<CommunicationProcessingResult> ProcessCommunicationActivity([ActivityTrigger] Guid communicationId)
    {
        try
        {
            _logger.LogInformation("Processing communication: {CommunicationId}", communicationId);

            // Reconstruct aggregate from persistence
            var communicationAggregate = await _aggregateRepository.ReconstructCommunicationAggregateAsync(communicationId);
            if (communicationAggregate == null)
            {
                return new CommunicationProcessingResult
                {
                    CommunicationId = communicationId,
                    IsSuccess = false,
                    ErrorMessage = "Communication not found"
                };
            }

            // Validate communication content
            communicationAggregate.ValidateContent();
            
            // For simplicity, assume validation always passes
            // In real implementation, you would validate email format, content rules, etc.
            communicationAggregate.MarkAsValidated();

            // Store communication
            communicationAggregate.Store();
            communicationAggregate.MarkAsStored();

            await _communicationRepository.UpdateAsync(communicationAggregate);
            await _communicationRepository.SaveChangesAsync();

            return new CommunicationProcessingResult
            {
                CommunicationId = communicationId,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing communication: {CommunicationId}", communicationId);
            return new CommunicationProcessingResult
            {
                CommunicationId = communicationId,
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    [Function("FinalizeSubmissionActivity")]
    public async Task<bool> FinalizeSubmissionActivity([ActivityTrigger] FinalizeSubmissionActivityInput input)
    {
        try
        {
            _logger.LogInformation("Finalizing submission: {SubmissionId}", input.SubmissionId);

            // Reconstruct aggregate from persistence
            var submissionAggregate = await _aggregateRepository.ReconstructSubmissionAggregateAsync(input.SubmissionId);
            if (submissionAggregate == null)
            {
                return false;
            }

            var allSuccessful = input.CommunicationResults.All(r => r.IsSuccess);

            if (allSuccessful)
            {
                submissionAggregate.MarkAsCompleted();
            }
            else
            {
                var failedCount = input.CommunicationResults.Count(r => !r.IsSuccess);
                submissionAggregate.MarkAsFailed($"Failed to process {failedCount} communications");
            }

            await _submissionRepository.UpdateAsync(submissionAggregate);
            await _submissionRepository.SaveChangesAsync();

            return allSuccessful;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing submission: {SubmissionId}", input.SubmissionId);
            return false;
        }
    }

    [Function("RaiseCommunicationsStoredEventActivity")]
    public async Task RaiseCommunicationsStoredEventActivity([ActivityTrigger] Guid submissionId)
    {
        try
        {
            _logger.LogInformation("Raising communications stored event for submission: {SubmissionId}", submissionId);

            // Get communications for the submission
            var communications = await _communicationRepository.GetBySubmissionIdAsync(submissionId);
            var communicationIds = communications.Select(c => c.Id).ToList();

            // In a real implementation, you would publish this event to a message bus
            // For now, we'll just log it
            _logger.LogInformation("Communications stored event raised for submission {SubmissionId} with {Count} communications: {CommunicationIds}", 
                submissionId, communicationIds.Count, string.Join(", ", communicationIds));

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error raising communications stored event for submission: {SubmissionId}", submissionId);
            throw;
        }
    }
}