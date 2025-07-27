using DocumentProcessing.Functions.Models.Domain;

namespace DocumentProcessing.Functions.Services;

public interface IXmlProcessingService
{
    Task<XmlValidationResult> ValidateXmlAsync(string xmlContent);
    Task<List<Communication>> ExtractCommunicationsFromXmlAsync(string xmlContent, Guid submissionId);
}

public class XmlValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> ValidationErrors { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class CommunicationExtractionResult
{
    public List<Communication> Communications { get; set; } = new();
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}