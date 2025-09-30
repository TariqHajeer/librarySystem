using LibrarySystem.BusinessLogic.BookUseCases;
using LibrarySystem.BusinessLogic.BorrowingUseCases;
using LibrarySystem.BusinessLogic.Domain;
using LibrarySystem.BusinessLogic.Helper;
using LibrarySystem.BusinessLogic.Repos;
using LibrarySystem.BusinessLogic.Repos.Implementation;
using LibrarySystem.BusinessLogic.Repos.Interfaces;
using LibrarySystem.BusinessLogic.UserUseCase;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem.BusinessLogic;

public static class IOC
{
    public static IServiceCollection InjectApplication(this IServiceCollection services)
    {
        services.Configure<RepositoryOptions<User>>(options =>
{
    options.InsertProcName = "sp_insertUser";
    options.GetProcName = "sp_getUsers";
    options.GetByIdProcName = "sp_getUserById";
});
        services.Configure<RepositoryOptions<Book>>(options =>
{
    options.InsertProcName = "sp_insertBook";
    options.GetProcName = "sp_getBooks";
    options.UpdateProcName = "sp_updateBook";
    options.GetByIdProcName = "sp_getBookById";
    
});

        services.Configure<RepositoryOptions<Borrowing>>(options =>
        {
            options.InsertProcName = "sp_insertBorrowing";
            options.GetProcName = "sp_getBorrowings";
            options.UpdateProcName = "sp_updateBorrowing";
        });
        services.AddScoped<ILockService, KeyedLockService>();
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IBorrowingService, BorrowingService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
