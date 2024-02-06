using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SwapView : MonoBehaviour
{
    public TMP_InputField cryptoCurrencyInput;
    public TMP_InputField currencyInput;

    private void OnEnable()
    {
        CurrenciesController.Instance.OnCryptoCurrencyPurchased += CurrenciesController_OnCryptoCurrencyPurchased_Handler;
    }

    private void OnDisable()
    {
        CurrenciesController.Instance.OnCryptoCurrencyPurchased -= CurrenciesController_OnCryptoCurrencyPurchased_Handler;
    }

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

    public async void BuyCryptoCurrency()
    {
        var balance = await CurrenciesController.Instance.GetCurrencyBalance();

        if (string.IsNullOrEmpty(balance))
        {
            //TODO UI message
            Debug.Log("Balance is null or empty.");
            return;
        }

        var balanceInt = int.Parse(balance);
        CurrenciesController.Instance.BuyCryptoCurrency(balanceInt);
    }

    public void Close()
    {
        cryptoCurrencyInput.text = string.Empty;
        gameObject.SetActive(false);
    }
    
    private void CurrenciesController_OnCryptoCurrencyPurchased_Handler()
    {
        Close();
    }
}
