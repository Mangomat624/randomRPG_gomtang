// TokenTextUI.cs

using UnityEngine;
using TMPro;

public class TokenTextUI : MonoBehaviour
{
    public TextMeshProUGUI tokenText;

    void OnEnable()
    {
        GameManager.OnTokensChanged += UpdateText;
    }

    void OnDisable()
    {
        GameManager.OnTokensChanged -= UpdateText;
    }

    private void UpdateText(int currentTokens)
    {
        if (tokenText != null)
        {
            tokenText.text = "ÅäÅ«: " + currentTokens;
        }
    }
}