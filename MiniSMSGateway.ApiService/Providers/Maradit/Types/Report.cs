using System.Collections.Generic;

namespace Maradit.Types
{
    public class Report
    {
        public Report()
        {
            List = new List<ReportItem>();
        }

        public List<ReportItem> List { get; set; }
    }
}