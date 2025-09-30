using System.Data;
using LibrarySystem.DataAccess.Exceptions;
using LibrarySystem.DataAccess.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LibrarySystem.DataAccess;

public class AdoNetDataAccessLayer : IAsyncDisposable
{
    private readonly SqlConnection _connection;
    private SqlTransaction _transaction;

    public AdoNetDataAccessLayer(IOptions<AdoNetOptions> options)
    {
        _connection = new SqlConnection(options.Value.ConnectionString);
    }

    public Task OpenConnection() => _connection.OpenAsync();

    public async ValueTask BeginTransaction(bool serializable = false)
    {
        if (_transaction != null)
            throw new InvalidOperationException("Transaction already in progress.");

        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();

        _transaction = (SqlTransaction)await _connection.BeginTransactionAsync(
            serializable ? IsolationLevel.Serializable : IsolationLevel.ReadCommitted
        );
    }
    public async Task<PagingResult<T>> ExecuteSelectAsync<T>(
    string storedProc,
    Func<SqlDataReader, T> map,
    Dictionary<string, object> parameters = null,
    int? skip = null,
    int? take = null)
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = storedProc;
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Transaction = _transaction;

        // Add parameters
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }
        if (skip.HasValue)
            cmd.Parameters.AddWithValue("@Skip", skip.Value);
        if (take.HasValue)
            cmd.Parameters.AddWithValue("@Take", take.Value);

        var result = new PagingResult<T>
        {
            Data = new List<T>(),
            TotalCount = 0
        };

        using var reader = await cmd.ExecuteReaderAsync();

        // Read paged data
        while (await reader.ReadAsync())
        {
            result.Data.Add(map(reader));
        }

        // Move to next result set to get total count
        if (await reader.NextResultAsync() && await reader.ReadAsync())
        {
            result.TotalCount = reader.GetInt32(0);
        }

        return result;
    }
    public async Task<T> ExecuteSelectByIdAsync<T>(
        string storedProc,
        Func<SqlDataReader, T> map,
        Dictionary<string, object> parameters = null)
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = storedProc;
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Transaction = _transaction;

        // Add parameters
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return map(reader);
        }

        return default; // null if no record found
    }


    public async Task<int> ExecuteNonQueryAsync(string storedProc, Dictionary<string, object> parameters = null)
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = storedProc;
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Transaction = _transaction;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        return await cmd.ExecuteNonQueryAsync();
    }
    public async Task<object> ExecuteScalarAsync(string storedProc, Dictionary<string, object>? parameters = null)
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = storedProc;
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Transaction = _transaction;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        try
        {
            return await cmd.ExecuteScalarAsync();
        }
        catch (SqlException ex) when (ex.Number == CustomErrorCode.DuplicationError)
        {
            // Map to a custom exception
            throw new DuplicateRecordException(ex.Message);
        }
        catch (SqlException ex)
        {
            // Optionally handle other SQL exceptions here
            throw;
        }
    }

    // Custom exception class



    public async Task CommitTransaction()
    {
        if (_transaction == null) throw new InvalidOperationException("No transaction to commit.");

        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransaction()
    {
        if (_transaction == null) throw new InvalidOperationException("No transaction to rollback.");

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }

        await _connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }

}