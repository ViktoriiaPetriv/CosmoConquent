using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    private void OnExitClicked()
    {
        PlayClickSound();
        Application.Quit();
        Debug.Log("Game exited");
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
