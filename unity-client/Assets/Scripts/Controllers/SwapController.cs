using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.Economy;
using UnityEngine;

public class SwapController : BaseController
{
    private void OnEnable()
    {
        CloudCodeMessager.Instance.OnCryptoCurrencyPurchased += CloudCodeMessager_OnCryptoCurrencyPurchased_Handler;
    }

    private void OnDisable()
    {
        CloudCodeMessager.Instance.OnCryptoCurrencyPurchased -= CloudCodeMessager_OnCryptoCurrencyPurchased_Handler;
    }

    public void ActivateView(bool activate)
    {
        viewPanel.SetActive(activate);    
    }
    
    public async void BuyCryptoCurrency(decimal amount)
    {
        statusText.Set("Buying crypto currency...");

        var functionParams = new Dictionary<string, object> { {"amount", amount} };
        await CloudCodeService.Instance.CallModuleEndpointAsync(GameConstants.CurrentCloudModule, GameConstants.BuyCryptoCloudFunctionName, functionParams);
        // Let's wait for the message from the backend coming through CloudCodeMessager
    }
    
    private async UniTaskVoid DecreaseCurrencyBalance(int amount)
    {
        try
        {
            // Await the asynchronous operation directly
            var result = await EconomyService.Instance.PlayerBalances.DecrementBalanceAsync(GameConstants.UgsCurrencyId, amount);

            // Continue with the rest of the code after the await
            Debug.Log($"New balance: {result.Balance}");
        }
        catch (EconomyException e)
        {
            // Handle the error
            Debug.LogError($"Failed to increment balance: {e.Message}");
        }
    }
    
    private void CloudCodeMessager_OnCryptoCurrencyPurchased_Handler(int amountPurchased)
    {
        // We successfully bought crypto currency so we need to decrease currency
        
        // Rate is 1:10 (Currency/CryptoCurrency)
        // TODO We should have this rate in the backend and retrieve it
        DecreaseCurrencyBalance(amountPurchased / GameConstants.CurrencyToCryptoSwapRate);
        
        statusText.Set("Crypto currency purchased.", 3f);
        ActivateView(false);
    }
}
