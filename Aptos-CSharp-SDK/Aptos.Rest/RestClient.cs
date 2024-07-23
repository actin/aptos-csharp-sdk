using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Aptos.Accounts;
using Aptos.BCS;
using Aptos.Rest.Model;
using Aptos.Rest;
using NBitcoin;
using Transaction = Aptos.Rest.Model.Transaction;

namespace Aptos.Rest
{
    /// <summary>
    /// Common configuration for clients,
    /// particularly for submitting transactions
    /// </summary>
    public static class ClientConfig
    {
        public const int EXPIRATION_TTL              = 600;
        public const int GAS_UNIT_PRICE              = 100;
        public const int MAX_GAS_AMOUNT              = 100000;
        public const int TRANSACTION_WAIT_IN_SECONDS = 20;
    }
    /*
     * {"message":"Invalid transaction: Type: Validation Code: INSUFFICIENT_BALANCE_FOR_TRANSACTION_FEE","error_code":"vm_error","vm_error_code":5}
     */
    public class ErrorInfo
    {
        [JsonProperty("message", Required = Required.AllowNull)]
        public string Message { get; set; }
        [JsonProperty("error_code", Required = Required.AllowNull)]
        public string ErrorCode { get; set; }
        [JsonProperty("vm_error_code", Required = Required.AllowNull)]
        public int VmErrorCode { get; set; }
    }

    /// <summary>
    /// The Aptos REST Client contains a set of methods
    /// for interacting with the Aptos blockchain.
    /// </summary>
    public class RestClient
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// Based endpoint for REST API.
        public Uri? Endpoint { get; private set; }

        public int? ChainId { get; private set; }

        public bool HasError => Error != null;

        public ErrorInfo? Error { get; private set; }

        public static RestClient SetEndPoint(string endpoint)
        {
            return new RestClient()
            {
                Endpoint = new Uri(endpoint)
            };
        }

