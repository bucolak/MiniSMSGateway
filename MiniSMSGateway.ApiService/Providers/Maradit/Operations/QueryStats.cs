using Maradit.Types;

namespace Maradit.Operations
{
    /// <summary>
    /// In
    /// </summary>
    public class QueryStats
    {
        public Credential Credential { get; set; }
    }

    /// <summary>
    /// Response
    /// </summary>
    public class QueryStatsResponse
    {
        public ReportStatsInfo Response { get; set; }
    }

    /// <summary>
    /// Out
    /// </summary>
    public class ReportStatsInfo
    {
        public ReportStatsInfo()
        {
            Report = new ReportStats();
            Status = new Status();
        }

        public ReportStats Report { get; set; }

        public Status Status { get; set; }
    }
}