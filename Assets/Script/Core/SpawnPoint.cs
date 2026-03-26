using UnityEngine;
using Cinemachine;

public class SpawnPoint : MonoBehaviour {
    [Header("Settings")]
    public GameObject playerPrefab;

    [Header("Camera Connection")]
    [Tooltip("Drag your CM vcam1 here.")]
    public CinemachineVirtualCamera virtualCamera;

    void Start() {
        if (playerPrefab != null) {
            // 1. Spawn the Player and save it in a variable called 'newPlayer'
            GameObject newPlayer = Instantiate(playerPrefab, transform.position, Quaternion.identity);

            // 2. Tell the Camera to follow this new player
            if (virtualCamera != null) {
                virtualCamera.Follow = newPlayer.transform;
                // If you use 'LookAt', uncomment the line below:
                // virtualCamera.LookAt = newPlayer.transform; 
            } else {
                // Fallback: If you forgot to assign the camera, try to find it automatically
                var cam = FindFirstObjectByType<CinemachineVirtualCamera>();
                if (cam != null) cam.Follow = newPlayer.transform;
            }
        } else {
            Debug.LogError("SpawnPoint: No Player Prefab assigned!");
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}