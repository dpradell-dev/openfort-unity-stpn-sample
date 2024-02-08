using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class AdvancedBuyPanel : MonoBehaviour
{
    public TextMeshProUGUI iapBuyPriceTag;
    public TextMeshProUGUI currencyBuyPriceTag;
    public TextMeshProUGUI cryptoCurrencyBuyPriceTag;

    private int _currencyPrice;
    private float _cryptoPrice; 
        
    public void Setup(decimal dollarPrice)
    {
        // IAP price
        iapBuyPriceTag.text = dollarPrice.ToString(CultureInfo.InvariantCulture);
        
        // Currency price
        var dollarPriceInt = Mathf.RoundToInt((float)dollarPrice);
        _currencyPrice = dollarPriceInt * GameConstants.DollarToCurrencyRate;
        currencyBuyPriceTag.text = _currencyPrice.ToString(CultureInfo.InvariantCulture);

        // Crypto price
        _cryptoPrice = (_currencyPrice * GameConstants.CurrencyToCryptoSwapRate) *
                       GameConstants.CryptoPriceReductionRate;
        cryptoCurrencyBuyPriceTag.text = _cryptoPrice.ToString(CultureInfo.InvariantCulture);
    }

    public int GetCurrencyPrice()
    {
        return _currencyPrice;
    }

    public float GetCryptoPrice()
    {
        return _cryptoPrice;
    }
}