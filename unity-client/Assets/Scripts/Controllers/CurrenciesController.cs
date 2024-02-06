using System;
using Cysharp.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.Economy;
using UnityEngine;

public class CurrenciesController : Singleton<CurrenciesController>
{
    public async UniTask<string> GetErc20Balance()
    {
        try
        {
            var balance = await CloudCodeService.Instance.CallModuleEndpointAsync<string>(GameConstants.CurrentCloudModule, "GetErc20Balance");
            return string.IsNullOrEmpty(balance) ? null : balance;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private async UniTask<string> GetInGameCurrencyBalance()
    {
        try
        {
            // Call GetBalancesAsync with the options
            var result = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();
            
            foreach (var balance in result.Balances)
            {
                if (balance.CurrencyId == "GOLD")
                {
                    Debug.Log($"The balance for GOLD is: {balance.Balance}");
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
