using UnityEngine;
using UnityEngine.UI;

public class NoteSystem : MonoBehaviour {
    public static NoteSystem Instance;

    [Header("UI References")]
    public GameObject notePanel;
    public Image noteImageDisplay;
    public GameObject blurBackground;

    [Header("Settings")]
    public bool pauseGameOnInspect = true;

    private bool isNoteOpen = false;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep between scenes
        } else {
            Destroy(gameObject);
        }

        CloseNote(); // Hide UI on start
    }

    void Update() {
        if (isNoteOpen) {
            // Allow closing with F, Escape, or Space
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space)) {
                CloseNote();
            }
        }
    }

    public void ShowNote(Sprite noteContent) {
        isNoteOpen = true;

        if (noteImageDisplay != null) {
            noteImageDisplay.sprite = noteContent;
            noteImageDisplay.preserveAspect = true;
        }

        notePanel.SetActive(true);

        if (pauseGameOnInspect) Time.timeScale = 0f;
    }

    public void CloseNote() {
        isNoteOpen = false;
        notePanel.SetActive(false);
        if (pauseGameOnInspect) Time.timeScale = 1f;
    }
}