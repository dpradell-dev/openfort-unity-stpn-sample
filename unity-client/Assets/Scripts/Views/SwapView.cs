using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SwapView : MonoBehaviour
{
    public TMP_InputField cryptoCurrencyInput;
    public TMP_InputField currencyInput;

    public void OnCurrencyValueChanged_Handler(string currencyStringValue)
    {
        // Rate is 1:10 (Currency/CryptoCurrency)
        // TODO We should have this rate in the backend and retrieve it

        if (string.IsNullOrEmpty(currencyStringValue))
        {
            cryptoCurrencyInput.text = 0.ToString();    
            return;
        }

        var currencyValue = int.Parse(currencyStringValue); // We know this is an Integer because it's set in the TMP_InputField
        cryptoCurrencyInput.text = (currencyValue * 10).ToString();
    }

    public void BuyCryptoCurrency()
    {
        
    }
    
    /*
    private async void TransferTokens(string productId, int amount)
    {
        statusText.Set("Transferring tokens...");
        
        var functionParams = new Dictionary<string, object> { {"purchasedProductId", productId}, {"amount", amount} };
        await CloudCodeService.Instance.CallModuleEndpointAsync(GameConstants.CurrentCloudModule, "TransferTokens", functionParams);
        // Let's wait for the message from backend --> Inside SubscribeToCloudCodeMessages()
    }
    */

    public void Close()
    {
        cryptoCurrencyInput.text = string.Empty;
        gameObject.SetActive(false);
    }
}
