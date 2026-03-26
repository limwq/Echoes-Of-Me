using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PressurePlate : MonoBehaviour {
    [Header("Settings")]
    public LayerMask triggerLayers;

    [Header("Visuals")]
    public Transform plateSprite; // Drag the moving sprite here
    public float pressedOffset = -0.05f;
    public float pressSpeed = 10f;

    [Header("Events")]
    public UnityEvent OnPressed;
    public UnityEvent OnReleased;

    [Header("Audio")]
    public string pressSound = "PlateDown";
    public string releaseSound = "PlateUp";

    // Internal State
    private bool isPressed;
    private Vector3 initialPos;
    private Vector3 targetPos;
    private List<Collider2D> objectsOnPlate = new List<Collider2D>();

    void Start() {
        if (plateSprite != null) initialPos = plateSprite.localPosition;
        targetPos = initialPos;
    }

    void Update() {
        if (plateSprite != null) {
            plateSprite.localPosition = Vector3.Lerp(plateSprite.localPosition, targetPos, Time.deltaTime * pressSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.isTrigger) return; // Ignore other triggers (Safety)

        if (IsInLayerMask(other.gameObject, triggerLayers)) {
            if (!objectsOnPlate.Contains(other)) objectsOnPlate.Add(other);
            CheckPressedState();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.isTrigger) return;

        if (IsInLayerMask(other.gameObject, triggerLayers)) {
            if (objectsOnPlate.Contains(other)) objectsOnPlate.Remove(other);
            CheckPressedState();
        }
    }

    // Called by HiddenObject when the plate disappears
    public void ResetPlate() {
        objectsOnPlate.Clear();
        if (isPressed) {
            isPressed = false;
            targetPos = initialPos;
            OnReleased.Invoke();
            Debug.Log("Plate Hidden - Forcing Release");
        }
    }

    void CheckPressedState() {
        bool shouldBePressed = objectsOnPlate.Count > 0;

        if (shouldBePressed && !isPressed) {
            isPressed = true;
            targetPos = initialPos + new Vector3(0, pressedOffset, 0);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(pressSound);

            Debug.Log("Press Sound Played");

            OnPressed.Invoke();
        } else if (!shouldBePressed && isPressed) {
            isPressed = false;
            targetPos = initialPos;

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(releaseSound);

            Debug.Log("Release Sound Played");

            OnReleased.Invoke();
        }
    }

    private bool IsInLayerMask(GameObject obj, LayerMask mask) {
        return (mask.value & (1 << obj.layer)) > 0;
    }
}