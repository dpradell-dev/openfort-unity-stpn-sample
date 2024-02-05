using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Openfort.Model;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.Subscriptions;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
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
    }

    private void OnDisable()
    {
        ShopItem.OnPurchaseButtonClicked -= ShopItem_OnPurchaseButtonClicked_Handler;
    }

    #region GAME_EVENT_HANDLERS
    public async void AuthController_OnAuthSuccess_Handler(string ofPlayerId)
    {
        InitializeIAP();
        await SubscribeToCloudCodeMessages();
    }
    
    private void ShopItem_OnPurchaseButtonClicked_Handler(string shopItemId)
    {
        PurchaseItem(shopItemId);
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
                IncrementCurrency("GOLD", 20);
                #endif 
                break;
            case ProductType.NonConsumable:
                MintNFT(productId);
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
                        var inventoryList = await CloudCodeService.Instance.CallModuleEndpointAsync<InventoryListResponse>(CurrentCloudModule, "GetPlayerNftInventory");

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
    private void IncrementCurrency(string currencyId, int amount)
    {
        try
        {
            var incrementBalanceTask = EconomyService.Instance.PlayerBalances.IncrementBalanceAsync(currencyId, amount);

            incrementBalanceTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    // Handle the error
                    Debug.LogError(task.Exception);
                }
                
                Debug.Log($"New balance: {task.Result.Balance}");

                //TODO això
                var item = GetShopItemById(currencyId);
                item.ActivateAnimation(false);
                
                statusText.Set("Tokens purchased.");
            });
        }
        catch (EconomyException e)
        {
            Debug.LogError($"Failed to increment balance: {e.Message}");
        }
    }
    
    private void GetInGameCurrencyBalance()
    {
        // Call GetBalancesAsync with the options
        var getBalancesTask = EconomyService.Instance.PlayerBalances.GetBalancesAsync();

        getBalancesTask.ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Handle the error
                Debug.LogError(task.Exception);
            }
            else
            {
                // Get the result and find the balance for the "GOLD" currency
                var balancesResult = task.Result;
                foreach (var balance in balancesResult.Balances)
                {
                    if (balance.CurrencyId == "GOLD")
                    {
                        Debug.Log($"The balance for GOLD is: {balance.Balance}");
                        // Do something with the balance
                    }
                }
            }
        });
    }

    #endregion
    
    #region CLOUD_CODE_METHODS
    private async void MintNFT(string productId)
    {
        statusText.Set("Minting NFT...");
        
        var functionParams = new Dictionary<string, object> { {"purchasedProductId", productId} };
        await CloudCodeService.Instance.CallModuleEndpointAsync(CurrentCloudModule, "MintNFT", functionParams);
        // Let's wait for the message from backend --> Inside SubscribeToCloudCodeMessages()
    }

    private async void TransferTokens(string productId, int amount)
    {
        statusText.Set("Transferring tokens...");
        
        var functionParams = new Dictionary<string, object> { {"purchasedProductId", productId}, {"amount", amount} };
        await CloudCodeService.Instance.CallModuleEndpointAsync(CurrentCloudModule, "TransferTokens", functionParams);
        // Let's wait for the message from backend --> Inside SubscribeToCloudCodeMessages()
    }
    
    private Task SubscribeToCloudCodeMessages()
    {
        // Register callbacks, which are triggered when a player message is received
        var callbacks = new SubscriptionEventCallbacks();
        callbacks.MessageReceived += @event =>
        {
            var txId = @event.Message;
            Debug.Log("Transaction ID: " + txId);
            
            // @event.messageType contains the iap product id that is being purchased
            var currentItem = GetShopItemById(@event.MessageType);

            //TODO at some point --> Now we are assuming all consumable products are related to transferring tokens, and all non-consumable to minting nfts.
            switch (currentItem.productType)
            {
                case ProductType.Consumable:
                    
                    statusText.Set("Tokens purchased.");
                    currentItem.ActivateAnimation(false);
                    break;
                
                case ProductType.NonConsumable:
                    
                    statusText.Set("NFT purchased.");
                    currentItem.MarkAsPurchased(true);
                    break;
                
                case ProductType.Subscription:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };
        callbacks.ConnectionStateChanged += @event =>
        {
            Debug.Log($"Got player subscription ConnectionStateChanged: {JsonConvert.SerializeObject(@event, Formatting.Indented)}");
        };
        callbacks.Kicked += () =>
        {
            Debug.Log($"Got player subscription Kicked");
        };
        callbacks.Error += @event =>
        {
            Debug.Log($"Got player subscription Error: {JsonConvert.SerializeObject(@event, Formatting.Indented)}");
        };
        return CloudCodeService.Instance.SubscribeToPlayerMessagesAsync(callbacks);
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
    #endregion
}