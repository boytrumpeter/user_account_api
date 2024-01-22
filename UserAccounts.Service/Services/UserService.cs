namespace UserAccounts.Service.Services
{
    using AutoMapper;
    using FluentValidation;
    using FluentValidation.Results;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UserAccounts.Domain.Interfaces;
    using UserAccounts.Domain.Models;
    using UserAccounts.Service.Models;

    public class UserService : IUserService
    {
        private readonly IValidator<UserModel> _validator;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IValidator<UserModel> validator, IUserRepository userRepository, IMapper mapper, ILogger<UserService> logger)
        {
            _validator = validator;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<UserModel>> GetUsersAsync()
        {
            var users = await _userRepository.GetAll().ConfigureAwait(false);
            return users.Select(x => _mapper.Map<UserModel>(x)).ToList();
        }

        public async Task<UserModel> GetUserAsync(int id)
        {
            var user = await _userRepository.GetById(id).ConfigureAwait(false);
            return _mapper.Map<UserModel>(user);
        }

        private async Task<ValidationResult> ValidateUser(UserModel userModel)
        {
            ValidationResult validationResult = await _validator.ValidateAsync(userModel);

            if (!validationResult.IsValid)
            {
                // Log Error. Try not to send the actual reason for invalid request but log instead or make it visible for business in dashboard as such.

                StringBuilder stringBuilder = new StringBuilder();
                validationResult.Errors.Select(x => stringBuilder.AppendLine(x.ErrorMessage));
                _logger.LogWarning(stringBuilder.ToString());
            }

            return validationResult;
        }

        public async Task DeleteAsync(int id)
        {
            await _userRepository.Delete(id);
        }

        public async Task<ServiceResponse> InsertUserAsync(UserModel userModel)
        {
            var validationResult = await ValidateUser(userModel);
            var response = new ServiceResponse(false);

            if (validationResult.IsValid)
            {
                var user = _mapper.Map<User>(userModel);
                await _userRepository.Add(user).ConfigureAwait(false);
                response.Success = true;
            }

            response.Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
            return response;
        }

        public async Task<ServiceResponse> UpdateUserAsync(UserModel userModel)
        {
            var validationResult = await ValidateUser(userModel);
            var response = new ServiceResponse(false);

            if (validationResult.IsValid)
            {
                var user = _mapper.Map<User>(userModel);
                await _userRepository.Update(user).ConfigureAwait(false);
                response.Success = true;
            }

            response.Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
            return response;
        }
    }
}
