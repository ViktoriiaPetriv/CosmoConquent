using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class RegisterUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject registerPanel;
    public TMP_InputField firstNameInput;
    public TMP_InputField lastNameInput;
    public TMP_Text statusText;

    [Header("Buttons")]
    public Button registerButton;
    public Button openButton;
    public Button closeButton;

    [Header("Server URL")]
    public string registerUrl = ""; 

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
        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
        {
            statusText.text = "Please enter both first and last name.";
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("first_name", firstName);
        form.AddField("last_name", lastName);

        using (UnityWebRequest www = UnityWebRequest.Post(registerUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                statusText.text = "Successfully registered!";
            }
            else
            {
                statusText.text = "Error: " + www.error;
            }
        }
    }

    void ClearForm()
    {
        firstNameInput.text = "";
        lastNameInput.text = "";
    }
}
