using System;
using Openfort.Model;
using TMPro;
using Unity.Services.CloudCode;
using UnityEngine;

public class InventoryController : BaseController
{
    [Header("UI")]
    public TextMeshProUGUI balanceValue;
    
    [Header("NFT related")]
    public Transform content;
    public NftPrefab nftPrefab;
    
    public void Activate()
    {
        viewPanel.SetActive(true);
        
        GetPlayerNftInventory();
        GetErc20Balance();
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
                    instantiatedNft.Setup(nft.AssetType.ToString(), nft.TokenId.ToString());
                    
                    Debug.Log(nft);
                }
                
                statusText.Set("Player inventory retrieved.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async void GetErc20Balance()
    {
        try
        {
            var balance = await CloudCodeService.Instance.CallModuleEndpointAsync<string>(GameConstants.CurrentCloudModule, "GetErc20Balance");

            if (string.IsNullOrEmpty(balance))
            {
            }
            else
            {
                balanceValue.text = balance;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
