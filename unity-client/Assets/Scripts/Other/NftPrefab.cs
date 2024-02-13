using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NftPrefab : MonoBehaviour
{
    public static event UnityAction<string> OnSellButtonPressed;
    public TextMeshProUGUI assetTypeText;
    public TextMeshProUGUI tokenIdText;
    public TextMeshProUGUI cryptoPriceText;
    public Button sellButton;
    
    private string _tokenId;

    //TODO handle when NFT is sold or sell fails.
    
    private void OnEnable()
    {
        sellButton.interactable = true;
    }

    public void Setup(string assetType, string tokenId, string cryptoPrice)
    {
        assetTypeText.text = assetType;
        tokenIdText.text = "ID: " + tokenId;
        cryptoPriceText.text = "SELL: " + cryptoPrice;

        _tokenId = tokenId;
    }

    public void SellButton_OnClick_Handler()
    {
        sellButton.interactable = false;
        OnSellButtonPressed?.Invoke(_tokenId);
    }
}
