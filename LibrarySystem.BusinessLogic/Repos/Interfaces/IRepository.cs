using LibrarySystem.BusinessLogic.Domain;
using LibrarySystem.DataAccess.Helpers;

namespace LibrarySystem.BusinessLogic.Repos.Interfaces;

public interface IRepository<T, TKey> where T : new()
{
    Task<TKey> Insert(T entity);
    Task<bool> Update(T entity);
    Task<T> GetById(TKey id);
    public Task<PagingResult<T>> Get(
    Dictionary<string, object> parameters = null,
    int? skip = null,
    int? take = null);
    ValueTask BeginTransaction(bool serializable = false);
    Task CommitTransaction();
    Task RollbackTransaction();
}
