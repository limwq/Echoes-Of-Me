using UnityEngine;
using UnityEngine.Video; // Required for VideoPlayer
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VideoEndingController : MonoBehaviour {
    [Header("Video Settings")]
    [SerializeField] private VideoPlayer endingVideo; // Drag your Video Player here
    [SerializeField] private bool autoPlay = true;

    [Header("UI References")]
    [SerializeField] private Button skipButton; // Optional: A button to skip the video

    [Header("Configuration")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool hasTriggeredNextScene = false;

    private void Awake() {
        // Unlock cursor so player can click Skip if they want
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;
    }

    private void Start() {
        // 1. Setup Video
        if (endingVideo != null) {
            // Important: Video must NOT loop, otherwise the 'End' event never fires
            endingVideo.isLooping = false;

            // Subscribe to the event that fires when video finishes
            endingVideo.loopPointReached += OnVideoFinished;

            if (autoPlay) {
                endingVideo.Play();
            }
        } else {
            Debug.LogWarning("VideoEndingController: No Video Player assigned! Loading menu immediately.");
            GoToMenu();
        }

        // 2. Setup Skip Button
        if (skipButton != null) {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(GoToMenu);
        }
    }

    // Event called automatically by Unity when video ends
    private void OnVideoFinished(VideoPlayer vp) {
        GoToMenu();
    }

    public void GoToMenu() {
        // Prevent double-loading (e.g. if video ends exactly when player clicks skip)
        if (hasTriggeredNextScene) return;
        hasTriggeredNextScene = true;

        // Play Click Sound (Optional, only if clicking button)
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("UIClick");
        }

        Debug.Log("Video finished/Skipped. Loading Main Menu...");

        // Load Main Menu
        if (GameSceneManager.Instance != null) {
            GameSceneManager.Instance.FadeScene(mainMenuSceneName);
        } else {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    // Good practice: Unsubscribe from event if object is destroyed
    private void OnDestroy() {
        if (endingVideo != null) {
            endingVideo.loopPointReached -= OnVideoFinished;
        }
    }
}