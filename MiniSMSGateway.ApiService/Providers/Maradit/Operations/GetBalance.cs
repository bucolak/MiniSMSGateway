using Maradit.Types;

namespace Maradit.Operations
{
    /// <summary>
    /// In
    /// </summary>
    public class GetBalance
    {
        public Credential Credential { get; set; }
    }

    /// <summary>
    /// Response
    /// </summary>
    public class GetBalanceResponse
    {
        public GetBalanceInfo Response { get; set; }
    }

    /// <summary>
    /// Out
    /// </summary>
    public class GetBalanceInfo
    {
        public GetBalanceInfo()
        {
            Balance = new Balance();
            Status = new Status();
        }

        public Balance Balance { get; set; }

        public Status Status { get; set; }
    }
}