using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Openfort.Model;
using Unity.Services.CloudCode;
using Unity.Services.Economy;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class ShopController : BaseController, IDetailedStoreListener
{
    public Transform content;
    public ShopItem shopItemPrefab;
    
    private readonly List<ShopItem> _allItems = new List<ShopItem>();
    
    private IStoreController _storeController;

    private void OnEnable()
    {
        ShopItem.OnPurchaseButtonClicked += ShopItem_OnPurchaseButtonClicked_Handler;
        CloudCodeMessager.Instance.OnMintNftSuccessful += CloudCodeMessager_OnMintNftSuccessful_Handler;
    }

    private void OnDisable()
    {
        ShopItem.OnPurchaseButtonClicked -= ShopItem_OnPurchaseButtonClicked_Handler;
        CloudCodeMessager.Instance.OnMintNftSuccessful -= CloudCodeMessager_OnMintNftSuccessful_Handler;
    }

    #region GAME_EVENT_HANDLERS
    public void AuthController_OnAuthSuccess_Handler(string ofPlayerId)
    {
        InitializeIAP();
    }
    
    private void ShopItem_OnPurchaseButtonClicked_Handler(string shopItemId)
    {
        PurchaseItem(shopItemId);
    }
    
    private void CloudCodeMessager_OnMintNftSuccessful_Handler()
    {
        // We want to get the Non Consumable item, which represents the NFT
        try
        {
            var currentItem = GetShopItemByProductType(ProductType.NonConsumable);
            currentItem.MarkAsPurchased(true);
            
            statusText.Set("NFT purchased.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    #endregion
    
    private void InitializeIAP()
    {
#if UNITY_EDITOR
        StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
        StandardPurchasingModule.Instance().useFakeStoreAlways = true;
#endif
        // Configure builder
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        // Load catalog
        var catalog = ProductCatalog.LoadDefaultCatalog();

        foreach (var product in catalog.allProducts)
        {
            var newProduct = new ProductDefinition(
                product.id, 
                product.type
            );

            // Add product to builder
            builder.AddProduct(product.id, product.type);
            
            // Instantiate shop item with product data
            var intantiatedItem = Instantiate(shopItemPrefab, content);
            intantiatedItem.Setup(product);
            _allItems.Add(intantiatedItem);
        }
        
        UnityPurchasing.Initialize(this, builder);
    }
    
    public void PurchaseItem(string itemId)
    {
        var product = _storeController.products.WithID(itemId);
        if (product != null && product.availableToPurchase)
        {
            GetShopItemById(itemId).ActivateAnimation(true);
            _storeController.InitiatePurchase(product);
        }
    }

    #region BUTTON_METHODS
    public async void Activate()
    {
        viewPanel.SetActive(true);
        await CheckNonConsumableReceipt();
    }
    #endregion

    #region IAP_CALLBACKS
    public async void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAP initialized.");
        _storeController = controller;
        
        viewPanel.SetActive(true);
        
        // Checking if the non-consumable item is bought or not.
        await CheckNonConsumableReceipt();
    }
    
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("IAP initialization failed: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("IAP initialization failed." + error + message);
        //TODO something?
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        var product = purchaseEvent.purchasedProduct;
        var productId = product.definition.id;
        Debug.Log("Purchase complete: " + productId);

        //TODO at some point --> Now we are assuming all consumable products lead to transferring tokens, and all non-consumable lead to minting nft.
        switch (product.definition.type)
        {
            case ProductType.Consumable:
                //TODO
                #if UNITY_EDITOR
                IncrementCurrency(GameConstants.UgsCurrencyId, 20);
                #endif 
                break;
            case ProductType.NonConsumable:
                MintNFT();
                break;
            case ProductType.Subscription:
                // Nothing
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"Purchase of {product.definition.id} failed: " + failureReason);
    }
    
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log($"Purchase of {product.definition.id} failed: " + failureDescription);
        
        var item = GetShopItemById(product.definition.id);
        item.ActivateAnimation(false);
    }
    #endregion

    #region IAP_RELATED_METHODS
    private async Task CheckNonConsumableReceipt()
    {
        if (_storeController == null) return;

        try
        {
            var nonConsumables = _storeController.products;
            foreach (var nc in nonConsumables.all)
            {
                // TO-IMPROVE: if there's any nft in the player's inventory, we now mark ANY in-app non-consumable product as purchased.
                // We could link the in-app NC product with the NFT somehow
                if (nc.definition.type == ProductType.NonConsumable)
                {
                    if (nc.hasReceipt)
                    {
                        Debug.Log("Has receipt.");
                        // Non-consumable item has already been purchased
                        GetShopItemById(nc.definition.id).MarkAsPurchased(true);
                    }
                    else
                    {
                        Debug.Log("No receipt.");
                        
                        // If we can not retrieve the receipt, we make sure if the nft is minted or not
                        var inventoryList = await CloudCodeService.Instance.CallModuleEndpointAsync<InventoryListResponse>(GameConstants.CurrentCloudModule, "GetPlayerNftInventory");

                        if (inventoryList.Data.Count == 0)
                        {
                            // Non-consumable item has not been purchased
                            GetShopItemById(nc.definition.id).MarkAsPurchased(false);
                        }
                        else
                        {
                            // Non-consumable item has already been purchased
                            GetShopItemById(nc.definition.id).MarkAsPurchased(true);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    #endregion

    #region ECONOMY_METHODS
    private async UniTaskVoid IncrementCurrency(string currencyId, int amount)
    {
        try
        {
            // Await the asynchronous operation directly
            var result = await EconomyService.Instance.PlayerBalances.IncrementBalanceAsync(currencyId, amount);

            // Continue with the rest of the code after the await
            Debug.Log($"New balance: {result.Balance}");

            // We want to get the Consumable item which represents the currency tokens
            var item = GetShopItemByProductType(ProductType.Consumable);
            item.ActivateAnimation(false);

            statusText.Set($"{currencyId} currency purchased.");
        }
        catch (EconomyException e)
        {
            // Handle the error
            Debug.LogError($"Failed to increment balance: {e.Message}");
        }
    }
    #endregion
    
    #region CLOUD_CODE_METHODS
    private async void MintNFT()
    {
        statusText.Set("Minting NFT...");
        
        await CloudCodeService.Instance.CallModuleEndpointAsync(GameConstants.CurrentCloudModule, GameConstants.MintNftCloudFunctionName);
        // Let's wait for the message from backend --> Inside SubscribeToCloudCodeMessages()
    }
    #endregion

    #region OTHER_METHODS
    private ShopItem GetShopItemById(string id)
    {
        try
        {
            var selectedItem = _allItems.Find(item => item.id == id);
            return selectedItem;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private ShopItem GetShopItemByProductType(ProductType pType)
    {
        try
        {
            var selectedItem = _allItems.Find(item => item.productType == pType);
            return selectedItem;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    #endregion
}