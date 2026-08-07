using System.Collections.Generic;
using Maradit.Types;

namespace Maradit.Operations
{
    /// <summary>
    /// Submit message. Can specify the sender, multiple receiver and 
    /// text of the short message. Other attributes include message 
    /// priority, data coding scheme, validity period etc.
    /// </summary>
    public class SubmitData
    {
        public Credential Credential { get; set; }
        public Header Header { get; set; }
        public List<DataItem> Data { get; set; }
        public List<string> To { get; set; }
    }

    /// <summary>
    /// Response
    /// </summary>
    public class SubmitDataResponse
    {
        public SubmitInfo Response { get; set; }
    }
}