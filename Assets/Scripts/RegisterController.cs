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
    [Header("Buttons")]
    public Button registerButton;
    public Button openButton;
    public Button closeButton;
    [Header("Waiting Screen")]
    public WaitingScreenManager waitingScreenManager;
    [Header("Server URL")]
    private string registerUrl = "https://6c0a-213-109-232-105.ngrok-free.app/register.php";


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
        if (openButton != null)
            openButton.onClick.AddListener(OpenPanel);
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        registerPanel.SetActive(true);
        if (statusText != null)
            statusText.text = "Register please!";
        if (waitingScreenManager != null)
        {
            waitingScreenManager.HideWaitingScreen();
        }
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
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("username", username);

        UnityWebRequest www = UnityWebRequest.Post(registerUrl, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string responseText = www.downloadHandler.text;
            //Debug.Log("Register response: " + responseText);

            bool parseSuccess = false;
            RegisterResponse response = null;

            try
            {
                response = JsonUtility.FromJson<RegisterResponse>(responseText);
                parseSuccess = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("JSON parsing error: " + e.Message);
                parseSuccess = false;
            }

            if (parseSuccess)
            {
                if (response.error != null && !string.IsNullOrEmpty(response.error))
                {
                    statusText.text = "Error: " + response.error;
                }
                else if (response.player_id > 0)
                {
                    SessionData.playerId = response.player_id;

                    statusText.text = "Successfully registered!";
                    yield return new WaitForSeconds(1.5f);
                    ClosePanel();

                    if (waitingScreenManager != null)
                    {
                        waitingScreenManager.ShowWaitingScreen();
                    }
                    else
                    {
                        Debug.LogError("WaitingScreenManager reference is missing!");
                    }
                }
                else
                {
                    statusText.text = "Registration failed. Please try again.";
                }
            }
            else
            {
                // Handle cases where HTML error might be returned
                if (responseText.Contains("exists") || responseText.Contains("taken"))
                {
                    statusText.text = "Username already exists. Please choose another one.";
                }
                else
                {
                    statusText.text = "Server error. Please try again.";
                }
            }
        }
        else
        {
            statusText.text = "Network error: " + www.error;
        }
    }

    void ClearForm()
    {
        usernameInput.text = "";
    }
}