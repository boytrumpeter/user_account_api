namespace UserAccounts.Domain.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository<T> where T : class, IEntity
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(int id);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(int id);
    }
}