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
    [Header("Controllers")]
    public CurrencyBalanceController currencyBalanceController;
    
    private enum BuyType
    {
        IAP,
        Currency,
        Crypto
    }

    private BuyType _currentBuyType;
    private int _currencyBuyPrice;
    
    [Header("Shop content and items")]
    public Transform content;
    public ShopItem shopItemPrefab;
    
    private readonly List<ShopItem> _allItems = new List<ShopItem>();
    
    private IStoreController _storeController;

    private void OnEnable()
    {
        ShopItem.OnIapBuyButtonClicked += ShopItem_OnIapBuyButtonClicked_Handler;
        ShopItem.OnCurrencyBuyButtonClicked += ShopItem_OnCurrencyBuyButtonClicked_Handler;
        ShopItem.OnCryptoBuyButtonClicked += ShopItem_OnCryptoBuyButtonClicked_Handler;
        
        CloudCodeMessager.Instance.OnMintNftSuccessful += CloudCodeMessager_OnMintNftSuccessful_Handler;
        CloudCodeMessager.Instance.OnCryptoCurrencySpent += CloudCodeMessager_OnCryptoCurrencySpent_Handler;
    }

    private void OnDisable()
    {
        ShopItem.OnIapBuyButtonClicked -= ShopItem_OnIapBuyButtonClicked_Handler;
        ShopItem.OnCurrencyBuyButtonClicked -= ShopItem_OnCurrencyBuyButtonClicked_Handler;
        ShopItem.OnCryptoBuyButtonClicked -= ShopItem_OnCryptoBuyButtonClicked_Handler;
        
        CloudCodeMessager.Instance.OnMintNftSuccessful -= CloudCodeMessager_OnMintNftSuccessful_Handler;
        CloudCodeMessager.Instance.OnCryptoCurrencySpent -= CloudCodeMessager_OnCryptoCurrencySpent_Handler;
    }

    #region GAME_EVENT_HANDLERS
    public void AuthController_OnAuthSuccess_Handler(string ofPlayerId)
    {
        InitializeIAP();
    }
    
    private void ShopItem_OnIapBuyButtonClicked_Handler(string shopItemId)
    {
        PurchaseItem(shopItemId);
    }
    
    private async void ShopItem_OnCurrencyBuyButtonClicked_Handler(string shopItemId, int price)
    {
        var product = _storeController.products.WithID(shopItemId);
        if (product == null || !product.availableToPurchase)
        {
            statusText.Set("Product not available.", 2f);
            return;
        }
        
        var item = GetShopItemById(shopItemId);
        item.ActivateAnimation(true);
        
        var currencyBalanceString = await currencyBalanceController.GetCurrencyBalance();
        var currencyBalance = int.Parse(currencyBalanceString);

        if (price > currencyBalance)
        {
            statusText.Set("Not enough balance.", 3f);
            item.ActivateAnimation(false);
            return;
        }
        
        // We have enough balance so let's mint the NFT.
        _currencyBuyPrice = price;
        MintNft(BuyType.Currency);
    }
    
    private async void ShopItem_OnCryptoBuyButtonClicked_Handler(string shopItemId, float price)
    {
        var product = _storeController.products.WithID(shopItemId);
        if (product == null || !product.availableToPurchase)
        {
            statusText.Set("Product not available.", 2f);
            return;
        }
        
        var item = GetShopItemById(shopItemId);
        
        item.ActivateAnimation(true);
        var cryptoBalance = await currencyBalanceController.GetCryptoBalanceInDecimal();

        if (price > (double)cryptoBalance)
        {
            statusText.Set("Not enough balance.", 3f);
            item = GetShopItemById(shopItemId);
            item.ActivateAnimation(false);
            return;
        }
        
        SpendCryptoCurrency(BuyType.Crypto, (decimal)price);
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
                #if UNITY_EDITOR
                // Calculate currency amount to buy
                var dollarPrice = Mathf.RoundToInt((float)product.metadata.localizedPrice);
                BuyCurrency(GameConstants.UgsCurrencyId, dollarPrice * GameConstants.DollarToCurrencyRate);
                #endif 
                break;
            case ProductType.NonConsumable:
                MintNft(BuyType.IAP);
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
    private async UniTaskVoid BuyCurrency(string currencyId, int amount)
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
    
    private async UniTaskVoid DecreaseCurrencyBalance(int amount)
    {
        try
        {
            // Await the asynchronous operation directly
            var result = await EconomyService.Instance.PlayerBalances.DecrementBalanceAsync(GameConstants.UgsCurrencyId, amount);

            // Continue with the rest of the code after the await
            Debug.Log($"New balance: {result.Balance}");
        }
        catch (EconomyException e)
        {
            // Handle the error
            Debug.LogError($"Failed to increment balance: {e.Message}");
        }
    }
    #endregion
    
    #region CLOUD_CODE_METHODS
    private async void MintNft(BuyType buyType)
    {
        _currentBuyType = buyType;
        statusText.Set("Minting NFT...");
        
        await CloudCodeService.Instance.CallModuleEndpointAsync(GameConstants.CurrentCloudModule, GameConstants.MintNftCloudFunctionName);
        // Let's wait for the message from backend --> Inside SubscribeToCloudCodeMessages()
    }
    
    private async void SpendCryptoCurrency(BuyType buyType, decimal amount)
    {
        _currentBuyType = buyType;
        statusText.Set("Spending crypto currency...");
        
        var functionParams = new Dictionary<string, object> { {"amount", amount} };
        await CloudCodeService.Instance.CallModuleEndpointAsync(GameConstants.CurrentCloudModule, GameConstants.SpendCryptoCloudFunctionName, functionParams);
        // Let's wait for the message from the backend coming through CloudCodeMessager
    }
    #endregion

    #region CLOUD_CODE_CALLBACKS
    private void CloudCodeMessager_OnMintNftSuccessful_Handler()
    {
        // We want to get the Non Consumable item, which represents the NFT
        try
        {
            var currentItem = GetShopItemByProductType(ProductType.NonConsumable);
            currentItem.MarkAsPurchased(true);

            // Depending on how what BuyType we used to mint the NFT, we need to act accordingly:
            switch (_currentBuyType)
            {
                case BuyType.IAP:
                    // nothing
                    break;
                case BuyType.Currency:
                    // Decrease currency balance
                    Debug.Log($"Currency buy price is {_currencyBuyPrice}");
                    DecreaseCurrencyBalance(_currencyBuyPrice);
                    break;
                case BuyType.Crypto:
                    var newBalance = currencyBalanceController.GetCryptoBalanceInString();
                    // TODO some log?
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            statusText.Set("NFT purchased.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private void CloudCodeMessager_OnCryptoCurrencySpent_Handler(int amountSpent)
    {
        // Now we mint the NFT
        MintNft(BuyType.Crypto);
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