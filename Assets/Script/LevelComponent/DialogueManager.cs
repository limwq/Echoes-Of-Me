using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {
    public static DialogueManager Instance;

    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Settings")]
    public float typingSpeed = 0.02f;

    private Queue<string> sentences;
    private bool isDialogueActive = false;
    private PlayerController2D player;

    void Awake() {
        // --- SINGLETON PATTERN FIX ---
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // <--- This is the key line!
        } else {
            Destroy(gameObject); // If a duplicate spawns, destroy it
            return;
        }
        // -----------------------------

        sentences = new Queue<string>();
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void Update() {
        if (!isDialogueActive) return;

        // SKIP logic
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(string npcName, string[] dialogueLines) {
        // Find player dynamically since they might be different in each level
        player = FindFirstObjectByType<PlayerController2D>();

        // 1. Freeze Player
        if (player != null) {
            player.canMove = false;
            // Stop physics and animation immediately
            if (player.GetComponent<Rigidbody2D>()) player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            if (player.GetComponent<Animator>()) player.GetComponent<Animator>().SetBool("isRunning", false);
        }

        // 2. Setup UI
        isDialogueActive = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (nameText != null) nameText.text = npcName;

        sentences.Clear();

        foreach (string line in dialogueLines) {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence() {
        if (sentences.Count == 0) {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence) {
        if (dialogueText != null) dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray()) {
            if (dialogueText != null) dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void EndDialogue() {
        isDialogueActive = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Unfreeze Player
        if (player != null) player.canMove = true;
    }
}