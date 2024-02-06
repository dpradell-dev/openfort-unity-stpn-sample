using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SwapView : MonoBehaviour
{
    private SwapController _controller;
    public TMP_InputField cryptoCurrencyInput;

    private void Start()
    {
        try
        {
            _controller = FindObjectOfType<SwapController>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void OnDisable()
    {
        cryptoCurrencyInput.text = string.Empty;
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
        cryptoCurrencyInput.text = (currencyValue * GameConstants.CurrencySwapRate).ToString();
    }

    public void OnBuyButtonClick_Handler()
    {
        var balanceInt = int.Parse(cryptoCurrencyInput.text); // We know this is an Integer
        Debug.Log($"Buying crypto currency, amount: {balanceInt}");
        
        _controller.BuyCryptoCurrency(balanceInt);
    }
}
