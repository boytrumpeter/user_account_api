using DocumentProcessing.Functions.Commands;
using DocumentProcessing.Functions.Services;
using DocumentProcessing.Functions.Infrastructure;

namespace DocumentProcessing.Functions.Commands.Handlers;

public class SendToPrintingCommandHandler : ICommandHandler<SendToPrintingCommand, PrintingResult>
{
    private readonly IPrintingApiService _printingApiService;

    public SendToPrintingCommandHandler(IPrintingApiService printingApiService)
    {
        _printingApiService = printingApiService;
    }

    public async Task<PrintingResult> HandleAsync(SendToPrintingCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var jobId = await _printingApiService.SubmitDocumentForPrintingAsync(
                request.DocumentId,
                request.DocumentName,
                request.XmlContent);

            return new PrintingResult
            {
                DocumentId = request.DocumentId,
                IsSuccessful = true,
                JobId = jobId
            };
        }
        catch (Exception ex)
        {
            return new PrintingResult
            {
                DocumentId = request.DocumentId,
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
    }
}