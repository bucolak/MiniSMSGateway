using MiniSMSGateway.ApiService.DTO;

namespace MiniSMSGateway.ApiService.Services
{
    public interface IUserService
    {
        UserResponse CreateUser(CreateUserRequest request);
    }
}
