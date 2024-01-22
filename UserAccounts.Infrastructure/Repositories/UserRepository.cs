namespace UserAccounts.Infrastructure.Repositories
{
    using System.Linq;
    using UserAccounts.Domain.Interfaces;
    using UserAccounts.Domain.Models;

    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public User GetByEmail(string email)
        {
            var user  = _dbContext.Set<User>().FirstOrDefault(x => x.Email.Equals(email, System.StringComparison.InvariantCultureIgnoreCase));

            return user;
        }
    }
}
