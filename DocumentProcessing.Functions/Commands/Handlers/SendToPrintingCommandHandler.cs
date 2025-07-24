using MediatR;
using DocumentProcessing.Functions.Commands;
using DocumentProcessing.Functions.Services;

namespace DocumentProcessing.Functions.Commands.Handlers;

public class SendToPrintingCommandHandler : IRequestHandler<SendToPrintingCommand, PrintingResult>
{
    private readonly IPrintingApiService _printingApiService;

    public SendToPrintingCommandHandler(IPrintingApiService printingApiService)
    {
        _printingApiService = printingApiService;
    }

    public async Task<PrintingResult> Handle(SendToPrintingCommand request, CancellationToken cancellationToken)
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