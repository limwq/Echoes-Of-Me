using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class EnemyAI : MonoBehaviour {
    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float patrolWaitTime = 1f;

    [Header("Detection Settings")]
    public float detectionRadius = 5f;
    public float attackRange = 1.2f;
    public float killRange = 1.2f;

    [Header("Combat Settings")]
    public float attackWindupTime = 1.0f;
    public Color alertColor = Color.red;
    public Color stunColor = Color.blue;
    public float knockbackResistance = 0.8f;

    [Header("Visual Effects")]
    public ParticleSystem stunParticles; // <-- NEW: Drag your particle system here

    [Header("Audio Clips")]
    public AudioClip alertSound;
    public AudioClip attackSound;
    public AudioClip stunSound;

    private AudioSource source;

    [Header("Patrol Path (Offsets)")]
    [Tooltip("Add points here. (0,0) is the enemy's starting position.")]
    public List<Vector2> patrolPoints = new List<Vector2>();

    // --- Internal State ---
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private Color originalColor;

    private int currentPointIndex = 0;
    private Vector2 startPosition;

    private bool isStunned;
    private bool isAttacking;
    private bool isWaitingAtPatrolPoint;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        source = GetComponent<AudioSource>();
        originalColor = sr.color;

        startPosition = transform.position;

        if (patrolPoints.Count == 0) {
            patrolPoints.Add(Vector2.zero);
        }

        // Ensure particles start OFF
        if (stunParticles != null) stunParticles.Stop();
    }

    void Update() {
        if (player == null) {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) {
                player = p.transform;
            } else {
                return;
            }
        }

        if (isStunned || isAttacking || player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // --- STATE MACHINE ---
        if (distToPlayer <= detectionRadius) {
            // FIX: Prevent head balancing
            if (player.position.y > transform.position.y + 1f) {
                MoveAway(player.position, chaseSpeed);
            } else if (distToPlayer <= attackRange) {
                StartCoroutine(AttackSequence());
            } else {
                MoveTowards(player.position, chaseSpeed);
            }
        } else {
            PatrolLogic();
        }
    }

    void PatrolLogic() {
        if (isWaitingAtPatrolPoint || patrolPoints.Count == 0) return;

        Vector2 worldTarget = startPosition + patrolPoints[currentPointIndex];

        MoveTowards(worldTarget, patrolSpeed);

        if (Vector2.Distance(transform.position, worldTarget) < 0.2f) {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    void MoveTowards(Vector2 target, float speed) {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);

        if (dir.x > 0 && transform.localScale.x < 0) Flip();
        else if (dir.x < 0 && transform.localScale.x > 0) Flip();
    }

    void MoveAway(Vector2 target, float speed) {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.velocity = new Vector2(-dir.x * speed, rb.velocity.y);

        if (dir.x > 0 && transform.localScale.x < 0) Flip();
        else if (dir.x < 0 && transform.localScale.x > 0) Flip();
    }

    IEnumerator WaitAtPatrolPoint() {
        isWaitingAtPatrolPoint = true;
        rb.velocity = new Vector2(0, rb.velocity.y);

        yield return new WaitForSeconds(patrolWaitTime);

        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
        isWaitingAtPatrolPoint = false;
    }

    // --- COMBAT LOGIC ---

    IEnumerator AttackSequence() {
        isAttacking = true;
        if (alertSound != null) source.PlayOneShot(alertSound);

        rb.velocity = Vector2.zero;
        sr.color = alertColor;

        yield return new WaitForSeconds(attackWindupTime);

        if (attackSound != null) source.PlayOneShot(attackSound);

        if (player != null && !isStunned) {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= killRange) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        sr.color = originalColor;
        isAttacking = false;
    }

    public void GetStunned(float duration) {
        if (stunSound != null) source.PlayOneShot(stunSound);

        StopAllCoroutines();
        StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration) {
        isStunned = true;
        isAttacking = false;
        isWaitingAtPatrolPoint = false;

        // Visuals ON
        sr.color = stunColor;
        if (stunParticles != null) stunParticles.Play(); // <-- START PARTICLES

        yield return new WaitForSeconds(duration);

        // Visuals OFF
        isStunned = false;
        sr.color = originalColor;
        if (stunParticles != null) stunParticles.Stop(); // <-- STOP PARTICLES
    }

    void Flip() {
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (patrolPoints.Count > 0) {
            Gizmos.color = Color.green;
            Vector2 basePos = Application.isPlaying ? startPosition : (Vector2)transform.position;
            for (int i = 0; i < patrolPoints.Count; i++) {
                Vector2 p1 = basePos + patrolPoints[i];
                Vector2 p2 = basePos + patrolPoints[(i + 1) % patrolPoints.Count];
                Gizmos.DrawSphere(p1, 0.2f);
                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}