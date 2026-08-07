using Maradit.Types;

namespace Maradit.Operations
{
    /// <summary>
    /// In
    /// </summary>
    public class Query
    {
        public Credential Credential { get; set; }
        public long MessageId { get; set; }
        public string MSISDN { get; set; }
    }

    /// <summary>
    /// Response
    /// </summary>
    public class QueryResponse
    {
        public ReportDetailInfo Response { get; set; }
    }

    /// <summary>
    /// Out
    /// </summary>
    public class ReportDetailInfo
    {
        public ReportDetailInfo()
        {
            ReportDetail = new ReportDetail();
            Status = new Status();
        }

        public ReportDetail ReportDetail { get; set; }

        public Status Status { get; set; }
    }
}