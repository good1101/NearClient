using NearClient.Utilities;
using System.Threading.Tasks;

namespace NearClient
{
    public abstract class AccountCreator
    {
        public abstract Task CreateAccountAsync(string newAccountId, PublicKey publicKey);
    }
}