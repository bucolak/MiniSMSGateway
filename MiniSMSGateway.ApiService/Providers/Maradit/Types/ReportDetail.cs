using System.Collections.Generic;

namespace Maradit.Types
{
    public class ReportDetail
    {
        public ReportDetail()
        {
            List = new List<ReportDetailItem>();
        }

        public List<ReportDetailItem> List { get; set; }
    }
}