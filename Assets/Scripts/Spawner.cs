using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public GameObject dropPrefab; // Assign your drop prefab in the Inspector
    public int gridSize = 20;     // Grid size (20x20)
    public int dropsToSpawn = 5;  // Number of drops to spawn each time

    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
    private List<GameObject> spawnedObjects = new List<GameObject>(); // Track all spawned objects

    public void Spawn()
    {
        // Clear existing positions and objects
        ClearAllSpawnedObjects();

        // Spawn new drops
        for (int i = 0; i < dropsToSpawn; i++)
        {
            Vector2Int position = GetRandomPosition();
            // Center the drop in the grid cell (e.g., (0,0) -> (0.5, 0, 0.5))
            Vector3 worldPosition = new Vector3(position.x + 0.5f, 0, position.y + 0.5f);
            GameObject drop = Instantiate(dropPrefab, worldPosition, Quaternion.identity);
            
            // Add to tracking list
            spawnedObjects.Add(drop);

            // Set up the Collectable script
            Collectable collectable = drop.GetComponent<Collectable>();
            if (collectable != null)
            {
                collectable.gridPosition = position;
                collectable.spawner = this;
            }
            else
            {
                Debug.LogWarning("Spawned drop does not have a Collectable component!");
            }

            occupiedPositions.Add(position);
        }
    }

    private Vector2Int GetRandomPosition()
    {
        Vector2Int position;
        int maxAttempts = 100; // Prevent infinite loops in case grid is full
        int attempts = 0;

        do
        {
            position = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
            attempts++;
            if (attempts > maxAttempts)
            {
                Debug.LogError("Could not find an unoccupied position after " + maxAttempts + " attempts!");
                return position; // Fallback to last position
            }
        } while (occupiedPositions.Contains(position));

        return position;
    }

    public void DropCollected(Vector2Int gridPos)
    {
        // Remove the position from occupiedPositions when a drop is collected
        if (occupiedPositions.Remove(gridPos))
        {
            Debug.Log($"Drop collected at {gridPos}. Remaining drops: {occupiedPositions.Count}");
            if (occupiedPositions.Count == 0)
            {
                Debug.Log("All drops collected! Respawning new set.");
                Spawn(); // Respawn when all drops are collected
            }
        }
        else
        {
            Debug.LogWarning($"Attempted to remove non-existent grid position: {gridPos}");
        }
    }

    public void ClearAllSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
        occupiedPositions.Clear();
    }

    public void ResetSpawner()
    {
        ClearAllSpawnedObjects();
        // Any additional reset logic can go here if needed
        // For example, resetting dropsToSpawn or gridSize to initial values
    }
}