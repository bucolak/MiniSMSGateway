using System;
using System.Collections.Generic;
using System.Text;

namespace Sms
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public DateTime createdAt = DateTime.UtcNow;
    }
}
