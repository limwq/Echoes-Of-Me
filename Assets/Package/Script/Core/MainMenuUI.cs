using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [Header("External Systems")]
    [SerializeField] private SettingsMenu settingsMenu;

    private void Start() {
        // 1. Setup Play Button
        if (playButton != null) {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayClicked);
        }

        // 2. Setup Options Button (The new part)
        if (optionsButton != null) {
            optionsButton.onClick.RemoveAllListeners();
            optionsButton.onClick.AddListener(OnOptionsClicked);
        }

        // 3. Setup Quit Button
        if (quitButton != null) {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void OnPlayClicked() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UIClick");
        if (GameSceneManager.Instance != null) GameSceneManager.Instance.LoadScene("Start");
    }

    private void OnOptionsClicked() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UIClick");

        // Tell the Settings script to swap the panels
        if (settingsMenu != null) {
            settingsMenu.OpenOptions();
        } else {
            Debug.LogWarning("SettingsMenu reference is missing in MainMenuUI!");
        }
    }

    private void OnQuitClicked() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UIClick");
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}