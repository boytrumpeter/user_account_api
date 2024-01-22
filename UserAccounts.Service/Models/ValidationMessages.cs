namespace UserAccounts.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class ValidationMessages
    {
        public const string USER_EXISTS = "User already exists.";
        public const string USER_NOT_FOUND = "User not found.";
        public const string NAME_LENGTH_ERROR = "Name length must not be more than 128 characters.";
        public const string INVALID_EMAIL = "Invalid Email Address.";
        public const string EMAIL_EXISTS = "Emails Already exists.";
        public const string INVALID_AGE = "Age must be 18 or over.";
    }
}
