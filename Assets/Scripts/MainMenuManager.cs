using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenuPanel;          
    public Button playButton;                 
    public Button exitButton;                  
    public TMP_Text titleText;                 

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    [Header("Scene Names")]
    public string gameSceneName = "SpaceScene";

    void Start()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnPlayClicked()
    {
        PlayClickSound();
        SceneManager.LoadScene(gameSceneName);
    }

    
    private IEnumerator DeletePlayer()
    {
        int playerId = SessionData.playerId;

        if (playerId <= 0)
        {
            Debug.LogWarning("Invalid player ID.");
            Application.Quit();
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("player_id", playerId);

        using (UnityWebRequest www = UnityWebRequest.Post("https://unity-server-sdfo.onrender.com/delete_player.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to delete player: " + www.error);
            }
            else
            {
                Debug.Log("Player deleted successfully: " + www.downloadHandler.text);
            }
        }
    }

    private void OnExitClicked()
    {
        PlayClickSound();
        StartCoroutine(DeletePlayer());
        Application.Quit();
        Debug.Log("Game exited");
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
