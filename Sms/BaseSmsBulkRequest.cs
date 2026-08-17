namespace Sms
{
    public class SmsEnvelopes
    {
        public string Message { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }
    public class BaseSmsBulkRequest
    {
        public SmsCredential Credential { get; set; }
        public SmsHeader Header { get; set; }
        public List<SmsEnvelopes> Envelopes { get; set; }
        public string Message { get; set; }
        public List<string> Numbers { get; set; }
    }
}
