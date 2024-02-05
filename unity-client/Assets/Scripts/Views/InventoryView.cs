using System;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    public Transform content;

    private void OnDisable()
    {
        ClearAllItems();
    }

    private void ClearAllItems()
    {
        foreach (Transform child in content) {
            Destroy(child.gameObject);
        }
    }
}
