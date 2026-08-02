using MiniSMSGateway.ApiService.DTO;
using MiniSMSGateway.ApiService.Models;
using MiniSMSGateway.ApiService.Data;

namespace MiniSMSGateway.ApiService.Services
{
    public class UserService : IUserService
    {
        private readonly SmsDbContext _context;

        public UserService(SmsDbContext context)
        {
            _context = context;
        }

        public UserResponse CreateUser(CreateUserRequest request)
        {
            var user = new User
            {
                UserName = request.UserName,
                ApiKey = Guid.NewGuid().ToString("N"),
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var response = new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                ApiKey = user.ApiKey,
                CreatedAt = user.CreatedAt
            };
            return response;
        }
        
    }
}
