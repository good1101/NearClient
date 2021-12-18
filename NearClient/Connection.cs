using NearClient.KeyStores;
using NearClient.Providers;
using System;

namespace NearClient
{
    public class Connection
    {
        private readonly string _networkId;
        private readonly Provider _provider;
        private readonly Signer _signer;

        public Connection(string networkId, Provider provider, Signer signer)
        {
            _networkId = networkId;
            _provider = provider;
            _signer = signer;
        }

        public string NetworkId => _networkId;
        public Provider Provider => _provider;
        public Signer Signer => _signer;

        public static Connection FromConfig(ConnectionConfig config)
        {
            var provider = GetProvider(config.Provider);
            var signer = GetSigner(config.Signer);
            return new Connection(config.NetworkId, provider, signer);
        }

        private static Provider GetProvider(ProviderConfig config)
        {
            switch (config.Type)
            {
                case ProviderType.JsonRpc:
                    {
                        if (config.WebProxy != null)
                            return new JsonRpcProvider(config.Args.Url as string, config.WebProxy);
                        else
                            return new JsonRpcProvider(config.Args.Url as string);
                    }
                default:
                    throw new Exception($"Unknown provider type { config.Type }");
            }
        }

        private static Signer GetSigner(SignerConfig config)
        {
            switch (config.Type)
            {
                case SignerType.InMemory:
                    {
                        return new InMemorySigner(config.Args.KeyStore as KeyStore);
                    }
                default:
                    throw new Exception($"Unknown signer type {config.Type}");
            }
        }
    }
}