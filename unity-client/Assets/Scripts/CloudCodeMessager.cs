using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.Subscriptions;
using UnityEngine;
using UnityEngine.Events;

public class CloudCodeMessager : Singleton<CloudCodeMessager>
{
    public event UnityAction OnMintNftSuccessful;
    public event UnityAction<string> OnSellNftSuccessful;
    public event UnityAction<int> OnCryptoCurrencyPurchased;
    public event UnityAction<int> OnCryptoCurrencyReceived;
    public event UnityAction<int> OnCryptoCurrencySpent;

    public async void AuthController_OnAuthSuccess_Handler(string ofPlayerId)
    {
        await SubscribeToCloudCodeMessages();
    }

    private bool _isCooldownActive = false;
    private const float CooldownDuration = 0.25f; // Cooldown duration in seconds

    private async Task SubscribeToCloudCodeMessages()
    {
        var callbacks = new SubscriptionEventCallbacks();
        callbacks.MessageReceived += async @event =>
        {
            if (!_isCooldownActive)
            {
                var message = @event.Message;
                Debug.Log("CloudCode player message received: " + message);

                switch (@event.MessageType)
                {
                    case GameConstants.BuyCryptoCloudFunctionName:
                        Debug.Log("OnCryptoCurrencyPurchased");
                        var amountPurchased = int.Parse(@event.Message);
                        OnCryptoCurrencyPurchased?.Invoke(amountPurchased);
                        break;
                    case GameConstants.SpendCryptoCloudFunctionName:
                        Debug.Log("SpendCryptoCloudFunctionName");
                        var amountSpent = int.Parse(@event.Message);
                        OnCryptoCurrencySpent?.Invoke(amountSpent);
                        break;
                    case GameConstants.MintNftCloudFunctionName:
                        Debug.Log("MintNftCloudFunctionName");
                        OnMintNftSuccessful?.Invoke();
                        break;
                    case GameConstants.SellNftCloudFunctionName:
                        Debug.Log("SellNftCloudFunctionName");
                        OnSellNftSuccessful?.Invoke(@event.Message);
                        break;
                    case GameConstants.ReceiveCryptoCloudFunctionName:
                        Debug.Log("ReceiveCryptoCloudFunctionName");
                        var amountReceived = int.Parse(@event.Message);
                        OnCryptoCurrencyReceived?.Invoke(amountReceived);
                        break;
                    case null:
                        Debug.LogError("Check this error");
                        break;
                    case "":
                        Debug.LogError("Check this error");
                        break;
                }

                // Activate cooldown
                _isCooldownActive = true;
                await Task.Delay(TimeSpan.FromSeconds(CooldownDuration));
                _isCooldownActive = false;
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

        await CloudCodeService.Instance.SubscribeToPlayerMessagesAsync(callbacks);
    }
}