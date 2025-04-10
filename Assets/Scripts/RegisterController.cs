using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class RegisterUI : MonoBehaviour
{
    public GameObject registerPanel;
    public TMP_InputField firstNameInput;
    public TMP_InputField lastNameInput;
    public TMP_Text statusText;
    public Button registerButton;

    public string registerUrl = "";

    public void ClosePanel()
    {
        registerPanel.SetActive(false);
    }
    public void OpenPanel()
    {
        registerPanel.SetActive(true);
    }

    public void OnRegisterClick()
    {
        StartCoroutine(SendRegisterRequest());
    }

    IEnumerator SendRegisterRequest()
    {
        WWWForm form = new WWWForm();
        form.AddField("first_name", firstNameInput.text);
        form.AddField("last_name", lastNameInput.text);

        UnityWebRequest www = UnityWebRequest.Post(registerUrl, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            statusText.text = "Error: " + www.error;
        }
        else
        {
            statusText.text = "Successfully registered!";
        }
    }
}
