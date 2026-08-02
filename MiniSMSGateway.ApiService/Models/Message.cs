using System.Threading.Tasks.Dataflow;
using MiniSMSGateway.ApiService.Enums;

namespace MiniSMSGateway.ApiService.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User user { get; set; } = null!;
        public string PhoneNumber { get; set; } = string.Empty;
        public MessageStatus Status { get; set; } = MessageStatus.Pending;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
    }
}
