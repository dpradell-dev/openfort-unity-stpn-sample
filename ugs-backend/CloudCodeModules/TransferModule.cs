using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json;
using Openfort.SDK;
using Openfort.SDK.Model;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Account = Nethereum.Web3.Accounts.Account;

namespace CloudCodeModules;

public class TransferModule: BaseModule
{
    private readonly SingletonModule _singleton;
    
    private readonly OpenfortClient _ofClient;
    private readonly int _chainId;
    private readonly PushClient _pushClient;

    public TransferModule(PushClient pushClient) 
    {
        _singleton = SingletonModule.Instance();
        _ofClient = _singleton.OfClient;
        _chainId = _singleton.ChainId;
        _pushClient = pushClient;
    }
    
    [CloudCodeFunction("TransferTokens")]
    public async Task TransferTokens(IExecutionContext context, string purchasedProductId, int amount)
    {
        var currentOfPlayer = _singleton.CurrentOfPlayer;
        var currentOfAccount = _singleton.CurrentOfAccount;

        if (currentOfPlayer == null || currentOfAccount == null)
        {
            throw new Exception("No Openfort account found for the player.");
        }
        
        //var weiAmount = UnitConversion.Convert.ToWei(amount);
        
        Interaction interaction =
            new Interaction(null,null, SingletonModule.OfGoldContract, "transfer", new List<object>{currentOfAccount.Address, amount});
        
        CreateTransactionIntentRequest request = new CreateTransactionIntentRequest(_chainId, null, SingletonModule.OfDevAccount,
            SingletonModule.OfSponsorPolicy, null, false, 0, new List<Interaction>{interaction});

        var txResponse = await _ofClient.TransactionIntents.Create(request);
        
        await SendPlayerMessage(context, txResponse.Id, purchasedProductId);
    }
    
    private async Task<string> SendPlayerMessage(IExecutionContext context, string message, string messageType)
    {
        var response = await _pushClient.SendPlayerMessageAsync(context, message, messageType);
        return "Player message sent";
    }
}
