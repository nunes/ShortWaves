using UnityEngine;
using System.Collections.Generic;

public class SceneSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPoint
    {
        public string previousSceneName;
        public Transform spawnTransform;
    }

    [Header("Player Prefab")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn Configuration")]
    [SerializeField] private Transform defaultSpawnPoint;
    [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    private void Start()
    {
        StartCoroutine(PositionPlayerRoutine());
    }

    private System.Collections.IEnumerator PositionPlayerRoutine()
    {
        // Wait for one frame to ensure all other initialization is done
        yield return new WaitForEndOfFrame();
        SpawnOrPositionPlayer();
    }

    private void SpawnOrPositionPlayer()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        // Determine target spawn point
        Transform targetSpawn = defaultSpawnPoint;
        string previousScene = GameLoopManager.Instance != null ? GameLoopManager.Instance.PreviousSceneName : "";

        Debug.Log($"SceneSpawnManager: Previous Scene: '{previousScene}'");

        // Find matching spawn point
        if (!string.IsNullOrEmpty(previousScene))
        {
            foreach (var sp in spawnPoints)
            {
                if (sp.previousSceneName == previousScene && sp.spawnTransform != null)
                {
                    targetSpawn = sp.spawnTransform;
                    break;
                }
            }
        }

        if (targetSpawn == null)
        {
            Debug.LogError("SceneSpawnManager: No valid spawn point found!");
            return;
        }

        // If no player exists, spawn one
        if (player == null)
        {
            if (playerPrefab != null)
            {
                Debug.Log($"SceneSpawnManager: No player found. Spawning player prefab at {targetSpawn.position}");
                player = Instantiate(playerPrefab, targetSpawn.position, targetSpawn.rotation);
                
                // Ensure player has the "Player" tag
                if (!player.CompareTag("Player"))
                {
                    player.tag = "Player";
                    Debug.LogWarning("SceneSpawnManager: Player prefab didn't have 'Player' tag. Tag has been set.");
                }
            }
            else
            {
                Debug.LogError("SceneSpawnManager: No player found in scene and no player prefab assigned!");
                return;
            }
        }
        else
        {
            // Player exists, just reposition it
            Debug.Log($"SceneSpawnManager: Player found. Repositioning to {targetSpawn.position}");
            PositionExistingPlayer(player, targetSpawn);
        }
    }

    private void PositionExistingPlayer(GameObject player, Transform targetSpawn)
    {
        // Handle CharacterController if present (it overrides transform.position)
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = targetSpawn.position;
        player.transform.rotation = targetSpawn.rotation;
        
        // Force physics update
        Physics.SyncTransforms();

        if (cc != null) cc.enabled = true;
        
        // Reset physics velocity if needed
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
