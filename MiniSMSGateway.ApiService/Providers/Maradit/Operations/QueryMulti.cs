using Maradit.Types;

namespace Maradit.Operations
{
    /// <summary>
    /// In
    /// </summary>
    public class QueryMulti
    {
        public Credential Credential { get; set; }
        public DateRange Range { get; set; }
    }

    /// <summary>
    /// Response
    /// </summary>
    public class QueryMultiResponse
    {
        public ReportInfo Response { get; set; }
    }

    /// <summary>
    /// Out
    /// </summary>
    public class ReportInfo
    {
        public ReportInfo()
        {
            Report = new Report();
            Status = new Status();
        }

        public Report Report { get; set; }

        public Status Status { get; set; }
    }
}