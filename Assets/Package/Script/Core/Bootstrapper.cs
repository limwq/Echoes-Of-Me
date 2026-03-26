using UnityEngine;

public class Bootstrapper : MonoBehaviour {
    [Header("System Prefabs")]
    public GameObject audioManagerPrefab;
    public GameObject sceneManagerPrefab;
    public GameObject noteSystemPrefab;
    public GameObject subtitleSystemPrefab;
    public GameObject dialogueSystemPrefab; // NEW: Add this line

    void Awake() {
        // 1. Audio Manager
        if (AudioManager.Instance == null && audioManagerPrefab != null)
            Instantiate(audioManagerPrefab);

        // 2. Scene Manager
        if (GameSceneManager.Instance == null && sceneManagerPrefab != null)
            Instantiate(sceneManagerPrefab);

        // 3. Note System
        if (NoteSystem.Instance == null && noteSystemPrefab != null) {
            Instantiate(noteSystemPrefab);
        }

        // 4. Subtitle System
        if (SubtitleSystem.Instance == null && subtitleSystemPrefab != null) {
            Instantiate(subtitleSystemPrefab);
        }

        // 5. Dialogue System (NEW)
        if (DialogueManager.Instance == null && dialogueSystemPrefab != null) {
            Instantiate(dialogueSystemPrefab);
        }

        Debug.Log("Systems Booted.");
    }

    void Start() {
        // After booting, automatically go to the Main Menu
        if (GameSceneManager.Instance != null) {
            GameSceneManager.Instance.FadeScene("MainMenu");
        }
    }
}