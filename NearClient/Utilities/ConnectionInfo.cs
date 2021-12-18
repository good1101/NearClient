using System;
using System.Collections.Generic;
using System.Net;

namespace NearClient.Utilities
{
    public class ConnectionInfo
    {
        public bool AllowInsecure { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Password { get; set; }
        public TimeSpan Timeout { get; set; }
        public string Url { get; set; }
        public string User { get; set; }
        public WebProxy WebProxy { get; set; }
    }
}