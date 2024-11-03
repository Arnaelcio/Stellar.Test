using System.Text;
using Microsoft.Extensions.Configuration;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Transactions;

namespace Stellar.Test;

internal abstract class Program
{
    public static async Task Main(string[] args)
    {
        await Start();
    }

    private static async Task Start()
    {
        while (true)
        {
            Console.WriteLine("""
                              Informe qual netork você deseja utilizar:
                              [1] TestNet
                              [2] MainNet
                              """);
            
            var input = Console.ReadLine();

            if (ValidInput(input)) continue;

            var server = Convert.ToInt32(input);
            if (server is not (1 or 2))
            {
                Console.Clear();
                Console.WriteLine("Informe um opção válida\n");

                continue;
            }

            if (server is 1)
            {
                try
                {
                    await GenerateTransactionDevNet();
                    Console.WriteLine($"\nTransação finalizada");
                }
                catch (Exception e)
                {
                    Console.Clear();
                    Console.WriteLine($"Ocorreu ao realizas a transação na TestNet:\n {e.Message}");
                }
            }
            else
            {
                try
                {
                    //TODO: caso desejar usar a mainNet, descomentar esse trecho
                    //await GenerateTransactionMainNet();
                    Console.WriteLine($"\nTransação finalizada");
                    
                }
                catch (Exception e)
                {
                    Console.Clear();
                    Console.WriteLine($"Ocorreu ao realizas a transação na MainNet:\n {e.Message}");
                }
            }

            Console.WriteLine("\n\nPressione <ENTER> iniciar uma nova transação");
            while (true)
            {
                ConsoleKeyInfo result = Console.ReadKey();
                if (result.Key == ConsoleKey.Enter)
                    Console.Clear();
                else continue;

                break;
            }
        }
    }

    private static bool ValidInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.Clear();
            Console.WriteLine("Informe um opção válida\n");

            return true;
        }

        if (input!.All(char.IsLetter))
        {
            Console.Clear();
            Console.WriteLine("Informe um opção válida\n");

            return true;
        }

        if (input is not ("1" or "2"))
        {
            Console.Clear();
            Console.WriteLine("Informe um opção válida\n");

            return true;
        }

        return false;
    }


    private static async Task GenerateTransactionDevNet()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(new string[0])
            .Build();

        // var keypairMainnet = KeyPair.Random();
        // var secretKeyMainnet = keypairMainnet.SecretSeed;
        // var publicKeyMainnet = keypairMainnet.AccountId;
        // Console.WriteLine("Account ID TestNet: " + publicKeyMainnet);
        // Console.WriteLine("Secret TestNet: " + secretKeyMainnet);

        var publicKeyTestNet = configuration["WalletSettings:PublicKeyTestNet"];
        var privateKeyTestNet = configuration["WalletSettings:PrivateKeyTestNet"];

        Console.WriteLine("Account ID TestNet: " + publicKeyTestNet);
        //Console.WriteLine("Account ID TestNet: " + privateKeyTestNet);

        Network networkTest = new Network("Test SDF Network ; September 2015");
        Server serverTest = new Server("https://horizon-testnet.stellar.org");

        AccountResponse accountResponseBefore = await serverTest.Accounts.Account(publicKeyTestNet!);

        Balance[] balancesBefore = accountResponseBefore.Balances;

        for (int i = 0; i < balancesBefore.Length; i++)
        {
            Balance asset = balancesBefore[i];
            Console.WriteLine("Asset Code: " + asset.AssetCode);
            Console.WriteLine("Asset Amount antes da transação: " + asset.BalanceString);
        }

        var message = "DEV30K";
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var messageBase64 = Convert.ToBase64String(messageBytes);

        var keypair = KeyPair.FromSecretSeed(configuration["WalletSettings:PrivateKeyTestNet"]!);

        var signature = keypair.Sign(Encoding.UTF8.GetBytes(messageBase64));

        Console.WriteLine($"Assinatura: {Convert.ToBase64String(signature)}");


        var transaction = new TransactionBuilder(accountResponseBefore)
            .AddOperation(new ManageDataOperation("desafio", signature))
            .AddMemo(Memo.Text("DEV30K"))
            .SetFee(100)
            .Build();

        transaction.Sign(keypair, networkTest);

        //var result = await serverTest.SubmitTransactionAsync(transaction);
        var result = await serverTest.SubmitTransaction(transaction);
        Console.WriteLine($"Transação enviada com sucesso para testNet: {result?.Hash} ");

        AccountResponse accountResponseLater = await serverTest.Accounts.Account(publicKeyTestNet!);

        Balance[] balancesLater = accountResponseLater.Balances;

        for (int i = 0; i < balancesLater.Length; i++)
        {
            Balance asset = balancesLater[i];
            Console.WriteLine("Asset Code: " + asset.AssetCode);
            Console.WriteLine("Asset Amount depois da transação: " + asset.BalanceString);
        }
    }

    private static async Task GenerateTransactionMainNet()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(new string[0])
            .Build();

        // var keypairMainnet = KeyPair.Random();
        // var secretKeyMainnet = keypairMainnet.SecretSeed;
        // var publicKeyMainnet = keypairMainnet.AccountId;
        // Console.WriteLine("Account ID TestNet: " + publicKeyMainnet);
        // Console.WriteLine("Secret TestNet: " + secretKeyMainnet);

        try
        {
            var publicKeyMainNet = configuration["WalletSettings:PublicKeyMainNet"];
            var privateKeyMainNet = configuration["WalletSettings:PrivateKeyMainNet"];

            Console.WriteLine("Account ID Mainnet: " + publicKeyMainNet);
            //Console.WriteLine("Account ID Mainnet: " + privateKeyMainNet);

            Network networkMainNet = new Network("Public Global Stellar Network ; September 2015");
            Server serverMainNet = new Server("https://horizon.stellar.org");

            AccountResponse accountResponseBefore = await serverMainNet.Accounts.Account(publicKeyMainNet!);

            Balance[] balancesBefore = accountResponseBefore.Balances;

            for (int i = 0; i < balancesBefore.Length; i++)
            {
                Balance asset = balancesBefore[i];
                Console.WriteLine("Asset Code: " + asset.AssetCode);
                Console.WriteLine("Asset Amount antes da transação: " + asset.BalanceString);
            }

            var message = "DEV30K";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var messageBase64 = Convert.ToBase64String(messageBytes);

            var keypair = KeyPair.FromSecretSeed(configuration["WalletSettings:PrivateKeyMainNet"]!);

            var signature = keypair.Sign(Encoding.UTF8.GetBytes(messageBase64));

            Console.WriteLine($"Assinatura: {Convert.ToBase64String(signature)}");


            var transaction = new TransactionBuilder(accountResponseBefore)
                .AddOperation(new ManageDataOperation("desafio", signature))
                .AddMemo(Memo.Text("DEV30K"))
                .SetFee(100)
                .Build();

            transaction.Sign(keypair, networkMainNet);

            var result = await serverMainNet.SubmitTransaction(transaction);
            Console.WriteLine($"Transação enviada com sucesso para Mainnet: {result?.Hash}");

            AccountResponse accountResponseLater = await serverMainNet.Accounts.Account(publicKeyMainNet!);

            Balance[] balancesLater = accountResponseLater.Balances;

            for (int i = 0; i < balancesLater.Length; i++)
            {
                Balance asset = balancesLater[i];
                Console.WriteLine("Asset Code: " + asset.AssetCode);
                Console.WriteLine("Asset Amount depois da transação: " + asset.BalanceString);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}