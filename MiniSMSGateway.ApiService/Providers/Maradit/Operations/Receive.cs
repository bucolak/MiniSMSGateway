using Maradit.Types;

namespace Maradit.Operations
{
    /// <summary>
    /// In
    /// </summary>
    public class Receive
    {
        public Credential Credential { get; set; }
        public DateRange Range { get; set; }
        public InboxState State { get; set; }
        public string Recipient { get; set; }
    }

    /// <summary>
    /// Response
    /// </summary>
    public class ReceiveResponse
    {
        public ReceiveInfo Response { get; set; }
    }

    /// <summary>
    /// Out
    /// </summary>
    public class ReceiveInfo
    {
        public ReceiveInfo()
        {
            Message = new MobileOrginated();
            Status = new Status();
        }

        public MobileOrginated Message { get; set; }

        public Status Status { get; set; }
    }
}