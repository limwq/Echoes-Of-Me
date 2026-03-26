using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour {
    [Header("References")]
    public PlayerController2D playerMovement;
    public MaskDetector maskDetector;
    public PlayerCombat playerCombat;

    [Header("Rectangular Bar")]
    public Slider maskBar;
    public Slider combatBar;

    [Header("Circular Bars")]
    public Image dashBar;       // Inner Circle (Dash Cooldown)

    [Header("Wall Jump Visuals")]
    [Tooltip("Drag the 3 arc images here (Order: 1, 2, 3)")]
    public Image[] wallJumpSegments;
    public Color activeColor = Color.cyan;
    public Color emptyColor = new Color(1, 1, 1, 0.2f); // Faint transparent

    void Update() {
        UpdateDashBar();
        UpdateMaskBar();
        UpdateWallJump();
        UpdatePushBar();
    }

    void UpdateDashBar() {
        if (playerMovement == null || dashBar == null) return;

        // Logic: Empty when cooling down, Full when ready
        if (playerMovement.dashCooldownTimer > 0) {
            float progress = 1f - (playerMovement.dashCooldownTimer / playerMovement.dashCooldown);
            dashBar.fillAmount = progress;
        } else {
            dashBar.fillAmount = 1f;
        }
    }

    void UpdateMaskBar() {
        if (maskDetector == null || maskBar == null) return;

        if (maskDetector.isAbilityActive) {
            float timeRemaining = maskDetector.deactivationTime - Time.time;
            maskBar.value = Mathf.Clamp01(timeRemaining / maskDetector.activeDuration);
        } else if (maskDetector.cooldownTimer > 0) {
            float timeElapsed = maskDetector.cooldownTime - maskDetector.cooldownTimer;
            maskBar.value = Mathf.Clamp01(timeElapsed / maskDetector.cooldownTime);
        } else {
            maskBar.value = 1f;
        }
    }

    void UpdateWallJump() {
        if (playerMovement == null || wallJumpSegments.Length == 0) return;

        // Loop through the 3 segments
        for (int i = 0; i < wallJumpSegments.Length; i++) {
            // If the current index is less than remaining jumps, it is ACTIVE.
            // Example: wallJumpLeft = 2. Index 0 is ON, Index 1 is ON, Index 2 is OFF.
            if (i < playerMovement.wallJumpLeft) {
                wallJumpSegments[i].color = activeColor;
            } else {
                wallJumpSegments[i].color = emptyColor;
            }
        }
    }

    void UpdatePushBar() {
        if (playerCombat == null || combatBar == null) return;
        if (playerCombat.pushTimer > 0) {
            combatBar.value = 1f - (playerCombat.pushTimer / playerCombat.pushCooldown);
        } else {
            combatBar.value = 1f;
        }
    }
}