using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;
using DeleteOptions = Unity.Services.CloudSave.Models.Data.Player.DeleteOptions;

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
    
    public static async UniTask DeleteFromCloud(string key)
    {
        try
        {
            await CloudSaveService.Instance.Data.Player.DeleteAsync(key, new DeleteOptions());
            Debug.Log($"Data deleted with key: {key}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine("An error occurred while trying to delete data from the cloud. But the program will continue running.");
        }
    }
}