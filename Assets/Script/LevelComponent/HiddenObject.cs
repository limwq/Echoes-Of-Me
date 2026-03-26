using UnityEngine;
using System.Collections.Generic;

public class HiddenObject : MonoBehaviour {
    [Header("Settings")]
    [Tooltip("If true, object vanishes on game start. If false, it stays visible.")]
    public bool startHidden = true; // <-- NEW SETTING

    [Header("Assign in Inspector")]
    public List<Collider2D> collidersToToggle;
    public PressurePlate pressurePlate;

    private SpriteRenderer[] allRenderers;

    void Awake() {
        allRenderers = GetComponentsInChildren<SpriteRenderer>();

        // Apply the starting state defined in the Inspector
        SetPhysicsActive(!startHidden);
    }

    public void SetPhysicsActive(bool isActive) {
        // Toggle Visuals
        if (allRenderers != null) {
            foreach (var sr in allRenderers) sr.enabled = isActive;
        }

        // Toggle Colliders
        if (collidersToToggle != null) {
            foreach (var col in collidersToToggle) {
                if (col != null) col.enabled = isActive;
            }
        }

        // Handle Pressure Plate Reset
        if (!isActive && pressurePlate != null) {
            pressurePlate.ResetPlate();
        }
    }
}