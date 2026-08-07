using System.Collections.Generic;

namespace Maradit.Types
{
    public class MobileOrginated
    {
        public MobileOrginated()
        {
            List = new List<MessageItem>();
        }

        public List<MessageItem> List { get; set; }
    }
}