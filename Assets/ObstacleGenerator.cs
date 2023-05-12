using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObstacleGenerator : MonoBehaviour
{

    private List<GameObject> wallVariants;
    private List<WallVariant> walls = new List<WallVariant>();
    
    void Generate(GameObject obstacleSpawnPoint, WallVariant previousWallVariant = null, bool noJump = false)
    {
        int wallVariantIndex = Random.Range(0, wallVariants.Count);

        if (previousWallVariant != null)
        {
            if (previousWallVariant.isJumpRequired)
            {
                noJump = true;
            }
        }
        
        if (noJump)
        {
            // Prevent a potential infinite loop
                int iterations = 0;
            while (wallVariants[wallVariantIndex].GetComponent<WallVariant>().isJumpRequired && iterations < 150)
            {
                wallVariantIndex = Random.Range(0, wallVariants.Count);
                iterations++;
            }

            if (iterations >= 150)
            {
                Debug.LogWarning("Obstacle Generator: Exceeded max. iterations");
            }
        }

        GameObject wall = Instantiate(wallVariants[wallVariantIndex], obstacleSpawnPoint.transform);
        wall.transform.position = obstacleSpawnPoint.transform.position;
        walls.Add(wall.GetComponent<WallVariant>());
    }
    
    void Start()
    {        
        walls = new List<WallVariant>();
        
        Random.InitState((int)DateTime.Now.Ticks);
        SetManager setManager = GameObject.FindWithTag("SetManager").GetComponent<SetManager>();
        wallVariants = setManager.wallVariants;
        
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.gameObject.CompareTag("ObstacleSpawnPoint"))
            {
                if (walls.Count > 0)
                {
                    Generate(child.gameObject, walls[walls.Count - 1]);
                }
                else
                {
                    Generate(child.gameObject, noJump: true);
                }
            }
        }
    }
}
