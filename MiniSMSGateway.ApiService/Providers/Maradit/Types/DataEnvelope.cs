using System.Collections.Generic;

namespace Maradit.Types
{
    public class DataEnvelope
    {
        public List<DataItem> Data { get; set; }
        public string To { get; set; }
    }
}