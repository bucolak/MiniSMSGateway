namespace Sms
{
    public class Enum
    {
        public enum MessageStatus
        { 
            Pending,
            Processing,
            Sent,
            Failed
        }

        public enum ProviderType
        {
            Maradit,
            Turkcell
        }
    }
}
