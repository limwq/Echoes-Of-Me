using UnityEngine;

public class NotePickup : MonoBehaviour {
    [Header("Content")]
    public Sprite noteSprite;
    public KeyCode interactKey = KeyCode.F;

    [Header("Optional Visuals")]
    [Tooltip("Assign a 'Press E' popup text object here.")]
    public GameObject interactPrompt;

    [Header("Audio")]
    public string pickupSound = "PaperRustle";

    private bool isPlayerInRange;

    void Start() {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    void Update() {
        // Only check input if player is standing on the note
        if (isPlayerInRange) {
            if (Input.GetKeyDown(interactKey)) {
                OpenNote();
            }
        }
    }

    void OpenNote() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(pickupSound);
        Debug.Log("PickUp Sound Played");
        // --- INSTANCE TEST ---
        if (NoteSystem.Instance != null) {
            NoteSystem.Instance.ShowNote(noteSprite);
        } else {
            // If this logs, you forgot to put the NoteSystem in the scene!
            Debug.LogError("CRITICAL ERROR: NoteSystem instance not found! Did you add the NoteUI prefab to the scene?");
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            isPlayerInRange = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            isPlayerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }
}