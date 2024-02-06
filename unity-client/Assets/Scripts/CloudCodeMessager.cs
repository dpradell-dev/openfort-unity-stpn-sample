using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.Subscriptions;
using UnityEngine;
using UnityEngine.Events;

public class CloudCodeMessager : Singleton<CloudCodeMessager>
{
    public event UnityAction OnMintNftSuccessful;
    public event UnityAction OnCryptoCurrencyPurchased;

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
            var message = @event.Message;
            Debug.Log("CloudCode player message received: " + message);

            switch (@event.MessageType)
            {
                case GameConstants.BuyCryptoCurrencyCloudFunctionName:
                    OnCryptoCurrencyPurchased?.Invoke();
                    break;
                case GameConstants.MintNftCloudFunctionName:
                    OnMintNftSuccessful?.Invoke();
                    break;
                //TODO case null or empty
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
            
            //TODO! Throw a generic error
        };
        return CloudCodeService.Instance.SubscribeToPlayerMessagesAsync(callbacks);
    }
}