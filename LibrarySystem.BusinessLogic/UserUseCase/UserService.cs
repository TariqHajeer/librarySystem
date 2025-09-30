using System;
using LibrarySystem.BusinessLogic.Common;
using LibrarySystem.BusinessLogic.Domain;
using LibrarySystem.BusinessLogic.Repos.Interfaces;
using LibrarySystem.BusinessLogic.UserUseCase.Dtos;
using LibrarySystem.DataAccess.Helpers;

namespace LibrarySystem.BusinessLogic.UserUseCase;

internal class UserService : Service<User, Guid, CreateUser, UserListDto>, IUserService
{
    public UserService(IRepository<User, Guid> repository) : base(repository)
    {
    }
    public Task<PagingResult<UserListDto>> GetUsers(int? page, int? pageSize)
    {
        int? skip = null;
        int? take = null;
        if (page.HasValue && pageSize.HasValue)
        {
            skip = (page - 1) * pageSize;
        }
        if (pageSize.HasValue)
        {
            take = pageSize;
        }
        return GetPage(null, skip, take);

    }
}
