using System.Reflection;
using LibrarySystem.BusinessLogic.Domain.Attributes;
using LibrarySystem.BusinessLogic.Helper;
using LibrarySystem.BusinessLogic.Repos.Interfaces;
using LibrarySystem.DataAccess;
using LibrarySystem.DataAccess.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LibrarySystem.BusinessLogic.Repos.Implementation;


public class Repository<T, TKey> : IRepository<T, TKey> where T : new()
{
    protected readonly string _insertProcName;
    protected readonly string _getProcName;
    protected readonly string _getByIdProcName;
    protected readonly string _updateProcName;
    private readonly AdoNetDataAccessLayer _adoNetDataAccess;
    public Repository(AdoNetDataAccessLayer adoNetDataAccess, IOptions<RepositoryOptions<T>> options)
    {
        _insertProcName = options.Value.InsertProcName;
        _adoNetDataAccess = adoNetDataAccess;
        _getProcName = options.Value.GetProcName;
        _getByIdProcName = options.Value.GetByIdProcName;
        _updateProcName = options.Value.UpdateProcName;
    }
    public async Task<TKey> Insert(T entity)
    {
        var parameters = entity!
            .GetType()
            .GetProperties()
            .Where(p => !Attribute.IsDefined(p, typeof(IgnoreOnInsertAttribute))) // skip ignored
            .ToDictionary(
                prop => "@" + prop.Name,
                prop => prop.GetValue(entity) ?? DBNull.Value
            );

        var result = await _adoNetDataAccess.ExecuteScalarAsync(_insertProcName, parameters);

        return (TKey)Convert.ChangeType(result, typeof(TKey));
    }

    public async Task<bool> Update(T entity)
    {
        // Build parameters from the entity properties
        var parameters = entity!
            .GetType()
            .GetProperties()
            .Where(p => !Attribute.IsDefined(p, typeof(IgnoreOnUpdateAttribute))) // skip ignored fields like Id, CreatedAt
            .ToDictionary(
                prop => "@" + prop.Name,
                prop => prop.GetValue(entity) ?? DBNull.Value
            );

        // Execute the stored procedure
        var rowsAffected = await _adoNetDataAccess.ExecuteNonQueryAsync(_updateProcName, parameters);
        return rowsAffected > 0;

    }

    public Task<PagingResult<T>> Get(
    Dictionary<string, object> parameters = null,
    int? skip = null,
    int? take = null)
    {
        return _adoNetDataAccess.ExecuteSelectAsync(
            _getProcName,
             MapReaderTo,
            parameters,
            skip,
            take);
    }
    public static T MapReaderTo(SqlDataReader reader)
    {
        var obj = new T();
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            if (!reader.HasColumn(prop.Name) || reader[prop.Name] is DBNull)
                continue;

            var value = reader[prop.Name];
            prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
        }

        return obj;
    }

    public ValueTask BeginTransaction(bool serializable = false)
    {
        return _adoNetDataAccess.BeginTransaction(serializable);
    }

    public Task CommitTransaction()
    {
        return _adoNetDataAccess.CommitTransaction();
    }

    public Task RollbackTransaction()
    {
        return _adoNetDataAccess.RollbackTransaction();
    }

    public Task<T> GetById(TKey id)
    {
        return _adoNetDataAccess.ExecuteSelectByIdAsync(_getByIdProcName, MapReaderTo, new Dictionary<string, object>()
        {
            {"Id",id}
        });
    }

}
