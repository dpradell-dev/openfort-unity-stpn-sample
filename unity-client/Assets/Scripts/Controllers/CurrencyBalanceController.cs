using System;
using System.Globalization;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Nethereum.Util;
using TMPro;
using Unity.Services.CloudCode;
using Unity.Services.Economy;
using UnityEngine;

public class CurrencyBalanceController : BaseController
{
    public TextMeshProUGUI currencyBalanceText;
    public TextMeshProUGUI cryptoCurrencyBalanceText;
    
    public void AuthController_OnAuthSuccess_Handler(string ofPlayerId)
    {
        EconomyService.Instance.PlayerBalances.BalanceUpdated += async currencyID => 
        {
            Debug.Log($"The currency that was updated was {currencyID}");

            await UniTask.Delay(1000);
            currencyBalanceText.text = await GetCurrencyBalance();
            cryptoCurrencyBalanceText.text = await GetCryptoBalanceInString();
        };
        
        ActivateView(true);
    }

    private async void ActivateView(bool activate)
    {
        viewPanel.SetActive(activate);

        if (activate)
        {
            currencyBalanceText.text = await GetCurrencyBalance();
            cryptoCurrencyBalanceText.text = await GetCryptoBalanceInString();
        }
    }
    
    public async UniTask<string> GetCryptoBalanceInString()
    {
        try
        {
            var balance = await CloudCodeService.Instance.CallModuleEndpointAsync<string>(GameConstants.CurrentCloudModule, "GetErc20Balance");

            if (string.IsNullOrEmpty(balance))
            {
                Debug.Log($"Crypto currency balance: {balance}");
                cryptoCurrencyBalanceText.text = "0";
                return "0";
            }
            
            // The amount in wei. Assuming it comes in wei
            BigInteger balanceInWei = BigInteger.Parse(balance);
            // Assuming decimals is the number of decimal places for the token
            int decimals = 18;
            // Convert to tokens using Nethereum
            decimal balanceInTokens = UnitConversion.Convert.FromWei(balanceInWei, decimals);
            string formattedBalance = balanceInTokens.ToString(CultureInfo.InvariantCulture);
            
            Debug.Log($"Crypto currency balance: {formattedBalance}");
            cryptoCurrencyBalanceText.text = formattedBalance;
            return formattedBalance;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async UniTask<decimal> GetCryptoBalanceInDecimal()
    {
        try
        {
            var balance = await CloudCodeService.Instance.CallModuleEndpointAsync<string>(GameConstants.CurrentCloudModule, "GetErc20Balance");

            if (string.IsNullOrEmpty(balance))
            {
                Debug.Log($"Crypto currency balance: {balance}");
                return 0;
            }
            
            // The amount in wei. Assuming it comes in wei
            BigInteger balanceInWei = BigInteger.Parse(balance);
            // Assuming decimals is the number of decimal places for the token
            int decimals = 18;
            // Convert to tokens using Nethereum
            decimal balanceInTokens = UnitConversion.Convert.FromWei(balanceInWei, decimals);
            
            Debug.Log($"Crypto currency balance: {balanceInTokens}");
            return balanceInTokens;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
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
