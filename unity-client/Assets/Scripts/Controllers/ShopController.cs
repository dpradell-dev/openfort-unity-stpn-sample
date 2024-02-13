using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Openfort.Model;
using Unity.Services.CloudCode;
using Unity.Services.Economy;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class ShopController : BaseController, IDetailedStoreListener
{
    public event UnityAction<string> OnNftPurchaseStarted;
    
    [Header("Controllers")]
    public CurrencyBalanceController currencyBalanceController;
    public SwapController swapController;
    
    private enum BuyType
    {
        IAP,
        Currency,
        Crypto
    }

    private BuyType _currentBuyType;
    private int _currencyBuyPrice;
    private decimal _currentMintPrice;
    
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
        CloudCodeMessager.Instance.OnSellNftSuccessful += CloudCodeMessager_OnSellNftSuccessful_Handler;
        CloudCodeMessager.Instance.OnCryptoCurrencySpent += CloudCodeMessager_OnCryptoCurrencySpent_Handler;
        CloudCodeMessager.Instance.OnCryptoCurrencyReceived += CloudCodeMessager_OnCryptoCurrencyReceived;
    }

    private void OnDisable()
    {
        ShopItem.OnIapBuyButtonClicked -= ShopItem_OnIapBuyButtonClicked_Handler;
        ShopItem.OnCurrencyBuyButtonClicked -= ShopItem_OnCurrencyBuyButtonClicked_Handler;
        ShopItem.OnCryptoBuyButtonClicked -= ShopItem_OnCryptoBuyButtonClicked_Handler;
        
        CloudCodeMessager.Instance.OnMintNftSuccessful -= CloudCodeMessager_OnMintNftSuccessful_Handler;
        CloudCodeMessager.Instance.OnSellNftSuccessful -= CloudCodeMessager_OnSellNftSuccessful_Handler;
        CloudCodeMessager.Instance.OnCryptoCurrencySpent -= CloudCodeMessager_OnCryptoCurrencySpent_Handler;
        CloudCodeMessager.Instance.OnCryptoCurrencyReceived -= CloudCodeMessager_OnCryptoCurrencyReceived;
    }

    #region GAME_EVENT_HANDLERS
    public void AuthController_OnAuthSuccess_Handler(string ofPlayerId)
    {
        InitializeIAP();
    }
    
    private void ShopItem_OnIapBuyButtonClicked_Handler(string shopItemId, float shopItemCryptoPrice)
    {
        _currentMintPrice = (decimal)shopItemCryptoPrice; // We will need it later when we sell the nft
        // We want to notify other controllers on the nft price
        OnNftPurchaseStarted?.Invoke(_currentMintPrice.ToString(CultureInfo.InvariantCulture));
        
        PurchaseItem(shopItemId);
    }
    
    private async void ShopItem_OnCurrencyBuyButtonClicked_Handler(string shopItemId, int price, float shopItemCryptoPrice)
    {
        _currentMintPrice = (decimal)shopItemCryptoPrice; // We will need it later when we sell the nft
        // We want to notify other controllers on the nft price
        OnNftPurchaseStarted?.Invoke(_currentMintPrice.ToString(CultureInfo.InvariantCulture));
        
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
        _currentMintPrice = (decimal)price; // We will need it later when we sell the nft
        // We want to notify other controllers on the nft price
        OnNftPurchaseStarted?.Invoke(_currentMintPrice.ToString(CultureInfo.InvariantCulture));
        
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
#if UNITY_EDITOR || UNITY_WEBGL
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
            Debug.Log("IAP product: " + product.id);
            
            // Instantiate shop item with product data
            var intantiatedItem = Instantiate(shopItemPrefab, content);
            intantiatedItem.Setup(product);
            _allItems.Add(intantiatedItem);
        }
        
        UnityPurchasing.Initialize(this, builder);
    }
    
    public void PurchaseItem(string itemId)
    {
        statusText.Set("Purchasing item...", 10f);
        
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
                /*
                // Calculate currency amount to buy
                var dollarPrice = Mathf.RoundToInt((float)product.metadata.localizedPrice);
                BuyCurrency(GameConstants.UgsCurrencyId, dollarPrice * GameConstants.DollarToCurrencyRate);
                */
                
                // Fixed amount of gold currency coins
                BuyCurrency(GameConstants.UgsCurrencyId, 50);
                break;
            case ProductType.NonConsumable:
                // Mint the NFT
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
                if (nc.definition.type == ProductType.NonConsumable)
                {
                    var containsNft = await CheckPlayerInventory();
                    if (containsNft)
                    {
                        // Non-consumable item has already been purchased
                        GetShopItemById(nc.definition.id).MarkAsPurchased(true);
                        return;
                    }
                    
                    if (nc.hasReceipt)
                    {
                        Debug.Log("Has receipt.");
                    
                        var txId = ExtractTxIdFromReceipt(nc.receipt);
                    
                        // Check with cloud save to see if it's the same txId
                        try
                        {
                            var savedTxId = await CloudSaveHelper.LoadFromCloud(GameConstants.ReceiptTransactionIdKey);
                            Debug.Log(savedTxId);
                            
                            if (txId == savedTxId)
                            {
                                // Non-consumable item has already been purchased
                                GetShopItemById(nc.definition.id).MarkAsPurchased(true);
                            }
                            else
                            {
                                Debug.Log("Receipt tx ID's are not the same.");
                                // It's not the product receipt of this player.
                                GetShopItemById(nc.definition.id).MarkAsPurchased(false);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            GetShopItemById(nc.definition.id).MarkAsPurchased(false);
                            throw;
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
    //TODO use swap controller?
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
    
    private async UniTask ReceiveCryptoCurrency(decimal amount)
    {
        statusText.Set("Receiving crypto currency...");

        try
        {
            var functionParams = new Dictionary<string, object> { {"amount", amount} };
            await CloudCodeService.Instance.CallModuleEndpointAsync(GameConstants.CurrentCloudModule, GameConstants.ReceiveCryptoCloudFunctionName, functionParams);
            // Let's wait for the message from the backend coming through CloudCodeMessager
        }
        catch (Exception e)
        {
            if (e.Message.Contains("timeout"))
            {
                // Sometimes Cloud Code calls reach timeout as they're interacting with the blockchain (minting, transferring, etc.)
                Debug.Log("timeout. keep waiting");
            }
            else
            {
                // It's a bad error
                statusText.Set("Transaction failed.");
                Console.WriteLine(e);
                throw;
            }
        }
    }
    #endregion

    #region CLOUD_CODE_CALLBACKS
    private async void CloudCodeMessager_OnMintNftSuccessful_Handler()
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
                    // Save the product receipt txId to Unity player cloud data
                    var product = _storeController.products.WithID(currentItem.id);
                    var txId = ExtractTxIdFromReceipt(product.receipt);
                    await CloudSaveHelper.SaveToCloud(GameConstants.ReceiptTransactionIdKey, txId);
                    break;
                case BuyType.Currency:
                    // Decrease currency balance
                    Debug.Log($"Currency buy price is {_currencyBuyPrice}");
                    await swapController.DecreaseCurrencyBalance(_currencyBuyPrice);
                    break;
                case BuyType.Crypto:
                    await UniTask.Delay(1000);
                    await currencyBalanceController.GetCryptoBalanceInString();
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
    
    private async void CloudCodeMessager_OnSellNftSuccessful_Handler(string soldTokenId)
    {
        Debug.Log($"Last mint nft price: {_currentMintPrice}");
        
        // We can remove ReceiptTxId from cloud save
        await CloudSaveHelper.DeleteFromCloud(GameConstants.ReceiptTransactionIdKey);
        
        // receive crypto currency from Treasury
        await ReceiveCryptoCurrency(_currentMintPrice);
    }
    
    private void CloudCodeMessager_OnCryptoCurrencySpent_Handler(int amountSpent)
    {
        // Now we mint the NFT
        MintNft(BuyType.Crypto);
    }
    
    private async void CloudCodeMessager_OnCryptoCurrencyReceived(int amount)
    {
        statusText.Set("Crypto currency received.");

        await UniTask.Delay(1000);
        await CheckNonConsumableReceipt();
        await currencyBalanceController.GetCryptoBalanceInString();
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

    private async UniTask<bool> CheckPlayerInventory()
    {
        var inventoryList = await CloudCodeService.Instance.CallModuleEndpointAsync<InventoryListResponse>(GameConstants.CurrentCloudModule, "GetPlayerNftInventory");
        
        if (inventoryList.Data.Count == 0)
        {
            return false;
        }

        return true;
    }
    
    private string ExtractTxIdFromReceipt(string receiptRaw)
    {
        var receipt = MiniJson.JsonDecode(receiptRaw) as Dictionary<string, object>;
        if (receipt != null && receipt.TryGetValue(GameConstants.ReceiptTransactionIdKey, out object txId))
        {
            Debug.Log($"Transaction ID: {txId}");
            return txId.ToString();
        }
        
        Debug.LogError("Error: Transaction ID not found in receipt");
        return null;
    }
    #endregion
}