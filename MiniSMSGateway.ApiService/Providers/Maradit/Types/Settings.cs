using System.Collections.Generic;

namespace Maradit.Types
{
    public class Settings
    {
        public Settings()
        {
            Senders = new List<Sender>();
            Keywords = new List<Keyword>();
            OperatorSettings = new OperatorSettings();
            Balance = new Balance();
        }

        public Balance Balance { get; set; }

        public List<Sender> Senders { get; set; }

        public List<Keyword> Keywords { get; set; }

        public OperatorSettings OperatorSettings { get; set; }
    }
}