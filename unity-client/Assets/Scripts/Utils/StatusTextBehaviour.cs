using TMPro;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class StatusTextBehaviour : MonoBehaviour
{
    public TextMeshProUGUI tmp;

    public void Set(string newText)
    {
        StopAllCoroutines();
        
        tmp.text = newText;
        StartCoroutine(ClearTextAfterDelay(10f)); // Start the coroutine
    }
    
    public void Set(string newText, float delay)
    {
        StopAllCoroutines();
        
        tmp.text = newText;
        StartCoroutine(ClearTextAfterDelay(delay)); // Start the coroutine
    }

    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        tmp.text = ""; // Clear the text
    }
}