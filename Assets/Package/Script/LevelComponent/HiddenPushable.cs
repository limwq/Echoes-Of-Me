using UnityEngine;

// Add this to the PARENT object
public class HiddenPushable : MonoBehaviour {
    [Header("Assign Child Components")]
    [Tooltip("The collider on the child object that makes the box solid.")]
    public Collider2D solidCollider;
    [Tooltip("The Rigidbody on the child object.")]
    public Rigidbody2D rb;
    [Tooltip("The SpriteRenderer on the child object.")]
    public SpriteRenderer sr;

    void Awake() {
        // Auto-find components in children if you forgot to drag them in
        if (rb == null) rb = GetComponentInChildren<Rigidbody2D>();
        if (solidCollider == null && rb != null) solidCollider = rb.GetComponent<Collider2D>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        // Default State: Hidden and Frozen
        SetRevealed(false);
    }

    // Called by MaskAbility
    public void SetPhysicsActive(bool isActive) {
        SetRevealed(isActive);
    }

    private void SetRevealed(bool isVisible) {
        // 1. Visuals
        if (sr != null) sr.enabled = isVisible;

        // 2. Physics Logic
        if (isVisible) {
            // When revealed: Fall and be pushable
            if (rb != null) {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.velocity = Vector2.zero; // Reset to prevent built-up momentum
            }
            if (solidCollider != null) solidCollider.enabled = true;
        } else {
            // When hidden: Float in place and ghost through walls
            if (rb != null) {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            if (solidCollider != null) solidCollider.enabled = false;
        }
    }
}