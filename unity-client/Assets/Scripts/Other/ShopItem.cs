using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public static event UnityAction<string, float> OnIapBuyButtonClicked;
    public static event UnityAction<string, int, float> OnCurrencyBuyButtonClicked;
    public static event UnityAction<string, float> OnCryptoBuyButtonClicked;
    
    private ProductCatalogItem _data;
    
    [HideInInspector]
    public ProductType productType;
    [HideInInspector]
    public string id;
    [HideInInspector]
    public string title;
    [HideInInspector]
    public string price;

    public AdvancedBuyPanel advancedBuyPanel;

    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI priceText;
    public Button iapBuyButton;
    public Button generalBuyButton;
    public Image itemImage;
    public GameObject purchasedImg;
    public GameObject purchasingAnim;

    [Header("Sprites")]
    public Sprite tokensSprite;
    public Sprite nftSprite;

    public void Setup(ProductCatalogItem itemData)
    {
        _data = itemData;
        
        // Save data
        productType = _data.type;
        id = _data.id;
        title = _data.defaultDescription.Title;
        price = _data.googlePrice.value.ToString(CultureInfo.InvariantCulture);
        
        // Set UI
        titleText.text = title;
        priceText.text = price;

        switch (productType)
        {
            case ProductType.Consumable:
                // In this demo, consumable products can only be bought with IAP
                iapBuyButton.gameObject.SetActive(true);
                itemImage.sprite = tokensSprite;
                break;
            case ProductType.NonConsumable:
                // In this demo, non consumable products can be bought with many options
                generalBuyButton.gameObject.SetActive(true);
                itemImage.sprite = nftSprite;
                
                // We also need to set up the UI of BuyNftPanel depending on price $
                advancedBuyPanel.Setup(_data.googlePrice.value);
                break;
            case ProductType.Subscription:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnIapButtonClick_Handler()
    {
        OnIapBuyButtonClicked?.Invoke(id, advancedBuyPanel.GetCryptoPrice());
    }
    
    public void OnCurrencyBuyButtonClicked_Handler()
    {
        OnCurrencyBuyButtonClicked?.Invoke(id, advancedBuyPanel.GetCurrencyPrice(), advancedBuyPanel.GetCryptoPrice());
    }
    
    public void OnCryptoBuyButtonClicked_Handler()
    {
        OnCryptoBuyButtonClicked?.Invoke(id, advancedBuyPanel.GetCryptoPrice());
    }
    
    public void MarkAsPurchased(bool status)
    {
        purchasedImg.SetActive(status);
        purchasingAnim.SetActive(false);
    }

    public void ActivateAnimation(bool status)
    {
        purchasingAnim.SetActive(status);
    }
}
