using Maradit.Types;

namespace Maradit.Operations
{
    /// <summary>
    /// In
    /// </summary>
    public class Cancel
    {
        public Credential Credential { get; set; }
        public long MessageId { get; set; }
    }

    /// <summary>
    /// Response
    /// </summary>
    public class CancelResponse
    {
        public CancelInfo Response { get; set; }
    }

    /// <summary>
    /// Out
    /// </summary>
    public class CancelInfo
    {
        public CancelInfo()
        {
            Status = new Status();
        }

        public Status Status { get; set; }
    }
}