using Aptos.Rest.Model;
using System;
using System.Collections;

namespace Aptos.Rest
{
    /// <summary>
    /// Faucet Client for claiming APT from Devnet.
    /// To claim APT from Testnet you can visit Aptos Testnet Airdrop site.
    /// </summary>
    public class FaucetClient
    {

        /// <summary>
        /// Funds a Testnet Account
        /// </summary>
        /// <param name="address">Address that will get funded.</param>
        /// <param name="amount">Amount of APT requested.</param>
        /// <param name="endpoint">Base URL for faucet.</param>
        /// <returns>Calls <c>callback</c> function with <c>(bool, ResponsiveInfo)</c>: \n
        /// A boolean stating that the request for funding was successful, and an object containg the response details</returns>
        public async Task<bool> FundAccount(string address, int amount, string endpoint)
        {
            var faucetURL = endpoint + "/mint?amount=" + amount + "&address=" + address;
            Uri transactionsURI = new(faucetURL);

            var response = await RequestClient.PostAsync(transactionsURI, string.Empty);

            if( !response.IsSuccessStatusCode )
            {
                await Task.Delay(1000);
                return false;
            }

            await Task.Delay(2000);
            return true;
            
        }
    }
}