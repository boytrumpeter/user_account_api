using DocumentProcessing.Functions.Models;

namespace DocumentProcessing.Functions.Services;

public interface IXmlValidationService
{
    Task<XmlValidationResult> ValidateXmlAsync(string xmlContent);
}

public class XmlValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
}