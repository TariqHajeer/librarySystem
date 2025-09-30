using System;

namespace LibrarySystem.BusinessLogic.Repos;

public class RepositoryOptions<T>
{
    public string InsertProcName { get; set; }
    public string GetProcName { get; set; }
    public string GetByIdProcName { get; set; }
    public string UpdateProcName { get; set; }
}
