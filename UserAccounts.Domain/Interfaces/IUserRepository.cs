namespace UserAccounts.Domain.Interfaces
{
    using UserAccounts.Domain.Models;

    public interface IUserRepository :IRepository<User>
    {
        User GetByEmail(string email);
    }
}
