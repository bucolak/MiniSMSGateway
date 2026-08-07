using System.Collections.Generic;
using Maradit.Types;

namespace Maradit.Operations
{
    /// <summary>
    /// Submit message. Can specify the sender, multiple receiver and 
    /// text of the short message. Other attributes include message 
    /// priority, data coding scheme, validity period etc.
    /// </summary>
    public class SubmitDataMulti
    {
        public Credential Credential { get; set; }
        public Header Header { get; set; }
        public List<DataEnvelope> Envelopes { get; set; }
    }

    /// <summary>
    /// Response
    /// </summary>
    public class SubmitDataMultiResponse
    {
        public SubmitInfo Response { get; set; }
    }
}