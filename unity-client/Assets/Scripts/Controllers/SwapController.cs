using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudCode;
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
        await CloudCodeService.Instance.CallModuleEndpointAsync(GameConstants.CurrentCloudModule, GameConstants.BuyCryptoCurrencyCloudFunctionName, functionParams);
        // Let's wait for the message from the backend coming through CloudCodeMessager.
    }
    
    private void CloudCodeMessager_OnCryptoCurrencyPurchased_Handler()
    {
        statusText.Set("Crypto currency purchased.", 3f);
        ActivateView(false);
    }
}
