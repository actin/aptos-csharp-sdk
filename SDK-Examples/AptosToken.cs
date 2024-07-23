using Aptos.Accounts;
using Aptos.Rest;
using Aptos.Rest.Model;
using Newtonsoft.Json;
using System.Diagnostics;
using Aptos.Rest.Model.Resources;
using Aptos.Sample;
using NBitcoin.RPC;
using RestClient = Aptos.Rest.RestClient;

namespace Aptos.Sample
{
    public class AptosToken
    {
        public static async Task RunAptosClientExample()
        {
            var faucetEndpoint = "https://faucet.devnet.aptoslabs.com";
            //var    faucetEndpoint = "http://192.168.50.10:8081";
            var    faucet_client  = new FaucetClient();
            var    rest_client    = RestClient.SetEndPoint(Constants.DEVNET_BASE_URL);
            //var    rest_client    = RestClient.SetEndPoint("http://192.168.50.10:8080/v1");
            var    succ           = await rest_client.SetUpAsync();

            var token_client = new AptosTokenClient();
            token_client.SetUp(rest_client);


            var alice = Account.Generate();
            var bob = Account.Generate();


            var alice_address = alice.AccountAddress;
            var bob_address   = bob.AccountAddress;

            var success = await faucet_client.FundAccount(alice_address.ToString(), 20000000, faucetEndpoint);


            success = await faucet_client.FundAccount(bob_address.ToString(), 10000000, faucetEndpoint);


            var coin = await rest_client.GetAccountBalanceAsync(alice_address);

            if (coin == null)
                return;

            Console.WriteLine("Alice's Balance After Funding: " + coin.Value);

            coin = await rest_client.GetAccountBalanceAsync(bob_address);

            if (coin == null)
                return;

            Console.WriteLine("Bob's Balance After Funding: " + coin.Value);


            Console.WriteLine("=== Creating Collection ===");
            string collectionName = "Alice's";
            string tokenName      = "Alice's first token";

            var createCollectionTxn = await token_client.CreateCollection(
                                             alice
                                             , "Alice's simple collection"
                                             , 1
                                             , collectionName
                                             , "http://ntroi.com"
                                             , true
                                            , true
                                            , true
                                            , true
                                            , true
                                            , true
                                            , true
                                            , true
                                            , true
                                            , 0, 1);
            
            if(createCollectionTxn == null)
                return;

            Console.WriteLine("Transaction Details: " + createCollectionTxn);


            CreateCollectionResponse txnResponse = JsonConvert.DeserializeObject<CreateCollectionResponse>(createCollectionTxn);

            string transactionHash = txnResponse.Hash;

            Console.WriteLine("Transaction Hash: " + transactionHash);


            bool waitForTxnSuccess = await rest_client.WaitForTransactionAsync(transactionHash);

            if (!waitForTxnSuccess)
            {
                Console.WriteLine("Transaction was not found. Breaking out of example: Error: ");
                return;
            }

            Console.WriteLine("<color=cyan>=== Get Account Resource for Alice ===</color>");

            var accountResourceResp = await rest_client.GetAccountResourceAsync(alice_address, "0x1::account::Account");

            if (accountResourceResp == null)
            {
                return;
            }

            var accountResource = JsonConvert.DeserializeObject<AccountResource>(accountResourceResp);

            if(accountResource == null)
            {
                return;
            }

            int             creationNum     = int.Parse(accountResource.Data.GuidCreationNum);
            Console.WriteLine("Creation Num: " + creationNum);

            coin = await rest_client.GetAccountBalanceAsync(alice_address);

            if (coin == null)
                return;

            Console.WriteLine("Alice's Balance After Funding: " + coin.Value);

            Console.WriteLine("<color=cyan>=== ========== ===</color>");
            Console.WriteLine("<color=cyan>=== Mint Token ===</color>");
            Console.WriteLine("<color=cyan>=== ========== ===</color>");


            var  mintTokenTxn = await token_client.MintToken(alice, collectionName, "Alice's simple token", tokenName, "https://aptos.dev/img/nyan.jpeg"
            , new PropertyMap(new List<Property> { Property.StringProp("string", "string value") }));

            if( mintTokenTxn == null)
            {
                return;
            }

            Console.WriteLine("Transaction Details: " + mintTokenTxn);

            CreateTokenResponse createTxnResponse = JsonConvert.DeserializeObject<CreateTokenResponse>(mintTokenTxn);

            string createTokenTxnHash = createTxnResponse.Hash;

            Console.WriteLine("Transaction Hash: " + createTokenTxnHash);

            waitForTxnSuccess = await rest_client.WaitForTransactionAsync(createTokenTxnHash);

            if( !waitForTxnSuccess)
            {
                Console.WriteLine("Transaction was not found. Breaking out of example: Error: ");
                return;
            }



            #region Collection Address and Token Address
            AccountAddress collectionAddress = AccountAddress.ForNamedCollection(
                    alice.AccountAddress, collectionName
                );

            var mintedTokens = await token_client.TokensMintedFromTransaction(createTokenTxnHash);

            if(mintedTokens == null)
            {
                return;
            }


            AccountAddress tokenAddress = mintedTokens[0];

            Console.WriteLine("AccountAddress ForNamedCollection: " + collectionAddress.ToString());
            Console.WriteLine("AccountAddress ForGuidObject: " + tokenAddress.ToString());
            #endregion

            #region Token Client read_object Collection
            Console.WriteLine("<color=cyan>=== ====================== ===</color>");
            Console.WriteLine("<color=cyan>=== Read Collection Object ===</color>");
            Console.WriteLine("<color=cyan>=== =================== ===</color>");

            var readObjectCollection = await token_client.ReadObject(collectionAddress);

            if(readObjectCollection == null)
            {
                return;
            }

            Console.WriteLine("Alice's collection: " + readObjectCollection);
            #endregion

            #region Token client read_object Token Address
            Console.WriteLine("<color=cyan>=== Read Token Object ===</color>");
            var readObjectToken = await token_client.ReadObject(tokenAddress);

            if(readObjectToken == null)
            {
                Console.WriteLine("ERROR: ");
                return;
            }


            Console.WriteLine("Alice's token: " + readObjectToken);
            #endregion

            #region Add token property
            Console.WriteLine(
                "<color=cyan>=== ================== ===</color>\n" +
                "<color=cyan>=== Add Token Property ===</color>\n" +
                "<color=cyan>=== ================== ===</color>"
            );

            var responseString = await token_client.AddTokenProperty(alice, tokenAddress, Property.BoolProp("test", false));
            // Add token property

            if(responseString == null)
            {
                return;
            }

            AddTokenPropertyResponse addTokenPropertyResponse = JsonConvert.DeserializeObject<AddTokenPropertyResponse>(responseString);
            transactionHash = addTokenPropertyResponse.Hash;

            // Wait for transaction
            waitForTxnSuccess = await rest_client.WaitForTransactionAsync(transactionHash);

            if (!waitForTxnSuccess)
            {
                Console.WriteLine("Transaction was not found. Breaking out of example: Error: ");
                return;
            }

            // Read token object after adding property
            readObjectToken = await token_client.ReadObject(tokenAddress);


            Console.WriteLine("Alice's token: " + readObjectToken);
            #endregion
            #region Remove token property
            Console.WriteLine("<color=cyan>=== =================== ===</color>");
            Console.WriteLine("<color=cyan>=== Remove Property ===</color>");
            Console.WriteLine("<color=cyan>=== =================== ===</color>");
            responseString = await token_client.RemoveTokenProperty(alice, tokenAddress, "string");

            // Wait for transaction
            waitForTxnSuccess = await rest_client.WaitForTransactionAsync(transactionHash);
            
            if (!waitForTxnSuccess)
            {
                Console.WriteLine("Transaction was not found. Breaking out of example: Error: ");
                return;
            }

            // Read token object after removing property
            readObjectToken = await token_client.ReadObject(tokenAddress);

            Console.WriteLine("Alice's token: " + readObjectToken);
            #endregion

            #region Update Token Property
            Console.WriteLine("<color=cyan>=== ================== ===</color>");
            Console.WriteLine("<color=cyan>=== Update Token Property ===</color>");
            Console.WriteLine("<color=cyan>=== ================== ===</color>");

            responseString = await token_client.UpdateTokenProperty(alice, tokenAddress, Property.BoolProp("test", true));

            UpdateTokenResponse UpdateTokenPropertyResponse = JsonConvert.DeserializeObject<UpdateTokenResponse>(responseString);
            transactionHash = addTokenPropertyResponse.Hash;

            // Wait for transaction
            waitForTxnSuccess = await rest_client.WaitForTransactionAsync(transactionHash);

            if (!waitForTxnSuccess)
            {
                Console.WriteLine("Transaction was not found. Breaking out of example: Error: ");
                return;
            }

            // Read token object after updating property
            readObjectToken = await token_client.ReadObject(tokenAddress);

            Console.WriteLine("Alice's token: " + readObjectToken);
            #endregion

            #region Add token property -- binary data
            Console.WriteLine(
                "<color=cyan>=== ==================================== ===</color>\n" +
                "<color=cyan>=== Add Token Property - Binary Sequence ===</color>\n" +
                "<color=cyan>=== ==================================== ===</color>\n"
            );
            // Add token property
            responseString =
                await token_client.AddTokenProperty(alice, tokenAddress, Property.BytesProp("bytes", new byte[] { 0x00, 0x01 }));


            addTokenPropertyResponse = JsonConvert.DeserializeObject<AddTokenPropertyResponse>(responseString);
            transactionHash = addTokenPropertyResponse.Hash;

            // Wait for transaction
            waitForTxnSuccess = await rest_client.WaitForTransactionAsync(transactionHash);

            if (!waitForTxnSuccess)
            {
                Console.WriteLine("Transaction was not found. Breaking out of example: Error: ");
                return;
            }

            // Read token object after adding property
            readObjectToken = await token_client.ReadObject(tokenAddress);

            Console.WriteLine("Alice's token: " + readObjectToken);
            #endregion

            #region Transferring Tokens
            Console.WriteLine(
                "<color=cyan>=== ======================================== ===</color>\n" +
                "<color=cyan>=== Transferring the Token from Alice to Bob ===</color>\n" +
                "<color=cyan>=== ======================================== ===</color>\n"
            );
            Console.WriteLine("Alice: " + alice.AccountAddress.ToString());
            Console.WriteLine("Bob: " + bob.AccountAddress.ToString());

            responseString = await rest_client.TransferObjectAsync(alice, tokenAddress, bob.AccountAddress);

            Console.WriteLine("Response: " + responseString);

            TransferObjectResponse transferObjectResponse = JsonConvert.DeserializeObject<TransferObjectResponse>(responseString);
            transactionHash = transferObjectResponse.Hash;

            // Wait for transaction
            waitForTxnSuccess = await rest_client.WaitForTransactionAsync(transactionHash);

            if (!waitForTxnSuccess)
            {
                Console.WriteLine("Transaction was not found. Breaking out of example: Error: ");
                return;
            }

            var transferObjectReadObject = await token_client.ReadObject(tokenAddress);
            
            Console.WriteLine("Alice's transferred token: " + transferObjectReadObject);
            #endregion
        }
    }
}

