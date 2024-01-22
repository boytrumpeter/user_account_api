namespace UserAccounts.Service.Validations
{
    using FluentValidation;
    
    using UserAccounts.Domain.Interfaces;
    using UserAccounts.Service.Models;
    

    public class UserAccountValidator : AbstractValidator<UserModel>
    {
        private readonly IUserRepository _userRepository;

        public UserAccountValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;

            RuleFor(x => x.Id).NotNull();
            RuleFor(x => x.FirstName)
                .Length(1, 128).WithMessage(ValidationMessages.NAME_LENGTH_ERROR);
            RuleFor(x => x.LastName)
                .Length(1, 128).WithMessage(ValidationMessages.NAME_LENGTH_ERROR);
            RuleFor(x => x.Email).EmailAddress()
                .WithMessage(ValidationMessages.INVALID_EMAIL)
                .Must(UniqueEmailAddress)
                .WithMessage(ValidationMessages.EMAIL_EXISTS)
                ;
            RuleFor(x => x.Age).InclusiveBetween(18, 150)
                .WithMessage(ValidationMessages.INVALID_AGE);
        }

        private bool UniqueEmailAddress(string email)
        {
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                return true;
            }

            return false;
        }
    }
    
}
