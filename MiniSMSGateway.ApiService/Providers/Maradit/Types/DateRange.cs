using System;

namespace Maradit.Types
{
    public class DateRange
    {
        public DateTime? Begin{ get; set; }

        public DateTime? End { get; set; }

        public void Validate()
        {
            if (!Begin.HasValue)
            {
                Begin = DateTime.UtcNow.Date;
            }

            if ((End.HasValue && Begin.Value > End.Value) || !End.HasValue)
            {
                End = Begin.Value.AddDays(1);
            }
        }
    }
}