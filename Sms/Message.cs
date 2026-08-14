namespace Sms
{
    public class Message
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string PhoneNumber { get; set; }
        public string Content { get; set; }
        public Enum.MessageStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
    }
}
