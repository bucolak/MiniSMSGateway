using System.Collections.Generic;
using Maradit.Types;

namespace Maradit.Operations
{
    /// <summary>
    /// Submit message. Can specify the sender, multiple receiver and 
    /// text of the short message. Other attributes include message 
    /// priority, data coding scheme, validity period etc.
    /// </summary>
    public class SubmitMulti
    {
        public Credential Credential { get; set; }
        public Header Header { get; set; }
        public List<Envelope> Envelopes { get; set; }

        /// <summary>
        /// Defines the encoding scheme of the short message user data.
        /// </summary>
        public DataCoding DataCoding { get; set; }
    }

    /// <summary>
    /// Response
    /// </summary>
    public class SubmitMultiResponse
    {
        public SubmitInfo Response { get; set; }
    }
}