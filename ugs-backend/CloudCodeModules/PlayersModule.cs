using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;
using Openfort.SDK;
using Openfort.SDK.Client;
using Openfort.SDK.Model;
using Unity.Services.CloudSave.Model;

namespace CloudCodeModules;

public class PlayersModule: BaseModule
{
    private readonly SingletonModule _singleton;
    
    private readonly OpenfortClient _ofClient;
    private readonly int _chainId;

    public PlayersModule()
    {
        _singleton = SingletonModule.Instance();
        
        _ofClient = _singleton.OfClient;
        _chainId = _singleton.ChainId;
    }
    
    [CloudCodeFunction("SetOpenfortPlayerData")]
    public async Task SetOpenfortPlayerData(IExecutionContext context, string ofPlayerId)
    {
        try
        {
            // Get Openfort player
            var request = new PlayerGetRequest(ofPlayerId);
            var player = await _ofClient.Players.Get(request);
            
            //Get Openfort account
            var accRequest = new AccountListRequest(ofPlayerId, 1);
            var accountList = await _ofClient.Accounts.List(accRequest);
            
            // Check if accountList 
            if (accountList.Data.Count == 0)
            {
                throw new Exception("No Openfort account found for the player.");
            }
            
            // Save it to the Singleton class
            _singleton.CurrentOfPlayer = player;
            _singleton.CurrentOfAccount = accountList.Data[0];
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    [CloudCodeFunction("CreateOpenfortPlayer")]
    public async Task<PlayerResponse?> CreateOpenfortPlayer(IExecutionContext context, IGameApiClient gameApiClient, string playerName)
    {
        // Create player
        var request = new PlayerCreateRequest(playerName);
        var player = await _ofClient.Players.Create(request);
        
        // Save Openfort Player to Singleton
        _singleton.CurrentOfPlayer = player;
        
        // Create account
        var accRequest = new CreateAccountRequest(_chainId, null!, null, null!, 0L, player.Id);
        var account = await _ofClient.Accounts.Create(accRequest);
        
        // Save Openfort Account to Singleton
        _singleton.CurrentOfAccount = account;
        
        // Now you can call SaveData with the context
        await SavePlayerData(context, gameApiClient, "OpenfortPlayerId", player.Id);
        await SavePlayerData(context, gameApiClient, "OpenfortAccountId", account.Id);
        
        return player;
    }
    
    private async Task SavePlayerData(IExecutionContext context, IGameApiClient gameApiClient, string key, object value)
    {
        try
        {
            await gameApiClient.CloudSaveData.SetItemAsync(context, context.AccessToken, context.ProjectId,
                context.PlayerId, new SetItemBody(key, value));
        }
        catch (ApiException ex)
        {
            throw new Exception($"Failed to save data for playerId {context.PlayerId}. Error: {ex.Message}");
        }
    }
}