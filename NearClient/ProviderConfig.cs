using System.Net;

namespace NearClient
{
    public class ProviderConfig
    {
        public dynamic Args { get; set; }
        public ProviderType Type { get; set; }
        public WebProxy WebProxy { get; set; }
    }
}