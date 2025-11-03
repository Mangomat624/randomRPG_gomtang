// StartMenuManager.cs (¼öÁ¤µÈ ÀüÃ¼ ÄÚµå)

using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions; // Á¤±Ô½Ä(Regex)
using System.Collections; // ÄÚ·çÆ¾

public class StartMenuManager : MonoBehaviour
{
    [Header("UI ¿ä¼Ò ¿¬°á")]
    public Button startButton;
    public GameObject nameInputPanel;
    public TMP_InputField nameInputField;
    public Button confirmButton;
    public TextMeshProUGUI warningText; // °æ°í ÅØ½ºÆ®

    [Header("¼³Á¤")]
    public string nextSceneName = "ChapterSelect";

    private const string NameValidationPattern = @"^[a-zA-Z0-9°¡-ÆR]{2,10}$";
    private const string WarningMessage = "ÀÌ¸§Àº ÃÖ¼Ò 2ÀÚ¿¡¼­ ÃÖ´ë 10ÀÚ±îÁö °¡´ÉÇÏ¸ç ¿µ¹® ¶Ç´Â ÇÑ±Û°ú ¼ıÀÚ Á¶ÇÕ¸¸ °¡´ÉÇÕ´Ï´Ù.";

    private Coroutine warningCoroutine;
    private Color defaultWarningColor = Color.white; // ±âº» »ö»ó(Èò»ö) ÀúÀå

    void Start()
    {
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);

        // (º¯°æ) WarningText¸¦ ¼û±âÁö ¾Ê°í, ±âº» ¸Ş½ÃÁö¿Í »ö»óÀ¸·Î ¼³Á¤ÇÕ´Ï´Ù.
        if (warningText != null)
        {
            warningText.text = WarningMessage;
            warningText.color = defaultWarningColor;
            warningText.gameObject.SetActive(false); // ÀÌ¸§ ÀÔ·ÂÃ¢ÀÌ ¶ã ¶§±îÁö´Â ¼û±è
        }

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        if (nameInputPanel != null)
            nameInputPanel.SetActive(true);

        // (º¯°æ) ÀÌ¸§ ÀÔ·ÂÃ¢À» ÄÓ ¶§ WarningTextµµ ÇÔ²² ÄÕ´Ï´Ù.
        if (warningText != null)
        {
            // È¤½Ã ÀÌÀü¿¡ »¡°£»öÀ¸·Î ³²¾ÆÀÖ¾ú´Ù¸é Èò»öÀ¸·Î ¸®¼Â
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }
            warningText.text = WarningMessage;
            warningText.color = defaultWarningColor;
            warningText.gameObject.SetActive(true); // ÅØ½ºÆ®¸¦ ÄÔ
        }

        if (startButton != null)
            startButton.gameObject.SetActive(false);
    }

    private void OnConfirmButtonClicked()
    {
        string newName = nameInputField.text;

        if (Regex.IsMatch(newName, NameValidationPattern))
        {
            // --- À¯È¿¼º °Ë»ç ¼º°ø ---
            if (warningText != null)
            {
                if (warningCoroutine != null) // È¤½Ã ±ôºıÀÌ´Â ÁßÀÌ¾ú´Ù¸é ÁßÁö
                {
                    StopCoroutine(warningCoroutine);
                    warningCoroutine = null;
                }
                warningText.gameObject.SetActive(false); // ¼º°øÇßÀ¸´Ï ¼û±è
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerName = newName;
            }
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // --- À¯È¿¼º °Ë»ç ½ÇÆĞ ---
            if (warningText != null)
            {
                // (º¯°æ) ÀÌ¹Ì ½ÇÇà ÁßÀÎ ÄÚ·çÆ¾ÀÌ ÀÖ´Ù¸é ¸ØÃß°í »õ·Î ½ÃÀÛ
                if (warningCoroutine != null)
                {
                    StopCoroutine(warningCoroutine);
                }
                warningCoroutine = StartCoroutine(ShowWarningBlink());
            }
            Debug.LogWarning($"ÀÌ¸§ À¯È¿¼º °Ë»ç ½ÇÆĞ: {newName}");
        }
    }

    // (º¯°æ) °æ°í ÅØ½ºÆ®¸¦ "±ôºı"ÀÌ°Ô ÇÏ´Â ÄÚ·çÆ¾
    private IEnumerator ShowWarningBlink()
    {
        // 1. ÅØ½ºÆ®°¡ ÄÑÁ® ÀÖ´ÂÁö È®ÀÎ (ÀÌ¹Ì ÄÑÁ® ÀÖÀ½)
        warningText.text = WarningMessage;

        // 2. »¡°£»öÀ¸·Î º¯°æ
        warningText.color = Color.red;

        // 3. 1ÃÊ µ¿¾È ±â´Ù¸³´Ï´Ù.
        yield return new WaitForSeconds(1f);

        // 4. 1ÃÊ°¡ Áö³ª¸é ´Ù½Ã ±âº» »ö»ó(Èò»ö)À¸·Î º¯°æÇÕ´Ï´Ù.
        warningText.color = defaultWarningColor;

        // 5. ÄÚ·çÆ¾ÀÌ ³¡³µÀ½À» ¾Ë¸³´Ï´Ù.
        warningCoroutine = null;
    }
}