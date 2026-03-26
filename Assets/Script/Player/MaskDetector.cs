using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteMask))]
public class MaskDetector : MonoBehaviour {
    [Header("Settings")]
    public KeyCode activationKey = KeyCode.E;
    public float activeDuration = 5f;
    public float cooldownTime = 3f;

    [Header("Visuals")]
    public SpriteRenderer areaVisual; // The Ring Sprite
    public float rotationSpeed = 30f; // NEW: How fast the ring spins
    public ParticleSystem maskOnEffect;
    public ParticleSystem maskOffEffect;

    [Header("Animation Settings")]
    public float animationSpeed = 5f;
    public float maxSize = 3f;
    public float minSize = 0f;
    public AnimationCurve openingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    public string maskOnSound = "MaskOn";
    public string maskOffSound = "MaskOff";

    [Header("Debug Info")]
    public bool isAbilityActive = false;
    public float cooldownTimer = 0f;
    [HideInInspector] public float deactivationTime;

    // References
    private PlayerController2D playerController;
    private PlayerIdentityManager identityManager;
    private SpriteMask spriteMask;
    private List<Collider2D> hiddenObjectsInRange = new List<Collider2D>();
    private Coroutine animationCoroutine;

    void Awake() {
        spriteMask = GetComponent<SpriteMask>();
        playerController = GetComponentInParent<PlayerController2D>();
        identityManager = GetComponentInParent<PlayerIdentityManager>();

        // Initialization
        transform.localScale = Vector3.one * minSize;
        if (spriteMask != null) spriteMask.enabled = false;
        if (areaVisual != null) areaVisual.enabled = false;
    }

    void Update() {
        // 1. Handle Cooldown
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        // 2. Handle Auto-Deactivation
        if (isAbilityActive && Time.time >= deactivationTime) {
            DeactivateMask();
        }

        // 3. Handle Input
        if (Input.GetKeyDown(activationKey) && !isAbilityActive && cooldownTimer <= 0) {
            ActivateMask();
        }

        // 4. NEW: Handle Rotation (Only if active and we have a visual)
        if (isAbilityActive && areaVisual != null) {
            areaVisual.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }

    void ActivateMask() {
        isAbilityActive = true;
        deactivationTime = Time.time + activeDuration;

        // Turn Visuals ON
        if (spriteMask != null) spriteMask.enabled = true;
        if (areaVisual != null) areaVisual.enabled = true;

        // Play On Effect
        if (maskOnEffect != null) maskOnEffect.Play();

        // Animation
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateMask(minSize, maxSize));

        // Logic
        if (playerController != null) playerController.ToggleHiddenLayer(true);
        if (identityManager != null) identityManager.SetMaskedIdentity(true);
        foreach (Collider2D obj in hiddenObjectsInRange.ToArray()) SetObjectVisibility(obj, true);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(maskOnSound);
    }

    void DeactivateMask() {
        isAbilityActive = false;
        cooldownTimer = cooldownTime;

        // Animation (Pass 'true' to disable visuals at the end)
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateMask(transform.localScale.x, minSize, true));

        // Logic
        if (playerController != null) playerController.ToggleHiddenLayer(false);
        if (identityManager != null) identityManager.SetMaskedIdentity(false);
        foreach (Collider2D obj in hiddenObjectsInRange.ToArray()) SetObjectVisibility(obj, false);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(maskOffSound);
    }

    IEnumerator AnimateMask(float startSize, float endSize, bool disableOnFinish = false) {
        float timer = 0f;
        while (timer < 1f) {
            timer += Time.deltaTime * animationSpeed;
            float scale = Mathf.Lerp(startSize, endSize, openingCurve.Evaluate(timer));
            transform.localScale = Vector2.one * scale;
            yield return null;
        }
        transform.localScale = Vector2.one * endSize;

        // Disable visuals ONLY when fully closed to prevent popping
        if (disableOnFinish) {
            // Play Off Effect here so it aligns with the close
            if (maskOffEffect != null) maskOffEffect.Play();

            if (spriteMask != null) spriteMask.enabled = false;
            if (areaVisual != null) areaVisual.enabled = false;
        }
    }

    // Trigger Logic
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Hidden")) {
            if (!hiddenObjectsInRange.Contains(other)) hiddenObjectsInRange.Add(other);
            if (isAbilityActive) SetObjectVisibility(other, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Hidden")) {
            hiddenObjectsInRange.Remove(other);
            SetObjectVisibility(other, false);
        }
    }

    void SetObjectVisibility(Collider2D col, bool isVisible) {
        if (col == null) return;
        HiddenObject hiddenObj = col.GetComponent<HiddenObject>();
        if (hiddenObj != null) hiddenObj.SetPhysicsActive(isVisible);

        HiddenPushable hiddenPush = col.GetComponent<HiddenPushable>();
        if (hiddenPush != null) hiddenPush.SetPhysicsActive(isVisible);
    }
}