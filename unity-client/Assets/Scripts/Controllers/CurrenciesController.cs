using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.Subscriptions;
using Unity.Services.Economy;
using UnityEngine;
using UnityEngine.Events;

public class CurrenciesController : Singleton<CurrenciesController>
{
    public event UnityAction OnCryptoCurrencyPurchased;

    public StatusTextBehaviour statusText;
    
    public async void AuthController_OnAuthSuccess_Handler(string ofPlayerId)
    {
        await SubscribeToCloudCodeMessages();
    }

    private Task SubscribeToCloudCodeMessages()
    {
        // Register callbacks, which are triggered when a player message is received
        var callbacks = new SubscriptionEventCallbacks();
        callbacks.MessageReceived += @event =>
        {
            var txId = @event.Message;
            Debug.Log("Transaction ID: " + txId);
            
            //TODO
            if (@event.MessageType == GameConstants.BuyCryptoCurrencyMessageType)
            {
                OnCryptoCurrencyPurchased?.Invoke();
                statusText.Set("Crypto currency tokens purchased.");
                
                // TODO get crypto currency balance somewhere.
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
    
    public async UniTask<string> GetCryptoCurrencyBalance()
    {
        try
        {
            var balance = await CloudCodeService.Instance.CallModuleEndpointAsync<string>(GameConstants.CurrentCloudModule, "GetErc20Balance");
            return string.IsNullOrEmpty(balance) ? null : balance;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async void BuyCryptoCurrency(int amount)
    {
        //TODO statusText.Set("Transferring tokens...");

        var functionParams = new Dictionary<string, object> { {"purchasedProductId", GameConstants.BuyCryptoCurrencyMessageType}, {"amount", amount} };
        await CloudCodeService.Instance.CallModuleEndpointAsync(GameConstants.CurrentCloudModule, "TransferTokens", functionParams);
        // Let's wait for the message from backend --> Inside SubscribeToCloudCodeMessages()
    }
    
    public async UniTask<string> GetCurrencyBalance()
    {
        try
        {
            // Call GetBalancesAsync with the options
            var result = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();
            
            foreach (var balance in result.Balances)
            {
                if (balance.CurrencyId == "GOLD")
                {
                    Debug.Log($"The balance for GOLD is: {balance.Balance}");
                    return balance.Balance.ToString();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return null;
    }
}
