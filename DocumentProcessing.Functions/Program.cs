using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MediatR;
using DocumentProcessing.Functions.Services;
using DocumentProcessing.Functions.Infrastructure;
using DocumentProcessing.Functions.Infrastructure.Repositories;
using DocumentProcessing.Functions.Models.Domain;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Database Context
        services.AddDbContext<DocumentProcessingDbContext>(options =>
        {
            var connectionString = Environment.GetEnvironmentVariable("DocumentProcessingConnectionString") 
                ?? "Server=(localdb)\\mssqllocaldb;Database=DocumentProcessing;Trusted_Connection=true;TrustServerCertificate=true;";
            options.UseSqlServer(connectionString);
        });

        // Add MediatR for CQRS pattern
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        
        // Domain Services
        services.AddScoped<IAggregateFactory, AggregateFactory>();
        
        // Infrastructure Services
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<ICommunicationRepository, CommunicationRepository>();
        
        // Application Services
        services.AddScoped<IBlobService, BlobService>();
        services.AddScoped<IXmlProcessingService, XmlProcessingService>();
        services.AddScoped<IXmlValidationService, XmlValidationService>();
        services.AddScoped<IPrintingApiService, PrintingApiService>();
        
        // HTTP Clients
        services.AddHttpClient<IBlobService, BlobService>();
        services.AddHttpClient<IPrintingApiService, PrintingApiService>();
        
        // Durable Functions
        services.AddDurableTask(builder =>
        {
            // Configure durable task options if needed
        });
    })
    .Build();

host.Run();