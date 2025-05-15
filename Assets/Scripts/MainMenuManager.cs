using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenuPanel;           // панель головного меню
    public Button playButton;                  // кнопка Play
    public Button exitButton;                  // кнопка Exit
    public TMP_Text titleText;                 // заголовок гри

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    [Header("Scene Names")]
    public string gameSceneName = "SpaceScene";

    void Start()
    {
        // переконайся, що меню активне при старті
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        // призначення кнопок
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnPlayClicked()
    {
        PlayClickSound();
        // завантаження сцени гри
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnExitClicked()
    {
        PlayClickSound();
        // вихід з гри (працює тільки у білді)
        Application.Quit();
        Debug.Log("Game exited");
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
