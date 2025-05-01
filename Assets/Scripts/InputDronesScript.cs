using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.InputSystem;

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

    [Header("Score Calculation")]
    public ScoreCalc scoreCalculator;

    private string submitUrl = "https://24b4-213-109-232-105.ngrok-free.app/submit_move.php";
    private int gameId;
    private int playerId;

    void Start()
    {
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(ValidateDistribution);
        }

        if (errorText != null)
        {
            errorText.text = "Enter the number of drones for each planet.\nThe sum of drones must be 1000\nand must follow the rule:\nKronus ≥ Lyrion ≥ Mystara ≥ Eclipsia ≥ Fiora";
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

        gameId = SessionData.gameId;
        playerId = SessionData.playerId;

        StartCoroutine(SubmitMove(playerId, gameId, kronus, lyrion, mystara, eclipsia, fiora));
        submitButton.interactable = false;

        if (scoreCalculator != null && gameId != -1)
        {
            scoreCalculator.BeginCheckingMoves();
        }
        else
        {
            Debug.LogError("ScoreCalc script not assigned in Inspector!");
        }
    }

    private bool TryParseInputs(out int eclipsia, out int mystara, out int lyrion, out int kronus, out int fiora)
    {
        eclipsia = mystara = lyrion = kronus = fiora = 0;
        return TryParseInputField(eclipsiaInput, out eclipsia)
            && TryParseInputField(mystaraInput, out mystara)
            && TryParseInputField(lyrionInput, out lyrion)
            && TryParseInputField(kronusInput, out kronus)
            && TryParseInputField(fioraInput, out fiora);
    }

    private bool TryParseInputField(TMP_InputField inputField, out int value)
    {
        value = 0;
        return inputField != null && int.TryParse(inputField.text, out value) && value >= 0 && value <= 1000;
    }

    private void DisplayError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.color = Color.red;
        }
        Debug.LogError(message);
    }

    private void DisplaySuccess(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.color = Color.green;
        }
        Debug.Log(message);
    }

    IEnumerator SubmitMove(int playerId, int gameId, int kronus, int lyrion, int mystara, int eclipsia, int fiora)
    {
        WWWForm form = new WWWForm();
        form.AddField("game_id", gameId);
        form.AddField("player_id", playerId);
        form.AddField("kronus", kronus);
        form.AddField("lyrion", lyrion);
        form.AddField("mystara", mystara);
        form.AddField("eclipsia", eclipsia);
        form.AddField("fiora", fiora);

        using (UnityWebRequest www = UnityWebRequest.Post(submitUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Server response: " + www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Failed to submit move: " + www.error);
            }
        }
    }
}