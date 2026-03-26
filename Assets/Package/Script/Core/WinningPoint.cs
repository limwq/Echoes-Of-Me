using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class WinningPoint : MonoBehaviour {
    [Header("Level Settings")]
    [Tooltip("The exact name of the next scene to load.")]
    public string nextSceneName;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.F;
    [Tooltip("Drag your 'Press F' text object here.")]
    public GameObject interactPrompt;

    [Header("Audio Settings")]
    public string victorySound;
    [Tooltip("How long to wait before loading (usually length of sound).")]
    public float waitDuration = 3.0f;

    // Internal State
    private bool isPlayerInRange;
    private bool isSequenceStarted;
    private GameObject playerObj;

    void Start() {
        // Hide the prompt at start
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    void Update() {
        // Check for input only if player is near AND we haven't started winning yet
        if (isPlayerInRange && !isSequenceStarted) {
            if (Input.GetKeyDown(interactKey)) {
                StartCoroutine(WinSequence());
            }
        }
    }

    IEnumerator WinSequence() {
        isSequenceStarted = true;

        // 1. Hide Prompt
        if (interactPrompt != null) interactPrompt.SetActive(false);

        // 2. Disable Player (The "Ban" Logic)
        if (playerObj != null) {
            // Stop Physics
            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;

            // Stop Input Script
            PlayerController2D pc = playerObj.GetComponent<PlayerController2D>();
            if (pc != null) pc.enabled = false;

            // Stop Combat Script (Optional, prevents swinging sword during win)
            PlayerCombat combat = playerObj.GetComponent<PlayerCombat>();
            if (combat != null) combat.enabled = false;

            // Optional: Play a "Victory" animation trigger if you have one
            Animator anim = playerObj.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("victory"); // Make sure you have this parameter!
        }

        // 3. Play Sound
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(victorySound)) {
            AudioManager.Instance.PlaySFX(victorySound);
        }

        Debug.Log("Victory! Loading next level in " + waitDuration + " seconds...");

        // 4. Wait for audio to finish
        yield return new WaitForSeconds(waitDuration);

        // 5. Load Level
        LoadNextLevel();
    }

    void LoadNextLevel() {
        // Try Custom Manager first
        GameSceneManager manager = FindFirstObjectByType<GameSceneManager>();
        if (manager != null) {
            manager.LoadScene(nextSceneName);
        } else {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    // --- Trigger Logic ---

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            isPlayerInRange = true;
            playerObj = other.gameObject;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            isPlayerInRange = false;
            playerObj = null;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }
}