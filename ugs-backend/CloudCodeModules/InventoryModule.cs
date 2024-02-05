using Microsoft.Extensions.Logging;
using Openfort.SDK;
using Openfort.SDK.Model;
using Unity.Services.CloudCode.Core;

namespace CloudCodeModules;

public class InventoryModule: BaseModule
{
    private readonly SingletonModule _singleton;
    
    private readonly OpenfortClient _ofClient;
    private readonly int _chainId;
    
    private readonly ILogger<InventoryModule> _logger;

    public InventoryModule(ILogger<InventoryModule> logger) 
    {
        _singleton = SingletonModule.Instance();
        _ofClient = _singleton.OfClient;
        _chainId = _singleton.ChainId;
        _logger = logger;
    }
    
    [CloudCodeFunction("GetPlayerNftInventory")]
    public async Task<InventoryListResponse?> GetPlayerNftInventory()
    {
        // Important to remember that this account persists in the Singleton until a new Openfort player is created.
        var currentOfAccount = _singleton.CurrentOfAccount;

        if (currentOfAccount == null)
        {
            throw new Exception("No CurrentOfAccount found.");
        }
        
        var request = new AccountInventoryListRequest(currentOfAccount.Id, null, null, null, new List<string>{SingletonModule.OfNftContract});
        var inventoryList = await _ofClient.Inventories.GetAccountNftInventory(request);
        
        return inventoryList;
    }
    
    [CloudCodeFunction("GetErc20Balance")]
    public async Task<string> GetErc20Balance()
    {
        var currentOfAccount = _singleton.CurrentOfAccount;

        if (currentOfAccount == null)
        {
            throw new Exception("No CurrentOfAccount found.");
        }
        
        // Passing OfGoldContract ID as a request parameter
        var request = new AccountInventoryListRequest(currentOfAccount.Id, null, null, null, new List<string>{SingletonModule.OfGoldContract});
        var inventoryList = await _ofClient.Inventories.GetAccountCryptoCurrencyInventory(request);

        if (inventoryList.Data.Count == 0)
        {
            _logger.LogInformation("{inventory_count}", inventoryList.Data.Count);
            return string.Empty;
        }
        
        // We assume there's only 1 result
        var balance = inventoryList.Data[0].Amount;
        return balance;
    }
}