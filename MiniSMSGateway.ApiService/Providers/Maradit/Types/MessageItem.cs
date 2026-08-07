using System;

namespace Maradit.Types
{
    public class MessageItem
    {
        public long Id { get; set; }
        public DateTime Received { get; set; }
        public DateTime? Forwarded { get; set; }
        public short Network { get; set; }
        public string MSISDN { get; set; }
        public string Recipient { get; set; }
        public string Keyword { get; set; }
        public string Text { get; set; }
        public decimal Price { get; set; }
        public string Xser { get; set; }
    }
}