using System;
using System.Collections.Generic;
using Openfort.Model;
using Unity.Services.CloudCode;
using UnityEngine;

public class InventoryController : BaseController
{
    // Get inventory view component
    public InventoryView inventoryView;

    [Header("Other controllers")] public ShopController shopController;
    
    [Header("NFT related")]
    public Transform content;
    public NftPrefab nftPrefab;

    private string _nftCryptoPrice; // The price of the NFT

    private void OnEnable()
    {
        shopController.OnNftPurchaseStarted += ShopController_OnNftPurchaseStarted_Handler;
        NftPrefab.OnSellButtonPressed += NftPrefab_OnSellButtonPressed_Handler;
        CloudCodeMessager.Instance.OnSellNftSuccessful += CloudCodeMessager_OnSellNftSuccessful_Handler;
    }

    private void OnDisable()
    {
        shopController.OnNftPurchaseStarted -= ShopController_OnNftPurchaseStarted_Handler;
        NftPrefab.OnSellButtonPressed -= NftPrefab_OnSellButtonPressed_Handler;
        CloudCodeMessager.Instance.OnSellNftSuccessful -= CloudCodeMessager_OnSellNftSuccessful_Handler;
    }

    public void Activate()
    {
        viewPanel.SetActive(true);
        GetPlayerNftInventory();
    }

    private async void SellNft(int tokenId)
    {
        statusText.Set("Selling NFT...");

        try
        {
            var functionParams = new Dictionary<string, object> { {"tokenId", tokenId} };
            await CloudCodeService.Instance.CallModuleEndpointAsync(GameConstants.CurrentCloudModule, GameConstants.SellNftCloudFunctionName, functionParams);
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
                // TODO Send event so we can enable nft sell button again.
                statusText.Set("Transaction failed.");
                Console.WriteLine(e);
                throw;
            }
        }
    }
    
    private async void GetPlayerNftInventory()
    {
        statusText.Set("Fetching player inventory...");
        
        try
        {
            var inventoryList = await CloudCodeService.Instance.CallModuleEndpointAsync<InventoryListResponse>(GameConstants.CurrentCloudModule, "GetPlayerNftInventory");

            if (inventoryList.Data.Count == 0)
            {
                statusText.Set("Player inventory is empty.");
            }
            else
            {
                foreach (var nft in inventoryList.Data)
                {
                    var instantiatedNft = Instantiate(nftPrefab, content);
                    instantiatedNft.Setup(nft.AssetType.ToString(), nft.TokenId.ToString(), _nftCryptoPrice);
                    
                    Debug.Log(nft);
                }
                
                inventoryView.GetCurrentNfts();
                statusText.Set("Player inventory retrieved.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private void CloudCodeMessager_OnSellNftSuccessful_Handler(string soldTokenId)
    {
        statusText.Set("NFT sold successfully.", 5f);
        inventoryView.ClearItem(soldTokenId);
    }
    
    private void ShopController_OnNftPurchaseStarted_Handler(string nftCryptoPrice)
    {
        // We only spent crypto for buying NFT. We want to save the price so we can populate it later.
        _nftCryptoPrice = nftCryptoPrice;
    }
    
    private void NftPrefab_OnSellButtonPressed_Handler(string tokenId)
    {
        // We know we can parse a NFT token id
        var tokenIdInt = int.Parse(tokenId);
        
        SellNft(tokenIdInt);
    }
}
