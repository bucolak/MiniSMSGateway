using Microsoft.AspNetCore.Mvc;
using MiniSMSGateway.ApiService.DTO;
using MiniSMSGateway.ApiService.Services;

namespace MiniSMSGateway.ApiService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public IActionResult CreateUser(CreateUserRequest request)
        {
            var response = _userService.CreateUser(request);
            return Ok(response);
        }

    }
}
