using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using DocumentProcessing.Functions.Services;
using DocumentProcessing.Functions.Infrastructure;
using DocumentProcessing.Functions.Infrastructure.Repositories;
using DocumentProcessing.Functions.Models.Domain;
using DocumentProcessing.Functions.Infrastructure.CommandDispatcher;
using DocumentProcessing.Functions.Infrastructure.QueryDispatcher;
using DocumentProcessing.Functions.Commands;
using DocumentProcessing.Functions.Commands.Handlers;
using DocumentProcessing.Functions.Queries;
using DocumentProcessing.Functions.Queries.Handlers;

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

        // Command/Query Dispatchers
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        
        // Command Handlers
        services.AddScoped<ICommandHandler<ProcessSubmissionCommand, ProcessSubmissionResponse>, ProcessSubmissionCommandHandler>();
        
        // Query Handlers
        services.AddScoped<IQueryHandler<GetSubmissionStatusQuery, SubmissionStatusResponse?>, GetSubmissionStatusQueryHandler>();
        services.AddScoped<IQueryHandler<GetSubmissionStatusHistoryQuery, List<SubmissionStatusEntry>>, GetSubmissionStatusHistoryQueryHandler>();
        services.AddScoped<IQueryHandler<GetCommunicationsQuery, List<CommunicationResponse>>, GetCommunicationsQueryHandler>();
        
        // Domain Services
        services.AddScoped<IAggregateFactory, AggregateFactory>();
        
        // Infrastructure Services - Each repository has its own responsibility
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<ICommunicationRepository, CommunicationRepository>();
        
        // Aggregate reconstruction service - separate from individual repositories
        services.AddScoped<IAggregateRepositoryService, AggregateRepositoryService>();
        
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