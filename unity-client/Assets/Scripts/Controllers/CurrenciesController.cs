using System;
using System.Collections.Generic;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Nethereum.Util;
using Unity.Services.CloudCode;
using Unity.Services.Economy;
using UnityEngine;

public class CurrenciesController : Singleton<CurrenciesController>
{
    public StatusTextBehaviour statusText;
    
    public async UniTask<string> GetCryptoCurrencyBalance()
    {
        try
        {
            var balance = await CloudCodeService.Instance.CallModuleEndpointAsync<string>(GameConstants.CurrentCloudModule, "GetErc20Balance");

            if (string.IsNullOrEmpty(balance))
            {
                return "0";
            }
            
            // The amount in wei. Assuming it comes in wei
            BigInteger balanceInWei = BigInteger.Parse(balance);
            // Assuming decimals is the number of decimal places for the token
            int decimals = 18;
            // Convert to tokens using Nethereum
            decimal amountInTokens = UnitConversion.Convert.FromWei(balanceInWei, decimals);
                
            // Format the decimal value with two decimal places
            string formattedAmount = amountInTokens.ToString("0.00");

            return formattedAmount;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async void BuyCryptoCurrency(decimal amount)
    {
        //TODO statusText.Set("Transferring tokens...");

        var functionParams = new Dictionary<string, object> { {"amount", amount} };
        await CloudCodeService.Instance.CallModuleEndpointAsync(GameConstants.CurrentCloudModule, GameConstants.BuyCryptoCurrencyCloudFunctionName, functionParams);
        // Let's wait for the message from backend --> Inside SubscribeToCloudCodeMessages()
    }
    
    public async UniTask<string> GetCurrencyBalance()
    {
        try
        {
            // Call GetBalancesAsync with the options
            var result = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();
            
            foreach (var balance in result.Balances)
            {
                if (balance.CurrencyId == GameConstants.UgsCurrencyId)
                {
                    Debug.Log($"The balance for {GameConstants.UgsCurrencyId} is: {balance.Balance}");
                    return balance.Balance.ToString();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return null;
    }
}
