namespace UserAccounts.Service.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using UserAccounts.Service.Models;

    public interface IUserService
    {

        Task<List<UserModel>> GetUsersAsync();
        Task<UserModel> GetUserAsync(int id);

        Task<ServiceResponse> InsertUserAsync(UserModel userModel);
        Task<ServiceResponse> UpdateUserAsync(UserModel userModel);

        Task DeleteAsync(int id);
    }
}
