using System;

namespace Maradit.Types
{
    public class ReportDetailItem : DataItem
    {
        /*
        MessageId	bigint	Unchecked
        ForeignId	varchar(65)	Checked
         */
        public long Id { get; set; }
        public short Network { get; set; }
        public string MSISDN { get; set; }
        public decimal Cost { get; set; }
        public DateTime Submitted { get; set; }
        public DateTime LastUpdated { get; set; }
        public DlrState State { get; set; }
        public byte Sequence { get; set; }
        public int ErrorCode { get; set; }
    }
}