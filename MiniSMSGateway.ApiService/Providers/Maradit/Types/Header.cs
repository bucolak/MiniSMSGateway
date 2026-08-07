using System;

namespace Maradit.Types
{
    public class Header
    {
        /// <summary>
        /// Address of SME which originated this message.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// The short message is to be scheduled by the MC for delivery.
        /// </summary>
        public DateTime? ScheduledDeliveryTime { get; set; }

        /// <summary>
        /// The validity period in minutes relative to the time at which the SMS was received or to the time set in <see cref="ScheduledDeliveryTime"/> by our gateway. 
        /// The message will not be  delivered if it is still queued on our gateway after this time period.
        /// </summary>
        public short ValidityPeriod { get; set; }
    }
}