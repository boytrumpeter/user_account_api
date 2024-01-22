namespace UserAccounts.Service.Models
{
    using Newtonsoft.Json;
    using System;
    using UserAccounts.Service.Extensions;

    public class UserModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        [JsonProperty("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        public int Age
        {
            get => DateOfBirth.Age();
        }
    }
}
