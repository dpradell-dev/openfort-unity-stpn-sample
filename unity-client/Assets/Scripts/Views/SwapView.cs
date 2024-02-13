using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwapView : MonoBehaviour
{
    private SwapController _swapController;
    private CurrencyBalanceController _currencyBalanceController;

    public TMP_InputField currencyInput;
    public TMP_InputField cryptoCurrencyInput;
    public TextMeshProUGUI swapStatus;

    public Button buyButton;
    public Button closeButton;

    private void Start()
    {
        try
        {
            _swapController = FindObjectOfType<SwapController>();
            _currencyBalanceController = FindObjectOfType<CurrencyBalanceController>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void OnDisable()
    {
        currencyInput.text = string.Empty;
        swapStatus.text = string.Empty;
        
        EnableButtons(true);
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
        cryptoCurrencyInput.text = (currencyValue * GameConstants.CurrencyToCryptoSwapRate).ToString();
    }

    public async void OnBuyButtonClick_Handler()
    {
        var currencySpendAmount = int.Parse(currencyInput.text);
        var currentCurrencyBalance = await _currencyBalanceController.GetCurrencyBalance();

        if (currencySpendAmount > int.Parse(currentCurrencyBalance))
        {
            swapStatus.text = "Not enough balance.";
            return;
        }
        
        var balanceInt = int.Parse(cryptoCurrencyInput.text); // We know this is an Integer
        Debug.Log($"Buying crypto currency, amount: {balanceInt}");
        
        EnableButtons(false);
        await _swapController.BuyCryptoCurrency(balanceInt);
    }

    private void EnableButtons(bool enable)
    {
        buyButton.interactable = enable;
        closeButton.interactable = enable;
    }
}
