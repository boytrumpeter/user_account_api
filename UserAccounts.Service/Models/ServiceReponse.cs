namespace UserAccounts.Service.Models
{
    using System.Collections.Generic;
    
    public class ServiceResponse
    {
        public ServiceResponse()
        {

        }

        public ServiceResponse(bool success, List<string> errors = null)
        {
            Success = success;
            Errors = errors;
        }
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
    }
}
