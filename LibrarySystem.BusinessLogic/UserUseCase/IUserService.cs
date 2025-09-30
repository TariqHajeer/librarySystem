using LibrarySystem.BusinessLogic.Common;
using LibrarySystem.BusinessLogic.Domain;
using LibrarySystem.BusinessLogic.UserUseCase.Dtos;
using LibrarySystem.DataAccess.Helpers;

namespace LibrarySystem.BusinessLogic.UserUseCase;

public interface IUserService : IService<User, Guid, CreateUser, UserListDto>
{
    Task<PagingResult<UserListDto>> GetUsers(int? page, int? pageSize);
}
