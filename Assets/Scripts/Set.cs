using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class Set : MonoBehaviour
{
    [SerializeField] public bool containsUpperLevel = false;
    [SerializeField] public bool containsLowerLevel = false;
    
    [SerializeField] private List<GameObject> wallVariants = new List<GameObject>();
    
    public Color TintColour;

    private List<WallVariant> walls = new List<WallVariant>();
    
    void GenerateWall(GameObject obstacleSpawnPoint, WallVariant previousWallVariant = null, bool noJump = false)
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
            // TODO: Pick from a new list with only wall variants without a jump requirement
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
        MeshRenderer[] wallObstacles = wall.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer obstacle in wallObstacles)
        {
            if (obstacle.materials[0].HasColor("_Colour"))
            {
                obstacle.materials[0].SetColor("_Colour", TintColour);
            }
        }
        
        wall.transform.position = obstacleSpawnPoint.transform.position;
        walls.Add(wall.GetComponent<WallVariant>());
    }

    public async Task GenerateAllWalls()
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
                    GenerateWall(child.gameObject, previousWallVariant: walls[walls.Count - 1]);
                }
                else
                {
                    GenerateWall(child.gameObject, noJump: true);
                }
            }
        }
    }
    
    void Start()
    {
        Random.InitState((int)DateTime.Now.Ticks);
        
        wallVariants = GameObject.FindGameObjectWithTag("SetManager").GetComponent<SetManager>().wallVariants;
        
        List<Color> colourOptions = new List<Color>();
        colourOptions.Add(Color.blue);
        colourOptions.Add(Color.cyan);
        colourOptions.Add(Color.gray);
        colourOptions.Add(Color.green);
        colourOptions.Add(Color.magenta);
        colourOptions.Add(Color.red);
        colourOptions.Add(Color.white);
        colourOptions.Add(Color.yellow);

        int randomColourIndex = Random.Range(0, colourOptions.Count);
        
        TintColour = colourOptions[randomColourIndex];

        MeshRenderer[] obstacles = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer obstacle in obstacles)
        {
            if (obstacle.materials[0].HasColor("_Colour"))
            {
                obstacle.materials[0].SetColor("_Colour", TintColour);
            }
        }

        GenerateAllWalls();
    }
}
