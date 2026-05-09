using UnityEngine;
using TMPro;

public class CoinDisplay : MonoBehaviour
{
    [Header("Drag your CoinText here")]
    public TextMeshProUGUI coinText;

    void Start()
    {
        // Fetch the total coins from memory (default to 0 if none exist)
        int coins = PlayerPrefs.GetInt("total_coins", 0);

        // Update the UI
        if (coinText != null)
        {
            coinText.text = coins.ToString();
        }
    }
}