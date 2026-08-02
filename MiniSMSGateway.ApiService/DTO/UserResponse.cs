namespace MiniSMSGateway.ApiService.DTO
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string ApiKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
