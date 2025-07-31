using DocumentProcessing.Functions.Commands;
using DocumentProcessing.Functions.Services;

namespace DocumentProcessing.Functions.Commands.Handlers;

public class ValidateDocumentCommandHandler
{
    private readonly IXmlValidationService _xmlValidationService;

    public ValidateDocumentCommandHandler(IXmlValidationService xmlValidationService)
    {
        _xmlValidationService = xmlValidationService;
    }

    public async Task<ValidationResult> ExecuteAsync(ValidateDocumentCommand request)
    {
        var validationResult = await _xmlValidationService.ValidateXmlAsync(request.XmlContent);

        return new ValidationResult
        {
            DocumentId = request.DocumentId,
            IsValid = validationResult.IsValid,
            Errors = validationResult.Errors
        };
    }
}