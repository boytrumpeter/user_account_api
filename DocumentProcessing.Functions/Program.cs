using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediatR;
using DocumentProcessing.Functions.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        
        // Add MediatR for CQRS pattern
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        
        // Add custom services
        services.AddScoped<IXmlValidationService, XmlValidationService>();
        services.AddScoped<IPrintingApiService, PrintingApiService>();
        services.AddHttpClient<IPrintingApiService, PrintingApiService>();
    })
    .Build();

host.Run();