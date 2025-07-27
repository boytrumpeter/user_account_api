using System.Xml;
using System.Xml.Linq;
using DocumentProcessing.Functions.Models.Domain;

namespace DocumentProcessing.Functions.Services;

public class XmlProcessingService : IXmlProcessingService
{
    private readonly ILogger<XmlProcessingService> _logger;

    public XmlProcessingService(ILogger<XmlProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<XmlValidationResult> ValidateXmlAsync(string xmlContent)
    {
        var result = new XmlValidationResult();
        var validationErrors = new List<ValidationError>();

        try
        {
            _logger.LogInformation("Starting XML validation");

            // Basic XML structure validation
            var doc = XDocument.Parse(xmlContent);

            // Validate root element
            if (doc.Root == null)
            {
                validationErrors.Add(new ValidationError 
                { 
                    Message = "XML document has no root element",
                    Severity = "Error"
                });
            }
            else
            {
                // Validate required structure for submissions
                await ValidateSubmissionStructureAsync(doc, validationErrors);
            }

            result.IsValid = validationErrors.Count == 0;
            result.ValidationErrors = validationErrors;

            _logger.LogInformation("XML validation completed. IsValid: {IsValid}, Errors: {ErrorCount}", 
                result.IsValid, validationErrors.Count);
        }
        catch (XmlException ex)
        {
            _logger.LogError(ex, "XML parsing error during validation");
            validationErrors.Add(new ValidationError
            {
                Message = $"XML parsing error: {ex.Message}",
                LineNumber = ex.LineNumber,
                ColumnNumber = ex.LinePosition,
                Severity = "Error"
            });
            result.IsValid = false;
            result.ValidationErrors = validationErrors;
            result.ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during XML validation");
            result.IsValid = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<List<Communication>> ExtractCommunicationsFromXmlAsync(string xmlContent, Guid submissionId)
    {
        var communications = new List<Communication>();

        try
        {
            _logger.LogInformation("Starting communication extraction from XML");

            var doc = XDocument.Parse(xmlContent);
            
            // Extract communications based on XML structure
            // This assumes a structure like:
            // <submission>
            //   <communications>
            //     <communication>
            //       <recipient>email@example.com</recipient>
            //       <subject>Subject line</subject>
            //       <content>Document content</content>
            //       <type>letter</type>
            //     </communication>
            //   </communications>
            // </submission>

            var communicationElements = doc.Descendants("communication");

            foreach (var commElement in communicationElements)
            {
                var communication = await ExtractSingleCommunicationAsync(commElement, submissionId);
                if (communication != null)
                {
                    communications.Add(communication);
                }
            }

            _logger.LogInformation("Extracted {Count} communications from XML", communications.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting communications from XML");
            throw new InvalidOperationException($"Failed to extract communications: {ex.Message}", ex);
        }

        return communications;
    }

    private async Task ValidateSubmissionStructureAsync(XDocument doc, List<ValidationError> validationErrors)
    {
        // Validate that required elements exist
        var requiredElements = new[] { "submission", "communications" };
        
        foreach (var requiredElement in requiredElements)
        {
            if (doc.Descendants(requiredElement).FirstOrDefault() == null)
            {
                validationErrors.Add(new ValidationError
                {
                    Message = $"Required element '{requiredElement}' is missing",
                    Severity = "Error"
                });
            }
        }

        // Validate communication structure
        var communications = doc.Descendants("communication");
        var commIndex = 0;
        
        foreach (var comm in communications)
        {
            commIndex++;
            await ValidateCommunicationElementAsync(comm, commIndex, validationErrors);
        }

        await Task.CompletedTask;
    }

    private async Task ValidateCommunicationElementAsync(XElement commElement, int index, List<ValidationError> validationErrors)
    {
        var requiredFields = new[] { "recipient", "content" };
        
        foreach (var field in requiredFields)
        {
            var element = commElement.Element(field);
            if (element == null || string.IsNullOrWhiteSpace(element.Value))
            {
                validationErrors.Add(new ValidationError
                {
                    Message = $"Communication {index}: Required field '{field}' is missing or empty",
                    Severity = "Error"
                });
            }
        }

        // Validate recipient email format
        var recipientElement = commElement.Element("recipient");
        if (recipientElement != null && !string.IsNullOrWhiteSpace(recipientElement.Value))
        {
            var email = recipientElement.Value;
            if (!IsValidEmail(email))
            {
                validationErrors.Add(new ValidationError
                {
                    Message = $"Communication {index}: Invalid email format '{email}'",
                    Severity = "Warning"
                });
            }
        }

        await Task.CompletedTask;
    }

    private async Task<Communication?> ExtractSingleCommunicationAsync(XElement commElement, Guid submissionId)
    {
        try
        {
            var recipient = commElement.Element("recipient")?.Value ?? string.Empty;
            var subject = commElement.Element("subject")?.Value ?? string.Empty;
            var content = commElement.Element("content")?.Value ?? string.Empty;
            var type = commElement.Element("type")?.Value ?? "document";

            if (string.IsNullOrWhiteSpace(recipient) || string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Skipping communication with missing required fields");
                return null;
            }

            var communication = new Communication
            {
                SubmissionId = submissionId,
                Recipient = recipient,
                Subject = subject,
                DocumentContent = content,
                DocumentType = type,
                Status = CommunicationStatus.Extracted,
                CreatedAt = DateTime.UtcNow
            };

            await Task.CompletedTask;
            return communication;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting single communication");
            return null;
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}