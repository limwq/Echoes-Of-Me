using UnityEngine;

public class DialogueTrigger : MonoBehaviour {
    [Header("Dialogue Content")]
    public string npcName = "Guide";
    [TextArea(3, 10)] // Makes a big box in Inspector
    public string[] sentences;

    [Header("Interaction Settings")]
    public GameObject interactPrompt; // The "Press F" UI object (World or Screen space)

    private bool isPlayerInRange;

    void Start() {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    void Update() {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F)) {
            // Check if dialogue isn't already open
            if (DialogueManager.Instance.dialoguePanel.activeSelf == false) {
                TriggerDialogue();
            }
        }
    }

    public void TriggerDialogue() {
        DialogueManager.Instance.StartDialogue(npcName, sentences);
        if (interactPrompt != null) interactPrompt.SetActive(false); // Hide prompt while talking
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