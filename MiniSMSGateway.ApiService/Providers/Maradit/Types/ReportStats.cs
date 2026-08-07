using System.Collections.Generic;

namespace Maradit.Types
{
    public class ReportStats
    {
        public ReportStats()
        {
            List = new List<ReportStatsItem>();
        }

        public List<ReportStatsItem> List { get; set; }
    }
}