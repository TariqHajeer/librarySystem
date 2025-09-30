using System.Reflection;
using LibrarySystem.BusinessLogic.Repos.Interfaces;
using LibrarySystem.BusinessLogic.UserUseCase.Dtos;
using LibrarySystem.DataAccess.Exceptions;
using LibrarySystem.DataAccess.Helpers;
using Microsoft.Data.SqlClient;

namespace LibrarySystem.BusinessLogic.Common;

internal abstract class Service<TEntity, TKey, TCreate, TList> : IService<TEntity, TKey, TCreate, TList> where TEntity : new()

{
    protected readonly IRepository<TEntity, TKey> _repository;
    public Service(IRepository<TEntity, TKey> repository)
    {
        _repository = repository;
    }
    public virtual Task<TKey> Create(TCreate create)
    {
        TEntity entity = MapToEntity(create);
        return _repository.Insert(entity);
    }
    public async Task<PagingResult<TList>> GetPage(
    Dictionary<string, object> parameters = null,
    int? skip = null,
    int? take = null)
    {
        var entitiesResult = await _repository.Get(parameters, skip, take);

        var listResult = new PagingResult<TList>
        {
            TotalCount = entitiesResult.TotalCount,
            Data = entitiesResult.Data.Select(MapToList).ToList()
        };

        return listResult;
    }

    protected virtual TEntity MapToEntity(TCreate create)
    {
        var entity = Activator.CreateInstance<TEntity>();

        foreach (var prop in typeof(TCreate).GetProperties())
        {
            var entityProp = typeof(TEntity).GetProperty(prop.Name);
            if (entityProp != null && entityProp.CanWrite)
            {
                entityProp.SetValue(entity, prop.GetValue(create));
            }
        }

        return entity;
    }
    protected virtual TList MapToList(TEntity entity)
    {
        if (entity == null)
            return default!;

        var listObj = Activator.CreateInstance<TList>();

        foreach (var prop in typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var listProp = typeof(TList).GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
            if (listProp != null && listProp.CanWrite)
            {
                var value = prop.GetValue(entity);
                listProp.SetValue(listObj, value);
            }
        }

        return listObj;
    }

    public async Task<T> ExecuteWithRetry<T>(Func<Task<T>> action, int maxRetries = 3, int baseDelayMs = 300)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await action();
            }
            catch (SqlException ex) when (ex.Number == 1205 || ex.Number == 1204) // deadlock or lock timeout
            {
                if (attempt == maxRetries)
                    throw new ConflictException("there's load on the db try again later");

                // exponential backoff: 300ms, 600ms, 1200ms...
                await Task.Delay(baseDelayMs * (int)Math.Pow(2, attempt - 1));
            }
        }
        throw new InvalidOperationException("Unexpected retry loop exit.");
    }
    public async Task ExecuteWithRetry(Func<Task> action, int maxRetries = 3, int delayMs = 300)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await action();
                return; // success
            }
            catch (SqlException ex) when (ex.Number == 1205 || ex.Number == 1204) // deadlock or lock timeout
            {
                if (attempt == maxRetries)
                    throw new ConflictException("there's load on the db try again later");

                await Task.Delay(delayMs); // simple backoff before retry
            }
        }
    }

}
