using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public static event UnityAction<string> OnPurchaseButtonClicked;
    
    private ProductCatalogItem _data;
    
    [HideInInspector]
    public ProductType productType;
    [HideInInspector]
    public string id;
    [HideInInspector]
    public string title;
    [HideInInspector]
    public string price;

    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI priceText;
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
        priceText.text = price + "$";

        switch (productType)
        {
            case ProductType.Consumable:
                itemImage.sprite = tokensSprite;
                break;
            case ProductType.NonConsumable:
                itemImage.sprite = nftSprite;
                break;
            case ProductType.Subscription:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnClick_Handler()
    {
        OnPurchaseButtonClicked?.Invoke(id);
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
