using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputDronesScript : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField eclipsiaInput;
    public TMP_InputField mystaraInput;
    public TMP_InputField lyrionInput;
    public TMP_InputField kronusInput;
    public TMP_InputField fioraInput;

    [Header("UI Elements")]
    public TextMeshProUGUI errorText;
    public Button submitButton;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    void Start()
    {
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(ValidateDistribution);
        }

        if (errorText != null)
        {
            errorText.text = "Enter the number of drones for each planet.\nThe sum of drones must be 1000\nfor the number of drones must follow the rule:\nKronus ≥ Lyrion ≥ Mystara ≥ Eclipsia ≥ Fiora";
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void ValidateDistribution()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        if (!TryParseInputs(out int eclipsia, out int mystara, out int lyrion, out int kronus, out int fiora))
        {
            DisplayError("Please enter valid numbers between 0 and 1000.");
            return;
        }

        int sum = eclipsia + mystara + lyrion + kronus + fiora;
        if (sum != 1000)
        {
            DisplayError($"The total must be 1000. Current total: {sum}");
            return;
        }

        if (kronus < lyrion || lyrion < mystara || mystara < eclipsia || eclipsia < fiora)
        {
            DisplayError("Distribution must follow: Kronus ≥ Lyrion ≥ Mystara ≥ Eclipsia ≥ Fiora");
            return;
        }

        DisplaySuccess("Distribution successful!");
    }

    private bool TryParseInputs(out int eclipsia, out int mystara, out int lyrion, out int kronus, out int fiora)
    {
        eclipsia = 0;
        mystara = 0;
        lyrion = 0;
        kronus = 0;
        fiora = 0;

        if (!TryParseInputField(eclipsiaInput, out eclipsia)) return false;
        if (!TryParseInputField(mystaraInput, out mystara)) return false;
        if (!TryParseInputField(lyrionInput, out lyrion)) return false;
        if (!TryParseInputField(kronusInput, out kronus)) return false;
        if (!TryParseInputField(fioraInput, out fiora)) return false;

        return true;
    }

    private bool TryParseInputField(TMP_InputField inputField, out int value)
    {
        value = 0;
        if (inputField == null || string.IsNullOrEmpty(inputField.text)) return false;

        if (int.TryParse(inputField.text, out value))
        {
            return value >= 0 && value <= 1000;
        }

        return false;
    }

    private void DisplayError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.color = Color.red;
        }
        else
        {
            Debug.LogError(message);
        }
    }

    private void DisplaySuccess(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.color = Color.green;
        }
        else
        {
            Debug.Log(message);
        }
    }

    void Update()
    {
    }
}