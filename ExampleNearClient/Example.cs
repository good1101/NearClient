﻿using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using NearClient;
using NearClient.KeyStores;
using NearClient.Providers;
using NearClient.Utilities;

namespace ExampleNearClient
{
    class Example
    {
        Near _near;
        const string NETWORK_ID = "testnet";
        const string KEY_DIR = "data";
        readonly KeyStore _keyStore = new UnencryptedFileSystemKeyStore(KEY_DIR);
        string _targetAccount;
        
        public void Init()
        {
            _near = new Near(config: new NearConfig()
            {
                NetworkId = NETWORK_ID,
                NodeUrl = $"https://rpc.{NETWORK_ID}.near.org",
                ProviderType = ProviderType.JsonRpc,
                SignerType = SignerType.InMemory,
                KeyStore = _keyStore,
                WalletUrl = $"https://wallet.{NETWORK_ID}.near.org/",
                //Можно использовать например для отладки что бы просматривать запросы в Fiddler4.
               // WebProxy = new System.Net.WebProxy("127.0.0.1:8888")


            });
        }

        public async Task WaitCommand()
        {
            GetAllCommand();
            await GetAccounts();
            while (true)
            {
                string com = Console.ReadLine();
                string[] parameters = com.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var command = parameters[0].ToLower();
                try
                {
                    switch (command)
                    {
                        case "add": await AddAccount(parameters); break;
                        case "i":
                        case "info": await AccountInfo(); break;
                        case "t":
                        case "target": await Target(parameters); break;
                        case "com":
                        case "commands": GetAllCommand(); break;
                        case "accs":
                        case "accounts": await GetAccounts(); break;
                        case "send": await Send(parameters); break;
                        case "create": await CreateAccount(parameters); break;
                        case "add_key": await AddKey(parameters); break;
                        case "del_acc":
                        case "delete_account": await DeleteAccount(parameters); break;
                        case "del_key":
                        case "delete_key": await DeleteKey(parameters); break;


                        default: Console.WriteLine($"unknown '{command}'"); break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private async Task AddKey(params string[] data)
        {
            Account account = await _near.AccountAsync(_targetAccount);
            KeyPair privateKey = KeyPairEd25519.FromRandom64();
            PublicKey publicKey = privateKey.GetPublicKey();
            if (data.Length < 3)
            {
                //add full access
               await account.AddKeyAsync(publicKey.ToString());
            }
            else
            {
                //add accces key for contract
                await account.AddKeyAsync(publicKey.ToString(), methodName: data[1], contractId: data[2]);
            }
            Console.WriteLine($"Added full access key publick:{publicKey}. Privale = {privateKey} ");
        }

        private async Task DeleteKey(params string[] data)
        {
            Account account = await _near.AccountAsync(_targetAccount);
            await account.DeleteKeyAsync(publicKey: data[1]);
            Console.WriteLine($"Key deleted {data[1]}");
            var kInfo = await account.GetAccessKeysAsync();
            Console.WriteLine("Current keys:");
            foreach (var key in kInfo.keys)
            {
                Console.WriteLine($"public key: {key.public_key}");
            }

        }

        public async Task CreateAccount(params string[] data)
        {
            double amount = 0.00264;
            if (data.Length > 1)
            {
                amount = double.Parse(data[1], CultureInfo.InvariantCulture);
            }
            var nAmount = GetNearFormat(amount);
         
            KeyPair privateKey = KeyPairEd25519.FromRandom64();

            Account account = await _near.AccountAsync(_targetAccount);
            var publickKey = privateKey.GetPublicKey();
            string newid = Guid.NewGuid().ToString("N");

            var res = await account.CreateAccountAsync(newid, publickKey, nAmount);
            Console.WriteLine("Created " + newid);
            Console.WriteLine(res.Status.SuccessValue);
            await _near.Config.KeyStore.SetKeyAsync(NETWORK_ID, newid, privateKey);
            await GetAccounts();

        }

        private async Task Target(params string[] data)
        {
            int ind = -1;
            if (int.TryParse(data[1], out ind))
            {
                var accounts = await _keyStore.GetAccountsAsync(NETWORK_ID);
                _targetAccount = accounts[ind];
            }
            else
            {
                _targetAccount = data[1];
            }
            Console.WriteLine("Selected account: " + _targetAccount);
        }

        private async Task AccountInfo()
        {
            Account account = await _near.AccountAsync(_targetAccount);
            AccountState state = await account.GetStateAsync();
            foreach (var prop in state.GetType().GetProperties())
            {
                if (prop.Name == "Amount")
                {
                    Console.WriteLine($"{prop.Name}: {FormatNearAmount(prop.GetValue(state).ToString())}");
                    continue;
                }
                Console.WriteLine($"{prop.Name}: {prop.GetValue(state)}");
            }
            var kInfo = await account.GetAccessKeysAsync();
            foreach (var key in kInfo.keys)
            {
                Console.WriteLine($"public key: {key.public_key}");
            }
        }

        private async Task<string[]> GetAccounts()
        {
            var accounts = await _keyStore.GetAccountsAsync(NETWORK_ID);
            int ind = 0;
            foreach (var ac in accounts)
            {
                Console.WriteLine($"{ind++} => {ac}");
            }
            return accounts;
        }

        private async Task AddAccount(params string[] data)
        {

            KeyPair keyPair = KeyPairEd25519.FromString(data[2]);
            await _keyStore.SetKeyAsync(NETWORK_ID, data[1], keyPair);
            Console.WriteLine("Account added.");
            _targetAccount = data[1];
            await GetAccounts();
        }

        private async Task Send(params string[] data)
        {
            string am = data[2].Replace(',', '.');
            double dAmount = double.Parse(am, CultureInfo.InvariantCulture);
            Account account = await _near.AccountAsync(_targetAccount);
            var nAmount = GetNearFormat(dAmount);
            FinalExecutionOutcome execution = await account.SendMoneyAsync(data[1], nAmount);
            Console.WriteLine($"Send to {data[1]} from {_targetAccount} amount {nAmount}");
        }

        private async Task DeleteAccount(params string[] data)
        {
            Account account = await _near.AccountAsync(_targetAccount);
            await account.DeleteAccountAsync(beneficiaryId:data[1]);
            await _keyStore.RemoveKeyAsync(NETWORK_ID, _targetAccount);
            Console.WriteLine($"Deleted account {_targetAccount}.");
            Console.WriteLine("Select other account.");
            await GetAccounts();
        }

        private string GetAllCommand()
        {
            StringBuilder builder = new StringBuilder("Command list: \n\n");
            builder.Append("add - Add Account. \n").
                Append("    parameters: accountId, privatekey. \n").
                Append("    example: add ololo2.testnet ed25519:64WWsiGtbQXPVqjcQvav84kH... \n\n");
            builder.Append("info or i - Get info selected account. \n").
                Append("    parameters: \n").
                Append("    example: info \n\n");
            builder.Append("accounts - View ids addeds accounts. \n").
                Append("    parameters: \n").
                Append("    example: accounts \n\n");
            builder.Append("target or t - Select an account. \n").
                Append("    parameters: index or accountId. \n").
                Append("    example: t 0 \n\n");
            builder.Append("send - Send money. \n").
                Append("    parameters: receiverId, amount. \n").
                Append("    example: ololo2.testnet 1.1 \n\n");
            builder.Append("create - Create account. \n").
                Append("    parameters: amount. \n").
                Append("    example: create 0.1 \n\n");
            builder.Append("add_key - Add access key. \n").
                Append("    parameters: empty or (methodName, contractId). \n").
                Append("    example: add_Key \n\n");
            builder.Append("delete_account - Delete selected account. \n").
                Append("    parameters: beneficiaryId. \n").
                Append("    example: delete_account ololo2.testnet \n\n");
            builder.Append("delete_key - Delete public key. \n").
                Append("    parameters: publickKey. \n").
                Append("    example: delete_key ed25519:2XTdCoLa32LMSEMVuBK4s1B... \n\n");

            Console.WriteLine(builder.ToString());
            return builder.ToString();
        }

        private static string FormatNearAmount(string amount)
        {
            int NEAR_NOMINATION_EXP = 24;
            string val = "";
            if (amount.Length > NEAR_NOMINATION_EXP)
                val = amount.Insert(amount.Length - NEAR_NOMINATION_EXP, ".");
            else
                val = $"0.{new string('0', NEAR_NOMINATION_EXP - amount.Length)}{amount}".TrimEnd('0');
            return $"{val} Near";
        }

        public static UInt128 GetNearFormat(double amount)
        {
            UInt128 p = new UInt128( amount * 1000000000);
            UInt128.Create(out var lp, 1000000000000000);
            var res = p * lp;
            return res;
        }
    }
}
