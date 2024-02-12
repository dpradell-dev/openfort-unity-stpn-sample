using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

public class CloudSaveHelper : MonoBehaviour
{
    public static async UniTask SaveToCloud(string key, string value)
    {
        // Creating a Dictionary with the data you want to save
        var dataToSave = new Dictionary<string, object>
        {
            [key] = value
        };

        // Save the data to the cloud.
        await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
        
        Debug.Log($"Data saved.");
    }
    
    public static async UniTask<string> LoadFromCloud(string key)
    {
        var cloudData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>{key});
    
        if (cloudData.TryGetValue(key, out var value))
        {
            Debug.Log($"Data loaded: {value.Value.GetAsString()}");
            return value.Value.GetAsString();
        }
        
        Debug.Log($"No data found for the key: {key}");
        return null;
    }
}