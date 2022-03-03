using System.Collections.Generic;
using System.Threading.Tasks;

namespace IRSeaBot.Data.Repositories
{
    public interface IBaseRepository<T>
    {
        Task<List<T>> GetSet();
        Task<T> GetById(int id);
        Task<T> Edit(T entity);
        Task Delete(int id);

    }
}
