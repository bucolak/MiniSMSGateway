using System;

namespace Maradit.Types
{
    public class ReportItem
    {
        public long Id { get; set; }
        public DateTime Received { get; set; }
        public DateTime Sent { get; set; }
        public State State { get; set; }
        public string Sender { get; set; }
        public decimal Cost { get; set; }
        public string Message { get; set; }
        public DataCoding Coding { get; set; }
        public int DeliveredCount { get; set; }
        public int Count { get; set; }
        public int UndeliveredCount { get; set; }
    }
}