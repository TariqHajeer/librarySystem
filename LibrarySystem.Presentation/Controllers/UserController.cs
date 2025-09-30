using LibrarySystem.BusinessLogic.UserUseCase;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            return Ok(await _userService.GetUsers(page, pageSize));
        }
    }
}
