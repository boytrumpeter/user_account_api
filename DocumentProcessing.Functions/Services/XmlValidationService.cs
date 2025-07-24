using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using DocumentProcessing.Functions.Models;
using Microsoft.Extensions.Logging;

namespace DocumentProcessing.Functions.Services;

public class XmlValidationService : IXmlValidationService
{
    private readonly ILogger<XmlValidationService> _logger;

    public XmlValidationService(ILogger<XmlValidationService> logger)
    {
        _logger = logger;
    }

    public async Task<XmlValidationResult> ValidateXmlAsync(string xmlContent)
    {
        var result = new XmlValidationResult();
        var errors = new List<ValidationError>();

        try
        {
            // Check if XML is well-formed
            var doc = XDocument.Parse(xmlContent);
            
            // Basic validation rules
            await ValidateBasicRules(doc, errors);
            
            result.IsValid = errors.Count == 0;
            result.Errors = errors;
            
            _logger.LogInformation("XML validation completed. IsValid: {IsValid}, ErrorCount: {ErrorCount}", 
                result.IsValid, errors.Count);
        }
        catch (XmlException xmlEx)
        {
            errors.Add(new ValidationError
            {
                Message = $"XML parsing error: {xmlEx.Message}",
                LineNumber = xmlEx.LineNumber,
                ColumnNumber = xmlEx.LinePosition,
                Severity = "Error"
            });
            
            result.IsValid = false;
            result.Errors = errors;
            
            _logger.LogError(xmlEx, "XML parsing failed");
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError
            {
                Message = $"Validation error: {ex.Message}",
                Severity = "Error"
            });
            
            result.IsValid = false;
            result.Errors = errors;
            
            _logger.LogError(ex, "XML validation failed");
        }

        return result;
    }

    private async Task ValidateBasicRules(XDocument doc, List<ValidationError> errors)
    {
        await Task.Run(() =>
        {
            // Rule 1: Root element must exist
            if (doc.Root == null)
            {
                errors.Add(new ValidationError
                {
                    Message = "Document must have a root element",
                    Severity = "Error"
                });
                return;
            }

            // Rule 2: Document must have specific required elements
            var requiredElements = new[] { "metadata", "content" };
            foreach (var requiredElement in requiredElements)
            {
                if (doc.Root.Element(requiredElement) == null)
                {
                    errors.Add(new ValidationError
                    {
                        Message = $"Required element '{requiredElement}' is missing",
                        Severity = "Warning"
                    });
                }
            }

            // Rule 3: Check for empty elements that should have content
            var elementsRequiringContent = new[] { "title", "author", "content" };
            foreach (var elementName in elementsRequiringContent)
            {
                var element = doc.Root.Descendants(elementName).FirstOrDefault();
                if (element != null && string.IsNullOrWhiteSpace(element.Value))
                {
                    errors.Add(new ValidationError
                    {
                        Message = $"Element '{elementName}' should not be empty",
                        Severity = "Warning"
                    });
                }
            }

            // Rule 4: Validate date formats if present
            var dateElements = doc.Root.Descendants().Where(e => 
                e.Name.LocalName.Contains("date", StringComparison.OrdinalIgnoreCase) ||
                e.Name.LocalName.Contains("created", StringComparison.OrdinalIgnoreCase));
            
            foreach (var dateElement in dateElements)
            {
                if (!string.IsNullOrEmpty(dateElement.Value) && 
                    !DateTime.TryParse(dateElement.Value, out _))
                {
                    errors.Add(new ValidationError
                    {
                        Message = $"Invalid date format in element '{dateElement.Name.LocalName}': {dateElement.Value}",
                        Severity = "Error"
                    });
                }
            }
        });
    }
}