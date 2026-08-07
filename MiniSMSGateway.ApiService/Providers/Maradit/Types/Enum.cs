namespace Maradit.Types
{
    public enum DataCoding
    {
        Default = 0,
        Octet = 1,
        UCS2 = 2
    }

    public enum State
    {
        Queued = 0,
        Sent = 1,
        Canceled = 2,
        Sending = 3,
        Invalid = 4,
        Receiving = 5,
        Debt = 6,
        Passive = 7,
    }

    public enum DlrState
    {
        Scheduled = 0,
        Enroute = 1,
        Delivered = 2,
        Expired = 3,
        Deleted = 4,
        Undeliverable = 5,
        Accepted = 6,
        Unknown = 7,
        Rejected = 8,
        Skipped = 9,
    }

    public enum InboxState
    {
        All = 0,
        Read = 1,
        Unread = 2
    }

    public enum Account
    {
        Postpaid = 0,
        Prepaid = 1
    }
}