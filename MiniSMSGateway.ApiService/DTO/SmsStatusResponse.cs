using MiniSMSGateway.ApiService.Enums;

namespace MiniSMSGateway.ApiService.DTO
{
    public class SmsStatusResponse
    {
        public int Id { get; set; }
        public MessageStatus Status { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
