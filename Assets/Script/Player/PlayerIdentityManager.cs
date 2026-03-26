using UnityEngine;
using System.Collections;

public class PlayerIdentityManager : MonoBehaviour {
    [Header("Identities (Animation)")]
    public RuntimeAnimatorController normalController;
    public RuntimeAnimatorController maskedController;

    [Header("Visual Identity (Colors)")]
    public Color normalColor = Color.white;
    public Color maskedColor = Color.cyan; // The color while mask is ON

    [Header("Flash Effect")]
    public float flashDuration = 0.1f;

    private Animator anim;
    private SpriteRenderer sr;
    private Coroutine flashCoroutine;

    void Awake() {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Ensure we start with the normal color
        if (sr != null) sr.color = normalColor;
    }

    public void SetMaskedIdentity(bool isMasked) {
        // 1. Swap Animator
        if (isMasked && maskedController != null) {
            anim.runtimeAnimatorController = maskedController;
        } else if (!isMasked && normalController != null) {
            anim.runtimeAnimatorController = normalController;
        }

        // 2. Change Color & Flash
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(ChangeIdentityRoutine(isMasked));
    }

    IEnumerator ChangeIdentityRoutine(bool isMasked) {
        if (sr == null) yield break;

        FindObjectOfType<ScreenFlash>().TriggerFlash();

        sr.color = isMasked ? maskedColor : normalColor;

        // Use Realtime so it works even if game pauses
        yield return new WaitForSecondsRealtime(flashDuration);

        
    }
}