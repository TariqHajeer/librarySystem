using System;
using LibrarySystem.DataAccess.Helpers;
using Microsoft.Data.SqlClient;

namespace LibrarySystem.BusinessLogic.Common;

public interface IService<TEntity, TKey, TCreate, TList> where TEntity:new()
{
    Task<TKey> Create(TCreate create);
    Task<PagingResult<TList>> GetPage(
    Dictionary<string, object> parameters = null,
    int? skip = null,
    int? take = null);
}
