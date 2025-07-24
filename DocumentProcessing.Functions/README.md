# Document Processing Azure Functions with CQRS

This project implements a **CQRS (Command Query Responsibility Segregation)** pattern using **Azure Durable Functions** to process XML documents with a **fan-out/fan-in** pattern for validation and printing.

## Architecture Overview

### CQRS Pattern Implementation
- **Commands**: Handle write operations (processing documents, validation, printing)
- **Queries**: Handle read operations (getting status, retrieving results)
- **MediatR**: Provides the command/query dispatcher pattern
- **Handlers**: Separate handlers for each command and query

### Durable Functions Pattern
- **Orchestrator**: Coordinates the entire document processing workflow
- **Activities**: Individual tasks (validate, print, notify)
- **Fan-out/Fan-in**: Parallel processing of multiple documents with aggregated results

## Project Structure

```
DocumentProcessing.Functions/
├── Commands/                     # CQRS Commands
│   ├── ProcessDocumentsCommand.cs
│   ├── ValidateDocumentCommand.cs
│   ├── SendToPrintingCommand.cs
│   └── Handlers/                 # Command Handlers
│       ├── ProcessDocumentsCommandHandler.cs
│       ├── ValidateDocumentCommandHandler.cs
│       └── SendToPrintingCommandHandler.cs
├── Queries/                      # CQRS Queries
│   ├── GetProcessingStatusQuery.cs
│   ├── GetDocumentStatusQuery.cs
│   └── Handlers/                 # Query Handlers
│       ├── GetProcessingStatusQueryHandler.cs
│       └── GetDocumentStatusQueryHandler.cs
├── Models/                       # Data Models
│   ├── Document.cs
│   └── ProcessDocumentsRequest.cs
├── Services/                     # Business Logic Services
│   ├── IXmlValidationService.cs
│   ├── XmlValidationService.cs
│   ├── IPrintingApiService.cs
│   └── PrintingApiService.cs
├── Orchestrators/                # Durable Function Orchestrators
│   └── DocumentProcessingOrchestrator.cs
├── Activities/                   # Durable Function Activities
│   └── DocumentProcessingActivities.cs
├── Functions/                    # HTTP Trigger Functions
│   └── DocumentProcessingFunctions.cs
├── Program.cs                    # Application Entry Point
├── host.json                     # Azure Functions Configuration
└── local.settings.json          # Local Development Settings
```

## Features

### 1. XML Document Validation
- Well-formed XML validation
- Custom business rule validation
- Detailed error reporting with line numbers

### 2. Parallel Processing (Fan-out/Fan-in)
- Processes multiple documents simultaneously
- Aggregates results from all document processing tasks
- Efficient resource utilization

### 3. External API Integration
- Sends validated documents to printing API
- Handles API failures gracefully
- Retry logic for transient failures

### 4. Status Tracking
- Real-time batch processing status
- Individual document status tracking
- Comprehensive error reporting

## API Endpoints

### Process Documents
```http
POST /api/ProcessDocuments
Content-Type: application/json

{
  "batchId": "optional-custom-batch-id",
  "documents": [
    {
      "name": "document1.xml",
      "xmlContent": "<root>...</root>"
    }
  ],
  "callbackUrl": "https://your-callback-endpoint.com/webhook",
  "metadata": {
    "userId": "12345",
    "priority": "high"
  }
}
```

### Get Processing Status
```http
GET /api/GetProcessingStatus/{batchId}
```

### Get Document Status
```http
GET /api/GetDocumentStatus/{documentId}
```

## Configuration

### Local Development (local.settings.json)
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AzureWebJobsFeatureFlags": "EnableWorkerIndexing",
    "PrintingApiBaseUrl": "https://api.printingservice.com",
    "PrintingApiKey": "your-printing-api-key"
  }
}
```

### Azure Production Settings
- Configure connection strings in Azure Portal
- Set up Application Insights for monitoring
- Configure storage account for durable functions state

## Development Setup

### Prerequisites
- .NET 8.0 SDK
- Azure Functions Core Tools v4
- Azure Storage Emulator (for local development)

### Local Development
1. Clone the repository
2. Install dependencies:
   ```bash
   dotnet restore
   ```
3. Start Azure Storage Emulator
4. Run the function app:
   ```bash
   func start
   ```

### Testing
Use tools like Postman, curl, or any HTTP client to test the endpoints:

```bash
# Process documents
curl -X POST http://localhost:7071/api/ProcessDocuments \
  -H "Content-Type: application/json" \
  -d '{
    "documents": [
      {
        "name": "test.xml",
        "xmlContent": "<root><test>Hello World</test></root>"
      }
    ]
  }'

# Check status
curl http://localhost:7071/api/GetProcessingStatus/{batchId}
```

## Deployment

### Azure Deployment
1. Create Azure Function App (Consumption or Premium plan)
2. Configure application settings
3. Deploy using Azure DevOps, GitHub Actions, or VS Code

### Infrastructure as Code
Consider using ARM templates or Bicep for infrastructure deployment:
- Function App
- Storage Account
- Application Insights
- Key Vault (for secrets)

## Monitoring and Observability

### Application Insights
- Custom telemetry tracking
- Performance monitoring
- Error tracking and alerting

### Logging
- Structured logging with correlation IDs
- Different log levels for development and production
- Centralized logging for distributed components

## Error Handling

### Retry Policies
- Configurable retry policies for external API calls
- Exponential backoff for transient failures
- Circuit breaker pattern for cascade failure prevention

### Validation Errors
- Detailed XML validation error reporting
- Business rule validation with custom messages
- Error aggregation for batch processing

## Security Considerations

### Authentication & Authorization
- API key authentication for printing service
- Function-level security keys
- Managed Identity for Azure resource access

### Data Protection
- Sensitive data encryption in transit and at rest
- Secure key management using Azure Key Vault
- Input validation and sanitization

## Performance Optimization

### Durable Functions Best Practices
- Efficient orchestrator patterns
- Proper activity function sizing
- State management optimization

### Scalability
- Horizontal scaling with consumption plan
- Connection pooling for external services
- Efficient memory usage patterns

## Contributing

1. Follow CQRS patterns for new features
2. Ensure proper error handling and logging
3. Add unit tests for business logic
4. Update documentation for API changes

## License

[Specify your license here]