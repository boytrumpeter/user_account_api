using DocumentProcessing.Functions.Commands;
using DocumentProcessing.Functions.Services;
using DocumentProcessing.Functions.Infrastructure;

namespace DocumentProcessing.Functions.Commands.Handlers;

public class ValidateDocumentCommandHandler : ICommandHandler<ValidateDocumentCommand, ValidationResult>
{
    private readonly IXmlValidationService _xmlValidationService;

    public ValidateDocumentCommandHandler(IXmlValidationService xmlValidationService)
    {
        _xmlValidationService = xmlValidationService;
    }

    public async Task<ValidationResult> HandleAsync(ValidateDocumentCommand request, CancellationToken cancellationToken = default)
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