using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    private List<NftPrefab> _nfts = new List<NftPrefab>();
    public Transform content;

    private void OnDisable()
    {
        ClearAllItems();
    }

    public void GetCurrentNfts()
    {
        // Get NftPrefab components from child transforms
        _nfts.Clear();
        foreach (Transform child in content)
        {
            NftPrefab nft = child.GetComponent<NftPrefab>();
            if (nft != null)
            {
                _nfts.Add(nft);
            }
        }
    }

    public void ClearItem(string tokenId)
    {
        NftPrefab nftToRemove = _nfts.Find(nft => nft.tokenIdText.text.Contains(tokenId));
        if (nftToRemove != null)
        {
            _nfts.Remove(nftToRemove);
            Destroy(nftToRemove.gameObject);
        }
}

    private void ClearAllItems()
    {
        foreach (Transform child in content) {
            Destroy(child.gameObject);
        }
    }
}
