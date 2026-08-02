namespace MiniSMSGateway.ApiService.DTO
{
    public class SendSmsRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
