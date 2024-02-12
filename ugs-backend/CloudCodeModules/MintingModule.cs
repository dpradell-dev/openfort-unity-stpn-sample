﻿using Openfort.SDK;
using Openfort.SDK.Model;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CloudCodeModules;

public class MintingModule: BaseModule
{
    private readonly SingletonModule _singleton;
    
    private readonly OpenfortClient _ofClient;
    private readonly int _chainId;
    private readonly PushClient _pushClient;

    public MintingModule(PushClient pushClient) 
    {
        _singleton = SingletonModule.Instance();
        _ofClient = _singleton.OfClient;
        _chainId = _singleton.ChainId;
        _pushClient = pushClient;
    }
    
    [CloudCodeFunction("MintNFT")]
    public async Task MintNFT(IExecutionContext context)
    {
        // Code to mint NFT
        var currentOfPlayer = _singleton.CurrentOfPlayer;
        var currentOfAccount = _singleton.CurrentOfAccount;

        if (currentOfPlayer == null || currentOfAccount == null)
        {
            throw new Exception("No Openfort account found for the player.");
        }

        Interaction interaction =
            new Interaction(null,null, SingletonModule.OfNftContract, "mint", new List<object>{currentOfAccount.Address});

        CreateTransactionIntentRequest request = new CreateTransactionIntentRequest(_chainId, currentOfPlayer.Id, null, 
            SingletonModule.OfSponsorPolicy, null, false, 0, new List<Interaction> { interaction });

        var txResponse = await _ofClient.TransactionIntents.Create(request);

        await SendPlayerMessage(context, txResponse.Id, "MintNFT");
    }
    
    [CloudCodeFunction("NftToMintingAccount")]
    public async Task NftToMintingAccount(IExecutionContext context, int tokenId)
    {
        var currentOfPlayer = _singleton.CurrentOfPlayer;
        var currentOfAccount = _singleton.CurrentOfAccount;

        if (currentOfPlayer == null || currentOfAccount == null)
        {
            throw new Exception("No Openfort account found for the player.");
        }

        List<object> functionArgs = new List<object>
        {
            currentOfAccount.Address, //from
            SingletonModule.OfDevMintingAccount, //to
            tokenId //nftTokenId
        };
        
        Interaction interaction =
            new Interaction(null,null, SingletonModule.OfNftContract, "safeTransferFrom", functionArgs);
        
        CreateTransactionIntentRequest request = new CreateTransactionIntentRequest(_chainId, currentOfPlayer.Id, null,
            SingletonModule.OfSponsorPolicy, null, false, 0, new List<Interaction>{interaction});
        
        TransactionIntentResponse txResponse;
        try
        {
            txResponse = await _ofClient.TransactionIntents.Create(request);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        await SendPlayerMessage(context, txResponse.Id, "NftToMintingAccount");
    }
    
    private async Task<string> SendPlayerMessage(IExecutionContext context, string message, string messageType)
    {
        var response = await _pushClient.SendPlayerMessageAsync(context, message, messageType);
        return "Player message sent";
    }
}
