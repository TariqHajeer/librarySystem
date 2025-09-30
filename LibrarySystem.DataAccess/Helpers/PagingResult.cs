using System;

namespace LibrarySystem.DataAccess.Helpers;

public class PagingResult<T>
{
    public List<T> Data { get; set; }
    public int TotalCount { get; set; }
}
