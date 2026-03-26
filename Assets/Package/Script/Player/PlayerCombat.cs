using UnityEngine;

public class PlayerCombat : MonoBehaviour {
    [Header("Combat (Push)")]
    [Tooltip("Force applied to enemies/objects.")]
    public float pushForce = 1f;
    [Tooltip("Radius of the push effect.")]
    public float pushRadius = 1.5f;
    [Tooltip("Time between pushes.")]
    public float pushCooldown = 1.0f;

    // Public so HUD can see it
    [HideInInspector] public float pushTimer = 0f;

    [Tooltip("Assign the 'PushPoint' child object here.")]
    public Transform pushPoint;
    [Tooltip("Layers that can be pushed (e.g., Enemy).")]
    public LayerMask enemyLayer;

    [Header("Audio")]
    public string pushSound = "PushAttack"; // Make sure to add this to AudioManager!

    Animator anim;

    void Awake() {

        anim = GetComponent<Animator>();

    }

    void Update() {
        // 1. Manage Timer (Count Down)
        if (pushTimer > 0) {
            pushTimer -= Time.deltaTime;
        }

        HandlePush();
    }

    void HandlePush() {
        // Only push if Cooldown is 0 AND button pressed
        if (Input.GetKeyDown(KeyCode.Mouse0) && pushTimer <= 0) {
            PerformPush();
        }
    }

    void PerformPush() {
        // Reset Timer
        pushTimer = pushCooldown;
        Debug.Log("Push Activated");

        anim.SetTrigger("Push");

        // Play Audio
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlayPlayerSound(pushSound);
        }

        if (pushPoint != null) {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(pushPoint.position, pushRadius, enemyLayer);

            foreach (Collider2D enemy in hitEnemies) {
                // 1. Physics Push
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null) {
                    Vector2 dir = (enemy.transform.position - transform.position).normalized;
                    dir += Vector2.up * 0.5f; // Add slight lift

                    // Apply Force
                    enemyRb.AddForce(dir * pushForce * 1000, ForceMode2D.Impulse);
                }

                // 2. Logic Stun
                EnemyAI enemyScript = enemy.GetComponent<EnemyAI>();
                if (enemyScript != null) {
                    enemyScript.GetStunned(5.0f);
                }
            }
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        if (pushPoint) Gizmos.DrawWireSphere(pushPoint.position, pushRadius);
    }
}