using System;
using System.Collections.Generic;
using System.Text;

namespace Sms
{
    public class SmsCredential
    {
        public string Password { get; set; }
        public string Username { get; set; }
    }

    public class SmsHeader
    {

    }
    public class BaseSmsRquest
    {
        public SmsCredential Credential { get; set; }
        public SmsHeader Header { get; set; }
        public string Message { get; set; } = string.Empty;
        public string To { get; set; }
    }
}
