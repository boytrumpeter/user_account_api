using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DocumentProcessing.Functions.Services;
using DocumentProcessing.Functions.Commands.Handlers;
using DocumentProcessing.Functions.Queries.Handlers;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Register handlers directly
        services.AddScoped<ProcessDocumentsCommandHandler>();
        services.AddScoped<ValidateDocumentCommandHandler>();
        services.AddScoped<SendToPrintingCommandHandler>();
        services.AddScoped<GetProcessingStatusQueryHandler>();
        services.AddScoped<GetDocumentStatusQueryHandler>();
        
        // Add custom services
        services.AddScoped<IXmlValidationService, XmlValidationService>();
        services.AddScoped<IPrintingApiService, PrintingApiService>();
        services.AddHttpClient<IPrintingApiService, PrintingApiService>();
    })
    .Build();

host.Run();