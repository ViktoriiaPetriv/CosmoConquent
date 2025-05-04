using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class RegisterUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject registerPanel;
    public TMP_InputField usernameInput;
    public TMP_Text statusText;

    private Outline statusOutline;

    [Header("Buttons")]
    public Button registerButton;

    [Header("Waiting Screen")]
    public WaitingScreenManager waitingScreenManager;

    [Header("Server URL")]
    private string registerUrl = "https://89a7-213-109-232-105.ngrok-free.app/register.php";

    [System.Serializable]
    public class RegisterResponse
    {
        public int player_id;
        public string error;
    }

    void Start()
    {
        if (registerButton != null)
            registerButton.onClick.AddListener(OnRegisterClick);

        registerPanel.SetActive(true);

        if (statusText != null)
        {
            statusText.text = "Register please!";
            statusOutline = statusText.GetComponent<Outline>();
            if (statusOutline == null)
                statusOutline = statusText.gameObject.AddComponent<Outline>();
        }

        if (waitingScreenManager != null)
            waitingScreenManager.HideWaitingScreen();
    }

    public void OpenPanel()
    {
        registerPanel.SetActive(true);
        ClearForm();
        statusText.text = "Register Please";
    }

    public void ClosePanel()
    {
        registerPanel.SetActive(false);
    }

    public void OnRegisterClick()
    {
        statusText.text = "Registering...";
        StartCoroutine(SendRegisterRequest());
    }

    IEnumerator SendRegisterRequest()
    {
        string username = usernameInput.text.Trim();

        if (string.IsNullOrEmpty(username))
        {
            statusText.text = "Please enter username.";
            SetStatusColor(Color.red, Color.red);
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("username", username);

        UnityWebRequest www = UnityWebRequest.Post(registerUrl, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string responseText = www.downloadHandler.text;

            RegisterResponse response = null;
            bool parseSuccess = false;

            try
            {
                response = JsonUtility.FromJson<RegisterResponse>(responseText);
                parseSuccess = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("JSON parsing error: " + e.Message);
            }

            if (parseSuccess && response != null)
            {
                if (!string.IsNullOrEmpty(response.error))
                {
                    statusText.text = "Error: " + response.error;
                    SetStatusColor(Color.red, Color.red);
                }
                else if (response.player_id > 0)
                {
                    SessionData.playerId = response.player_id;
                    statusText.text = "Successfully registered!";
                    SetStatusColor(Color.green, Color.green);
                    yield return new WaitForSeconds(1.5f);
                    ClosePanel();
                    if (waitingScreenManager != null)
                        waitingScreenManager.ShowWaitingScreen();
                    else
                        Debug.LogError("WaitingScreenManager reference is missing!");
                }
                else
                {
                    statusText.text = "Registration failed. Please try again.";
                    SetStatusColor(Color.red, Color.red);
                }
            }
            else
            {
                if (responseText.Contains("exists") || responseText.Contains("taken"))
                    statusText.text = "Username already exists. Please choose another one.";
                else
                    statusText.text = "Server error. Please try again.";

                SetStatusColor(Color.red, Color.red);
            }
        }
        else
        {
            statusText.text = "Network error: " + www.error;
            SetStatusColor(Color.red, Color.red);
        }
    }

    void ClearForm()
    {
        usernameInput.text = "";
    }

    void SetStatusColor(Color textColor, Color outlineColor)
    {
        if (statusText != null)
            statusText.color = textColor;

        if (statusOutline != null)
        {
            statusOutline.effectColor = outlineColor;
            statusOutline.effectDistance = new Vector2(0.1f, -0.1f);
            statusOutline.enabled = true;
        }
    }
}