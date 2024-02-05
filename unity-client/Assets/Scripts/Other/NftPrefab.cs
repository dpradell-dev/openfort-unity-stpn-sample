using TMPro;
using UnityEngine;

public class NftPrefab : MonoBehaviour
{
    public TextMeshProUGUI assetTypeText;
    public TextMeshProUGUI tokenIdText;
    
    public void Setup(string assetType, string tokenId)
    {
        assetTypeText.text = assetType;
        tokenIdText.text = "ID: " + tokenId;
    }
}