        public async Task<bool> SetUpAsync()
        {
            var ledgerInfo = await GetInfoAsync();
            if (ledgerInfo != null)
            {
                this.ChainId = ledgerInfo.ChainId;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Get the latest ledger information, including data such as chain ID, role type, ledger versions, epoch, etc.
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <returns>Calls <c>callback</c>function with <c>(LedgerInfo, response)</c>: \n
        /// An object that contains the chain details - null if the request fails, and a response object that contains the response details. </returns>
        public async Task<LedgerInfo?> GetInfoAsync()
        {
            if (Endpoint == null) return null;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
                var response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LedgerInfo>(content);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get Account Details.
        /// Return the authentication key and the sequence number for an account address. Optionally, a ledger version can be specified.
        /// If the ledger version is not specified in the request, the latest ledger version is used.
        /// </summary>
        /// <param name="accountAddress">Address of the account.</param>
        /// <returns>Calls <c>callback</c> function with <c>(AccountData, ResponseInfo)</c>: \n
        /// An object that contains the account's:
        /// <c>sequence_number</c>, a string containing a 64-bit unsigned integer.
        /// Example: <code>32425224034</code>
        /// <c>authentication_key</c> All bytes (Vec) data is represented as hex-encoded string prefixed with 0x and fulfilled with two hex digits per byte.
        /// Unlike the Address type, HexEncodedBytes will not trim any zeros.
        /// Example: <code>0x88fbd33f54e1126269769780feb24480428179f552e2313fbe571b72e62a1ca1</code>, it is null if the request fails \n
        /// and a response object that contains the response details.
        /// </returns>
        public async Task<AccountData?> GetAccountAsync(AccountAddress accountAddress)
        {
            if (Endpoint == null) return null;

            try
            {
                string accountsURL = Endpoint + "/accounts/" + accountAddress.ToString();
                var request = new HttpRequestMessage(HttpMethod.Get, accountsURL);
                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AccountData>(content);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Get an account's balance.    
        /// 
        /// The <c>/account</{address}/resource/{coin_type}</c> endpoint for AptosCoin returns the following response:     
        /// Gets Account Sequence Number
        /// </summary>
        /// <param name="accountAddress">Address of the account.</param>
        /// <returns>Calls <c>callback</c> function with <c>(string, ResponseInfo)</c>: \n
        /// A Sequence number as a string - null if the request fails, and a response object containing the response details. </returns>
        public async Task<string?> GetAccountSequenceNumberAsync(AccountAddress accountAddress)
        {
            var accountData = await GetAccountAsync(accountAddress);
            return accountData?.SequenceNumber;
        }
        /// <summary>
        /// Get an account's balance.
        ///
        /// The <c>/account</{address}/resource/{coin_type}</c> endpoint for AptosCoin returns the following response:
        /// <code>
        /// {
        ///     "type":"0x1::coin::CoinStore<0x1::aptos_coin::AptosCoin>",
        ///     "data":{
        ///         "coin":{
        ///             "value":"3400034000"
        ///         },
        ///         "deposit_events":{
        ///             "counter":"68",
        ///             "guid":{
        ///                 "id":{
        ///                     "addr":"0xd89fd73ef7c058ccf79fe4c1c38507d580354206a36ae03eea01ddfd3afeef07",
        ///                     "creation_num":"2"
        ///                 }
        ///             }
        ///         },
        ///         "frozen":false,
        ///         "withdraw_events":{
        ///             "counter":"0",
        ///             "guid":{
        ///                 "id":{
        ///                     "addr":"0xd89fd73ef7c058ccf79fe4c1c38507d580354206a36ae03eea01ddfd3afeef07",
        ///                     "creation_num":"3"
        ///                 }
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </summary>
        /// <param name="accountAddress">Address of the account.</param>
        /// <returns>Calls <c>callback</c> function with <c>(AccountResourceCoin.Coin, ResponseInfo)</c>: \n
        /// A representation of the coin, and an object containing the response details.</returns>

        public async Task<AccountResourceCoin.Coin?> GetAccountBalanceAsync(AccountAddress accountAddress)
        {
            if (Endpoint == null) return null;

            try
            {
                string accountsURL = Endpoint + "/accounts/" + accountAddress.ToString() + "/resource/" + Constants.APTOS_COIN_TYPE;
                var request = new HttpRequestMessage(HttpMethod.Get, accountsURL);
                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode) return new AccountResourceCoin.Coin { Value = "0" };

                var content = await response.Content.ReadAsStringAsync();
                var acctResourceCoin = JsonConvert.DeserializeObject<AccountResourceCoin>(content);
                return acctResourceCoin?.DataProp.Coin;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get a resource of a given type from an account.
        /// NOTE: The response is a complex object of types only known to the developer writing the contracts.
        /// This function return a string and expect the developer to deserialize it into an object.
        /// See <see cref="GetAccountResourceCollection(Action{ResourceCollection, ResponseInfo}, AccountAddress, string)">GetAccountResourceCollection</see> for an example.
        /// </summary>
        /// <param name="accountAddress">Address of the account.</param>
        /// <param name="resourceType">Type of resource being queried for.</param>
        /// <returns>Calls <c>callback</c> function with <c>(bool, long, string)</c>: \n
        /// -- bool: success boolean \n
        /// -- long: - error code, string - JSON response to be deserialized by the consumer of the function\n
        /// -- string: - the response which may contain the resource details</returns>
        public async Task<string?> GetAccountResourceAsync(AccountAddress accountAddress, string resourceType = "", string ledgerVersion = "")
        {
            if (Endpoint == null) return null;

            try
            {
                string accountsURL = Endpoint + "/accounts/" + accountAddress.ToString() + "/resource/" + resourceType;
                var    request     = new HttpRequestMessage(HttpMethod.Get, accountsURL);
                var    response    = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Get resources (in a JSON format) from a given account.
        /// </summary>
        /// <param name="accountAddress">Address of the account.</param>
        /// <param name="ledgerVersion">Type of resource being queried for.</param>
        /// <returns></returns>
        public async Task<string?> GetAccountResourcesAsync(AccountAddress accountAddress, string ledgerVersion = "")
        {
            if (Endpoint == null) return null;

            try
            {
                string accountsURL = Endpoint + "/accounts/" + accountAddress.ToString() + "/resources";
                var request = new HttpRequestMessage(HttpMethod.Get, accountsURL);
                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Get Account Resource
        /// </summary>
        /// <param name="accountAddress">Address of the account.</param>
        /// <param name="resourceType">Type of resource being queried for.</param>
        /// <returns>Calls <c>callback</c> function with <c>(ResourceCollection, ResponseInfo)</c>:\n
        /// An object representing a collection resource - null if the request fails, and a response object contains the response details.</returns>
        public async Task<ResourceCollection?> GetAccountResourceCollectionAsync(AccountAddress accountAddress, string resourceType)
        {
            if (Endpoint == null) return null;

            try
            {
                string accountsURL = Endpoint + "/accounts/" + accountAddress.ToString() + "/resource/" + resourceType;
                var request = new HttpRequestMessage(HttpMethod.Get, accountsURL);
                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ResourceCollection>(content);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets table item that represents a coin resource
        /// See <see cref="GetTableItem(Action{string}, string, string, string, string)">GetTableItem</see>
        /// </summary>
        /// <param name="handle">The identifier for the given table.</param>
        /// <param name="keyType">String representation of an on-chain Move tag that is exposed in the transaction.</param>
        /// <param name="valueType">String representation of an on-chain Move type value.</param>
        /// <param name="key">The value of the table item's key, e.g. the name of a collection</param>
        /// <returns>Calls <c>callback</c> function with <c>(AccountResourceCoing, ResponseInfo)</c>:\n
        /// An object representing the account resource that holds the coin's information - null if the request fails, and a response object the contains the response details.</returns>
        public async Task<AccountResourceCoin?> GetTableItemCoinAsync(string handle, string keyType, string valueType, string key)
        {
            if (Endpoint == null) return null;

            try
            {
                string getTableItemURL = Endpoint + "/tables/" + handle + "/item";
                var tableItemRequest = new TableItemRequest
                {
                    KeyType = keyType,
                    ValueType = valueType,
                    Key = key
                };
                var content = new StringContent(JsonConvert.SerializeObject(tableItemRequest), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(getTableItemURL, content);

                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AccountResourceCoin>(responseContent);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get a  table item at a specific ledger version from the table identified
        /// by the handle {table_handle} in the path and a [simple] "key" (TableItemRequest)
        /// provided by the request body.
        ///
        /// Further details are provider <see cref="https://fullnode.devnet.aptoslabs.com/v1/spec#/operations/get_table_item">here</see>
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="handle">The identifier for the given table</param>
        /// <param name="keyType">String representation of an on-chain Move tag that is exposed in the transaction, e.g. "0x1::string::String"</param>
        /// <param name="valueType">String representation of an on-chain Move type value, e.g. "0x3::token::CollectionData"</param>
        /// <param name="key">The value of the table item's key, e.g. the name of a collection.</param>
        /// <returns>Calls <c>callback</c> function with a JSON object representing a table item - null if the request fails.</returns>
        public async Task<string?> GetTableItemAsync(string handle, string keyType, string valueType, string key)
        {
            if (Endpoint == null) return null;

            try
            {
                string getTableItemURL = Endpoint + "/tables/" + handle + "/item";
                var tableItemRequest = new TableItemRequest
                {
                    KeyType = keyType,
                    ValueType = valueType,
                    Key = key
                };
                var content = new StringContent(JsonConvert.SerializeObject(tableItemRequest), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(getTableItemURL, content);

                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get a  table item for a NFT from the table identified
        /// by the handle {table_handle} in the path and a complex key provided by the request body.
        ///
        /// See <see cref="GetTableItem(Action{string}, string, string, string, string)">GetTableItem</see> for a get table item using a generic string key.
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="handle">The identifier for the given table.</param>
        /// <param name="keyType">String representation of an on-chain Move tag that is exposed in the transaction.</param>
        /// <param name="valueType">String representation of an on-chain Move type value.</param>
        /// <param name="key">A complex key object used to search for the table item. For example:
        /// <code>
        /// {
        ///     "token_data_id":{
        ///         "creator":"0xcd7820caacab04fbf1d7e563f4d329f06d2ce3140d654729d99258b5b68a27bf",
        ///         "collection":"Alice's",
        ///         "name":"Alice's first token"
        ///     },
        ///     "property_version":"0"
        /// }
        /// </code>
        /// </param>
        /// <returns>Calls <c>callback</c> function with <c>(TableItemToken, ResponseInfo)</c>: \n
        /// An object containing the details of the token - null if the request fails, and a response object containing the response details.
        /// </returns>
        public async Task<TableItemToken?> GetTableItemNFTAsync(string handle, string keyType, string valueType, TokenIdRequest key)
        {
            if (Endpoint == null) return null;

            try
            {
                string getTableItemURL = Endpoint + "/tables/" + handle + "/item";
                var tableItemRequest = new TableItemRequestNFT
                {
                    KeyType = keyType,
                    ValueType = valueType,
                    Key = key
                };
                var content = new StringContent(JsonConvert.SerializeObject(tableItemRequest), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(getTableItemURL, content);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new TableItemToken
                    {
                        Id = new Id
                        {
                            TokenDataId = new TokenDataId
                            {
                                Creator = key.TokenDataId.Creator,
                                Collection = key.TokenDataId.Collection,
                                Name = key.TokenDataId.Name
                            }
                        },
                        Amount = "0"
                    };
                }

                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TableItemToken>(responseContent);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Get a table item that contains a token's (NFT) metadata.
        /// In this case we are using a complex key to retrieve the table item.
        ///
        /// Note: the response received from the REST <c>/table</c>  for this methods
        /// has a particular format specific to the SDK example.
        ///
        /// Developers will have to implement their own custom object's to match
        /// the properties of the NFT, meaning the content in the table item.
        ///
        /// The response for <c>/table</c> in the SDK example has the following format:
        /// <code>
        /// {
        ///     "default_properties":{
        ///         "map":{
        ///             "data":[ ]
        ///         }
        ///     },
        ///     "description":"Alice's simple token",
        ///     "largest_property_version":"0",
        ///     "maximum":"1",
        ///     "mutability_config":{
        ///         "description":false,
        ///         "maximum":false,
        ///         "properties":false,
        ///         "royalty":false,
        ///         "uri":false
        ///     },
        ///     "name":"Alice's first token",
        ///     "royalty":{
        ///         "payee_address":"0x8b8a8935cd90a87eaf47c7f309b6935e305e48b47f95982d65153b03032b3f33",
        ///         "royalty_points_denominator":"1000000",
        ///         "royalty_points_numerator":"0"
        ///     },
        ///     "supply":"1",
        ///     "uri":"https://aptos.dev/img/nyan.jpeg"
        /// }
        /// </code>
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="handle">The identifier for the given table.</param>
        /// <param name="keyType">String representation of an on-chain Move tag that is exposed in the transaction.</param>
        /// <param name="valueType">String representation of an on-chain Move type value.</param>
        /// <param name="key">A complex key object used to search for the table item. In this case it's a TokenDataId object that contains the token / collection info</param>
        /// <returns>Calls <c>callback</c> function with <c>(TableItemTokenMetadata, ResponseInfo)</c>: \n
        /// An object that represent the NFT's metadata - null if the request fails, and a response object with the response details.
        /// </returns>
        public async Task<TableItemTokenMetadata?> GetTableItemTokenDataAsync(string handle, string keyType, string valueType, TokenDataId key)
        {
            if (Endpoint == null) return null;

            try
            {
                string getTableItemURL = Endpoint + "/tables/" + handle + "/item";
                var tableItemRequest = new TableItemRequestTokenData
                {
                    KeyType = keyType,
                    ValueType = valueType,
                    Key = key
                };
                var content = new StringContent(JsonConvert.SerializeObject(tableItemRequest), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(getTableItemURL, content);

                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TableItemTokenMetadata>(responseContent);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Aggregates the values from a resource query.
        /// </summary>
        /// <param name="Callback"></param>
        /// <param name="AccountAddress"></param>
        /// <param name="ResourceType"></param>
        /// <param name="AggregatorPath"></param>
        /// <returns></returns>
        public async Task<string?> AggregatorValueAsync(AccountAddress AccountAddress, string ResourceType, List<string> AggregatorPath)
        {
            // This method is not implemented in the original code
            return null;
        }
        public async Task<string?> SimulateTransactionAsync(RawTransaction transaction, Account sender)
        {
            if (Endpoint == null) return null;

            try
            {
                byte[] emptySignature = new byte[64]; // all 0's
                var authenticator = new Authenticator(
                    new Ed25519Authenticator(
                        sender.PublicKey,
                        new Signature(emptySignature)
                    )
                );

                var signedTransaction = new SignedTransaction(transaction, authenticator);

                string simulateTxnEndpoint = Endpoint + "/transactions/simulate";
                var request = new HttpRequestMessage(HttpMethod.Post, simulateTxnEndpoint);
                request.Content = new ByteArrayContent(signedTransaction.Bytes());
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x.aptos.signed_transaction+bcs");

                var response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Submits a BCS transaction.
        /// </summary>
        /// <param name="callback">Callback function used after response is received with the JSON response.</param>
        /// <param name="SignedTransaction">The signed transaction.</param>
        /// <returns></returns>
        public async Task<string?> SubmitBCSTransactionAsync(SignedTransaction SignedTransaction)
        {
            if (Endpoint == null) return null;
     
            string submitTxnEndpoint = Endpoint + "/transactions";
            var request = new HttpRequestMessage(HttpMethod.Post, submitTxnEndpoint);
            request.Content = new ByteArrayContent(SignedTransaction.Bytes());
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x.aptos.signed_transaction+bcs");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// Execute the Move view function with the given parameters and return its execution result.
        ///
        /// Even if the function returns a single value, it will be wrapped in an array.
        ///
        /// Usage example where we determine an accounts AptosCoin balance:
        /// <code>
        /// string[] data = new string[] {};
        /// ViewRequest viewRequest = new ViewRequest();
        /// viewRequest.Function = "0x1::coin::balance";
        /// viewRequest.TypeArguments = new string[] { "0x1::aptos_coin::AptosCoin" };
        /// viewRequest.Arguments = new string[] { bobAddress.ToString() };
        /// Coroutine getBobAccountBalanceView = StartCoroutine(RestClient.Instance.View((_data, _responseInfo) =>
        /// {
        ///     data = _data;
        ///     responseInfo = _responseInfo;
        /// }, viewRequest));
        /// yield return getBobAccountBalanceView;
        /// if (responseInfo.status == ResponseInfo.Status.Failed) {
        ///     Debug.LogError(responseInfo.message);
        ///     yield break;
        /// }
        /// Debug.Log("Bob's Balance After Funding: " + ulong.Parse(data[0]));
        /// </code>
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="viewRequest">The payload for the view function</param>
        /// <returns>A vec containing the values returned from the view functions.</returns>
        public async Task<string[]?> ViewAsync(ViewRequest viewRequest)
        {
            if (Endpoint == null) return null;

            try
            {
                var viewURL = Endpoint + "/view";
                var content = new StringContent(JsonConvert.SerializeObject(viewRequest), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(viewURL, content);

                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string[]>(responseContent);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 1) Generates a transaction request \n
        /// 2) submits that to produce a raw transaction \n
        /// 3) signs the raw transaction \n
        /// 4) submits the signed transaction \n
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="sender">Account submitting the transaction.</param>
        /// <param name="payload">Transaction payload.</param>
        /// <returns>Calls <c>callback</c>function with <c>(Transaction, ResponseInfo)</c>:\n
        /// An object that represents the transaction submitted - null if the transaction fails, and a response object with the response detials.
        /// </returns>
        public async Task<Model.Transaction?> SubmitTransactionAsync(Account sender, EntryFunction entryFunction)
        {
            if (Endpoint == null) return null;

            try
            {
                ///////////////////////////////////////////////////////////////////////
                // 1) Generate a transaction request
                ///////////////////////////////////////////////////////////////////////
                string? sequenceNumber = await GetAccountSequenceNumberAsync(sender.AccountAddress);
                if (sequenceNumber == null) return null;

                var expirationTimestamp = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Constants.EXPIRATION_TTL).ToString();

                var txnRequest = new TransactionRequest
                {
                    Sender = sender.AccountAddress.ToString(),
                    SequenceNumber = sequenceNumber,
                    MaxGasAmount = Constants.MAX_GAS_AMOUNT.ToString(),
                    GasUnitPrice = Constants.GAS_UNIT_PRICE.ToString(),
                    ExpirationTimestampSecs = expirationTimestamp,
                    EntryFunction = entryFunction
                };
                ///////////////////////////////////////////////////////////////////////
                // 2) Submits that to produce a raw transaction
                ///////////////////////////////////////////////////////////////////////
                var txnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());
                var encodedSubmission = await EncodeSubmissionAsync(txnRequestJson);
                if (encodedSubmission == null) return null;

                byte[] toSign = StringToByteArray(encodedSubmission.Trim('"')[2..]);

                ///////////////////////////////////////////////////////////////////////
                // 3) Signs the raw transaction
                ///////////////////////////////////////////////////////////////////////
                Signature signature = sender.Sign(toSign);

                txnRequest.Signature = new SignatureData
                {
                    Type = Constants.ED25519_SIGNATURE,
                    PublicKey = "0x" + BitConverter.ToString(sender.PublicKey).Replace("-", "").ToLower(),
                    Signature = signature.ToString()
                };

                txnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());

                string transactionURL = Endpoint + "/transactions";
                var content = new StringContent(txnRequestJson, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(transactionURL, content);
                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Model.Transaction>(responseContent, new TransactionConverter());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// A Coroutine that polls for a transaction hash until it is confimred in the blockchain
        /// Times out if the transaction hash is not found after querying for N times.
        ///
        /// Waits for a transaction query to respond whether a transaction submitted has been confirmed in the blockchain.
        /// Queries for a given transaction hash (txnHash) using <see cref="TransactionPending"/>
        /// by polling / looping until we find a "Success" transaction response, or until it times out after <see cref="TransactionWaitInSeconds"/>.
        ///
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="txnHash">Transaction hash.</param>
        /// <returns>Calls <c>callback</c> function with (bool, ResponseInfo): \n
        /// A boolean that is: \n
        /// -- true if the transaction hash was found after polling and the transaction was succesfully commited in the blockhain \n
        /// -- false if we did not find the transaction hash and timed out \n
        ///
        /// A response object that contains the response details.
        /// </returns>
        public async Task<bool> WaitForTransactionAsync(string txnHash)
        {
            int count = 0;
            bool isTxnPending = true;

            while (isTxnPending)
            {
                var isPending = await TransactionPendingAsync(txnHash);
                if (isPending == null) return false;

                if (!isPending.Value)
                {

                    var transaction = await TransactionByHashAsync(txnHash);
                    if (transaction == null) return false;
                    return transaction.Success;
                }

                if (count > ClientConfig.TRANSACTION_WAIT_IN_SECONDS)
                {
                    return false; // Transaction timed out
                }

                count++;
                await Task.Delay(2000);
            }

            return false;
        }
        /// <summary>
        /// Query to see if transaction has been 'confirmed' in the blockchain by using the transaction hash.
        /// A 404 error will be returned if the transaction hasn't been confirmed.
        /// Once the transaction is confirmed it will have a `pending_transaction` state.
        /// </summary>
        /// <param name="txnHash">Transaction hash being queried for.</param>
        /// <returns>Calls <c>callback</c>function with <c>(bool, ResponseInfo)</c>: \n
        /// A boolean that is: \n
        /// -- true if transaction is still pending / hasn't been found, meaning 404, error in response, or `pending_transaction` is true \n
        /// -- false if transaction has been found, meaning `pending_transaction` is true \n
        ///
        /// A response object that contains the response details.
        /// </returns>
        public async Task<bool?> TransactionPendingAsync(string txnHash)
        {
            if (Endpoint == null) return null;

            try
            {
                string txnURL = Endpoint + "/transactions/by_hash/" + txnHash;
                var request = new HttpRequestMessage(HttpMethod.Get, txnURL);
                var response = await httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return true;
                }

                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var transactionResult = JsonConvert.DeserializeObject<Model.Transaction>(content, new TransactionConverter());
                return transactionResult?.Type.Equals("pending_transaction");
            }
            catch
            {
                return null;
            }
        }

        public async Task<Model.Transaction?> TransactionByHashAsync(string txnHash)
        {
            if (Endpoint == null) return null;

            try
            {
                string txnURL = Endpoint + "/transactions/by_hash/" + txnHash;
                var request = new HttpRequestMessage(HttpMethod.Get, txnURL);
                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Model.Transaction>(content, new TransactionConverter());
            }
            catch
            {
                return null;
            }
        }

        public async Task<SignedTransaction?> CreateMultiAgentBCSTransactionAsync(Account Sender, List<Account> SecondaryAccounts, BCS.TransactionPayload Payload)
        {
            try
            {
                string? sequenceNumber = await GetAccountSequenceNumberAsync(Sender.AccountAddress);
                if (sequenceNumber == null) return null;

                List<AccountAddress> secondaryAddressList = SecondaryAccounts.Select(account => account.AccountAddress).ToList();

                if (ChainId == null)
                {
                    var setupSuccess = await SetUpAsync();
                    if (!setupSuccess) return null;
                }

                ulong expirationTimestamp = (ulong)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Constants.EXPIRATION_TTL);

                MultiAgentRawTransaction rawTransaction = new MultiAgentRawTransaction(
                    new RawTransaction(
                        Sender.AccountAddress,
                        int.Parse(sequenceNumber),
                        Payload,
                        ClientConfig.MAX_GAS_AMOUNT,
                        ClientConfig.GAS_UNIT_PRICE,
                        expirationTimestamp,
                        (int)this.ChainId!
                    ),
                    new BCS.Sequence(secondaryAddressList.ToArray())
                );

                byte[] keyedTxn = rawTransaction.Keyed();

                List<Tuple<AccountAddress, Authenticator>> secondarySigners = SecondaryAccounts.Select(account =>
                    Tuple.Create<AccountAddress, Authenticator>(
                        account.AccountAddress,
                        new Authenticator(
                            new Ed25519Authenticator(account.PublicKey, account.Sign(keyedTxn))
                        )
                    )).ToList();

                Authenticator authenticator = new Authenticator(
                    new MultiAgentAuthenticator(
                        new Authenticator(
                            new Ed25519Authenticator(Sender.PublicKey, Sender.Sign(keyedTxn))
                        ),
                        secondarySigners
                    )
                );

                return new SignedTransaction(rawTransaction.Inner(), authenticator);
            }
            catch
            {
                return null;
            }
        }

        public async Task<RawTransaction?> CreateBCSTransactionAsync(Account Sender, BCS.TransactionPayload payload)
        {
            try
            {
                string? sequenceNumber = await GetAccountSequenceNumberAsync(Sender.AccountAddress);
                if (sequenceNumber == null) return null;

                if (ChainId == null)
                {
                    var setupSuccess = await SetUpAsync();
                    if (!setupSuccess) return null;
                }

                ulong expirationTimestamp = (ulong)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Constants.EXPIRATION_TTL);

                return new RawTransaction(
                    Sender.AccountAddress,
                    int.Parse(sequenceNumber),
                    payload,
                    ClientConfig.MAX_GAS_AMOUNT,
                    ClientConfig.GAS_UNIT_PRICE,
                    expirationTimestamp,
                    (int)this.ChainId!
                );
            }
            catch
            {
                return null;
            }
        }

        public async Task<SignedTransaction?> CreateBCSSignedTransactionAsync(Account Sender, BCS.TransactionPayload Payload)
        {
            try
            {
                RawTransaction? rawTransaction = await CreateBCSTransactionAsync(Sender, Payload);
                if (rawTransaction == null) return null;

                Signature signature = Sender.Sign(rawTransaction.Keyed());
                Authenticator authenticator = new Authenticator(
                    new Ed25519Authenticator(Sender.PublicKey, signature)
                );

                return new SignedTransaction(rawTransaction, authenticator);
            }
            catch
            {
                return null;
            }
        }
        public async Task<Model.Transaction?> TransferAsync(Account sender, string to, long amount)
        {
            if (!HdWallet.Utils.Utils.IsValidAddress(to))
            {
                return null;
            }

            EntryFunction payload = new EntryFunction(
                new ModuleId(AccountAddress.FromHex("0x1"), "aptos_account"),
                "transfer",
                new TagSequence(new ISerializableTag[] { }),
                new BCS.Sequence(
                    new ISerializable[]
                    {
                        AccountAddress.FromHex(to),
                        new U64((ulong)amount)
                    }
                )
            );

            return await SubmitTransactionAsync(sender, payload);
        }

        public async Task<string?> BCSTransferAsync(Account Sender, AccountAddress Recipient, int Amount)
        {
            try
            {
                ISerializable[] transactionArguments =
                {
                    Recipient,
                    new U64((ulong)Amount)
                };

                EntryFunction payload = EntryFunction.Natural(
                    new ModuleId(AccountAddress.FromHex("0x1"), "aptos_account"),
                    "transfer",
                    new TagSequence(new ISerializableTag[] { }),
                    new BCS.Sequence(transactionArguments)
                );

                var signedTransaction = await CreateBCSSignedTransactionAsync(Sender, new BCS.TransactionPayload(payload));
                if (signedTransaction == null) return null;

                return await SubmitBCSTransactionAsync(signedTransaction);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> EncodeSubmissionAsync(string txnRequestJson)
        {
            if (Endpoint == null) return null;

            try
            {
                string transactionsEncodeURL = Endpoint + "/transactions/encode_submission";
                var content = new StringContent(txnRequestJson, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(transactionsEncodeURL, content);
                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<byte[]?> EncodeSubmissionAsBytesAsync(string txnRequestJson)
        {
            if (Endpoint == null) return null;

            try
            {
                string transactionsEncodeURL = Endpoint + "/transactions/encode_submission";
                var content = new StringContent(txnRequestJson, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(transactionsEncodeURL, content);
                if (!response.IsSuccessStatusCode)
                {
                    Error = JsonConvert.DeserializeObject<ErrorInfo>(await response.Content.ReadAsStringAsync());
                    return null;
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<Model.Transaction?> CreateCollectionAsync(Account sender, string collectionName, string collectionDescription, string uri)
        {
            try
            {
                EntryFunction payload = EntryFunction.Natural(
                    new ModuleId(AccountAddress.FromHex("0x3"), "token"),
                    "create_collection_script",
                    new TagSequence(new ISerializableTag[] { }),
                    new BCS.Sequence(
                        new ISerializable[]
                        {
                            new BString(collectionName),
                            new BString(collectionDescription),
                            new BString(uri),
                            new U64(18446744073709551615),
                            new BCS.Sequence(new[] { new Bool(false), new Bool(false), new Bool(false) })
                        }
                    )
                );

                var signedTransaction = await CreateBCSSignedTransactionAsync(sender, new BCS.TransactionPayload(payload));
                if (signedTransaction == null) return null;

                var response = await SubmitBCSTransactionAsync(signedTransaction);
                if (response == null) return null;

                return JsonConvert.DeserializeObject<Model.Transaction>(response, new TransactionConverter());
            }
            catch
            {
                return null;
            }
        }

        public async Task<Model.Transaction?> CreateTokenAsync(Account senderRoyaltyPayeeAddress, string collectionName, string tokenName, string description, int supply, int max, string uri, int royaltyPointsPerMillion)
        {
            try
            {
                EntryFunction payload = EntryFunction.Natural(
                    new ModuleId(AccountAddress.FromHex("0x3"), "token"),
                    "create_token_script",
                    new TagSequence(new ISerializableTag[] { }),
                    new BCS.Sequence(
                        new ISerializable[]
                        {
                            new BString(collectionName),
                            new BString(tokenName),
                            new BString(description),
                            new U64((ulong)supply),
                            new U64((ulong)supply),
                            new BString(uri),
                            senderRoyaltyPayeeAddress.AccountAddress,
                            new U64(1000000),
                            new U64((ulong)royaltyPointsPerMillion),
                            new BCS.Sequence(new[] { new Bool(false), new Bool(false), new Bool(false), new Bool(false), new Bool(false) }),
                            new BCS.Sequence(new BString[] {}),
                            new BCS.Sequence(new Bytes[] {}),
                            new BCS.Sequence(new BString[] {})
                        }
                    )
                );

                var signedTransaction = await CreateBCSSignedTransactionAsync(senderRoyaltyPayeeAddress, new BCS.TransactionPayload(payload));
                if (signedTransaction == null) return null;

                var response = await SubmitBCSTransactionAsync(signedTransaction);
                if (response == null) return null;

                return JsonConvert.DeserializeObject<Model.Transaction>(response, new TransactionConverter());
            }
            catch
            {
                return null;
            }
        }

        public async Task<Model.Transaction?> OfferTokenAsync(Account account, AccountAddress receiver, AccountAddress creator, string collectionName, string tokenName, int amount, int propertyVersion = 0)
        {
            if (!HdWallet.Utils.Utils.IsValidAddress(receiver.ToString()) || !HdWallet.Utils.Utils.IsValidAddress(creator.ToString()))
            {
                return null;
            }

            try
            {
                EntryFunction payload = EntryFunction.Natural(
                    new ModuleId(AccountAddress.FromHex("0x3"), "token_transfers"),
                    "offer_script",
                    new TagSequence(new ISerializableTag[] { }),
                    new BCS.Sequence(
                        new ISerializable[]
                        {
                            receiver,
                            creator,
                            new BString(collectionName),
                            new BString(tokenName),
                            new U64((ulong)propertyVersion),
                            new U64((ulong)amount)
                        }
                    )
                );

                var signedTransaction = await CreateBCSSignedTransactionAsync(account, new BCS.TransactionPayload(payload));
                if (signedTransaction == null) return null;

                var response = await SubmitBCSTransactionAsync(signedTransaction);
                if (response == null) return null;

                return JsonConvert.DeserializeObject<Model.Transaction>(response, new TransactionConverter());
            }
            catch
            {
                return null;
            }
        }

        public async Task<Model.Transaction?> ClaimTokenAsync(Account account, AccountAddress sender, AccountAddress creator, string collectionName, string tokenName, int propertyVersion = 0)
        {
            if (!HdWallet.Utils.Utils.IsValidAddress(sender.ToString()) || !HdWallet.Utils.Utils.IsValidAddress(creator.ToString()))
            {
                return null;
            }

            try
            {
                EntryFunction payload = EntryFunction.Natural(
                    new ModuleId(AccountAddress.FromHex("0x3"), "token_transfers"),
                    "claim_script",
                    new TagSequence(new ISerializableTag[] { }),
                    new BCS.Sequence(
                        new ISerializable[]
                        {
                            sender,
                            creator,
                            new BString(collectionName),
                            new BString(tokenName),
                            new U64((ulong)propertyVersion)
                        }
                    )
                );

                var signedTransaction = await CreateBCSSignedTransactionAsync(account, new BCS.TransactionPayload(payload));
                if (signedTransaction == null) return null;

                var response = await SubmitBCSTransactionAsync(signedTransaction);
                if (response == null) return null;

                return JsonConvert.DeserializeObject<Model.Transaction>(response, new TransactionConverter());
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Get token information.
        /// </summary>
        /// <param name="ownerAddress">Address of token owner.</param>
        /// <param name="creatorAddress">Address of token creator.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="tokenName">Name of the token.</param>
        /// <param name="propertyVersion">Version of the token.</param>
        /// <returns>Calls <c>callback</c> function with <c>(TableItemToken, ResponseInfo)</c>: \n
        /// An object the represents the transaction submitted - null if the transaction to get a token failed, and a response object that contains the response details.
        /// </returns>
        public async Task<TableItemToken?> GetTokenAsync(AccountAddress ownerAddress, AccountAddress creatorAddress, string collectionName, string tokenName, int propertyVersion = 0)
        {
            try
            {
                var tokenStoreResourceResp = await GetAccountResourceAsync(ownerAddress, "0x3::token::TokenStore");
                if (tokenStoreResourceResp == null) return null;

                var accountResource = JsonConvert.DeserializeObject<AccountResourceTokenStore>(tokenStoreResourceResp);
                if (accountResource == null) return null;

                string tokenStoreHandle = accountResource.DataProp.Tokens.Handle;

                TokenIdRequest tokenId = new TokenIdRequest
                {
                    TokenDataId = new TokenDataId()
                    {
                        Creator = creatorAddress.ToString(),
                        Collection = collectionName,
                        Name = tokenName
                    },
                    PropertyVersion = propertyVersion.ToString()
                };

                return await GetTableItemNFTAsync(tokenStoreHandle, "0x3::token::TokenId", "0x3::token::Token", tokenId);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get balance for a given non-fungible token (NFT).
        /// </summary>
        /// <param name="ownerAddress">Address of token owner.</param>
        /// <param name="creatorAddress">Address of token creator.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="tokenName">Name of the token.</param>
        /// <param name="propertyVersion">Version of the token.</param>
        /// <returns>Token balance as a string.</returns>
        public async Task<string?> GetTokenBalanceAsync(AccountAddress ownerAddress, AccountAddress creatorAddress, string collectionName, string tokenName, int propertyVersion = 0)
        {
            try
            {
                var tableItemToken = await GetTokenAsync(ownerAddress, creatorAddress, collectionName, tokenName, propertyVersion);
                return tableItemToken?.Amount ?? "0";
            }
            catch
            {
                return "0";
            }
        }

        /// <summary>
        /// Read Collection's token data table.
        /// An example
        /// <code>
        /// {
        ///     "default_properties":{
        ///         "map":{
        ///             "data":[ ]
        ///         }
        ///     },
        ///     "description":"Alice's simple token",
        ///     "largest_property_version":"0",
        ///     "maximum":"1",
        ///     "mutability_config":{
        ///         "description":false,
        ///         "maximum":false,
        ///         "properties":false,
        ///         "royalty":false,
        ///         "uri":false
        ///     },
        ///     "name":"Alice's first token",
        ///     "royalty":{
        ///         "payee_address":"0x3f99ffee67853e129173b9df0e2e9c6af6f08fe2a4417daf43df46ad957a8bbe",
        ///         "royalty_points_denominator":"1000000",
        ///         "royalty_points_numerator":"0"
        ///     },
        ///     "supply":"1",
        ///     "uri":"https://aptos.dev/img/nyan.jpeg"
        /// }
        /// </code>
        /// </summary>
        /// <param name="creator">Address of the creator.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="tokenName">Name of the token.</param>
        /// <param name="propertyVersion">Version of the token.</param>
        /// <returns>Calls <c>callback</c> function with <c>(TableItemToken, ResponseInfo)</c>: \n
        /// An object the represents the NFT's token metadata - null if the transaction to get a token failed, and a response object that contains the response details.
        /// </returns>
        public async Task<TableItemTokenMetadata?> GetTokenDataAsync(AccountAddress creator, string collectionName, string tokenName, int propertyVersion = 0)
        {
            try
            {
                var collectionResourceResp = await GetAccountResourceAsync(creator, "0x3::token::Collections");
                if (collectionResourceResp == null) return null;

                var resourceCollection = JsonConvert.DeserializeObject<ResourceCollection>(collectionResourceResp);
                if (resourceCollection == null) return null;

                string tokenDataHandle = resourceCollection.DataProp.TokenData.Handle;

                TokenDataId tokenDataId = new TokenDataId
                {
                    Creator = creator.ToString(),
                    Collection = collectionName,
                    Name = tokenName
                };

                return await GetTableItemTokenDataAsync(tokenDataHandle, "0x3::token::TokenDataId", "0x3::token::TokenData", tokenDataId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GetCollectionAsync(AccountAddress creator, string collectionName)
        {
            try
            {
                var collectionResourceResp = await GetAccountResourceAsync(creator, "0x3::token::Collections");
                if (collectionResourceResp == null) return null;

                var resourceCollection = JsonConvert.DeserializeObject<ResourceCollection>(collectionResourceResp);
                if (resourceCollection == null) return null;

                string tokenDataHandle = resourceCollection.DataProp.CollectionData.Handle;

                return await GetTableItemAsync(tokenDataHandle, "0x1::string::String", "0x3::token::CollectionData", collectionName);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> TransferObjectAsync(Account Owner, AccountAddress Object, AccountAddress To)
        {
            try
            {
                ISerializable[] transactionArguments =
                {
                    Object,
                    To
                };

                EntryFunction payload = EntryFunction.Natural(
                    new ModuleId(AccountAddress.FromHex("0x1"), "object"),
                    Constants.APTOS_TRANSFER_CALL,
                    new TagSequence(new ISerializableTag[] { }),
                    new BCS.Sequence(transactionArguments)
                );

                var signedTransaction = await CreateBCSSignedTransactionAsync(Owner, new BCS.TransactionPayload(payload));
                if (signedTransaction == null) return null;

                return await SubmitBCSTransactionAsync(signedTransaction);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Convert byte array to string.
        /// </summary>
        /// <param name="hex">Hexadecimal string</param>
        /// <returns>Byte array representing the hex string.</returns>
        private byte[] StringToByteArray(string hex)
        {
            int    NumberChars = hex.Length;
            byte[] bytes       = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        /// <summary>
        /// Turns byte array to hexadecimal string.
        /// </summary>
        /// <param name="bytes">Byte array</param>
        /// <returns>String that represents byte array of hexadecials.</returns>
        private string ToHexadecimalRepresentation(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
        /// <summary>
        /// Utility Function coverts byte array to hex
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        private string ToHex(byte[] seed)
        {
            return BitConverter.ToString(seed).Replace("-", "").ToLower();

        }
    }
}

