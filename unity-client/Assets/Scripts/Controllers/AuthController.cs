using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Openfort.Model;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.Events;

public class AuthController : BaseController
{
    [Header("Events")]
    public UnityEvent<string> authSuccess;

    [Header("Testing")]
    public bool createNewPlayer;
    public bool simulatePlayerCreation;

    private void Start()
    {
        viewPanel.SetActive(true);
    }

    public async void SignIn()
    {
        statusText.Set("Initializing Unity Services...");
        // Initialize the Unity Services Core SDK
        await UnityServices.InitializeAsync();

        if (createNewPlayer) // This will force AuthenticationService to sign in as a new anonymous player
        {
            AuthenticationService.Instance.ClearSessionToken();    
        }
        
        SetupAuthEvents();
        
        viewPanel.SetActive(false);
        statusText.Set("Signing in...");
        // Authenticate by logging into an anonymous account
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    
    private void SetupAuthEvents() {
      AuthenticationService.Instance.SignedIn += async () => {

          if (simulatePlayerCreation)
          {
              statusText.Set("Signed in successfully.");
              authSuccess?.Invoke("someOpenfortPlayerId");
              return;
          }
          
          // Shows how to get a playerID
          Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
          // Shows how to get an access token
          Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

          var ofPlayerId = await CloudSaveHelper.LoadFromCloud(GameConstants.OpenfortPlayerIdKey);
          
          if (string.IsNullOrEmpty(ofPlayerId))
          {
              try
              {
                  statusText.Set("Creating Openfort player...");
                  // Call the function within the module and provide the parameters we defined in there
                  var functionParams = new Dictionary<string, object> {{"playerName", AuthenticationService.Instance.PlayerId}};
                  var openfortPlayer = await CloudCodeService.Instance.CallModuleEndpointAsync<PlayerResponse>(GameConstants.CurrentCloudModule, "CreateOpenfortPlayer", functionParams);
                  Debug.Log(openfortPlayer.Id);
                  
                  statusText.Set("Signed in successfully.");
                  authSuccess.Invoke(openfortPlayer.Id);
              } 
              catch (CloudCodeException exception)
              {
                  Debug.LogException(exception);
              }
          }
          else
          {
              statusText.Set("Setting current Openfort player data...");
              var functionParams = new Dictionary<string, object> { { "ofPlayerId", ofPlayerId } };
              await CloudCodeService.Instance.CallModuleEndpointAsync(GameConstants.CurrentCloudModule, "SetOpenfortPlayerData",
                  functionParams);
              
              statusText.Set("Signed in successfully.");
              authSuccess?.Invoke(ofPlayerId);
          }
      };
    
      AuthenticationService.Instance.SignInFailed += (err) => { 
          Debug.LogError(err);
          viewPanel.SetActive(true);
      };
    
      AuthenticationService.Instance.SignedOut += () => {
          Debug.Log("Player signed out.");
          viewPanel.SetActive(true);
      };
    
      AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired.");
            viewPanel.SetActive(true);
        };
    }

    public async Task<string> LoadOpenfortPlayerId()
    {
        statusText.Set("Loading Openfort player...");
        var playerData =
            await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>{"OpenfortPlayerId"});

        if (playerData.TryGetValue("OpenfortPlayerId", out var item))
        {
            // Assuming the value is stored as a string within the JsonValue.
            var openfortPlayerIdValue = item.Value.GetAsString();
        
            // Use the value as needed in your application
                Debug.Log($"Retrieved OpenfortPlayerId: {openfortPlayerIdValue}");
        
            return openfortPlayerIdValue;
        }
        
        Debug.Log("OpenfortPlayerId key not found.");
        statusText.Set("Openfort player not found.");
        return null;
    }
}
