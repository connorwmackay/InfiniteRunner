using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class Set : MonoBehaviour
{
    [SerializeField] public bool containsUpperLevel = false;
    [SerializeField] public bool containsLowerLevel = false;
    
    [SerializeField] private List<GameObject> wallVariants = new List<GameObject>();

    public Color TintColour;

    public List<WallVariant> walls;

    private SeedManager seedManager;

    void GenerateWall(GameObject obstacleSpawnPoint, WallVariant previousWallVariant = null, bool noJump = false)
    {
        List<GameObject> allowedWallVariants = new List<GameObject>();

        if (previousWallVariant != null)
        {
            allowedWallVariants = previousWallVariant.AllowedFollowUpWalls;
        }
        else
        {
            foreach (GameObject wallVariant in wallVariants)
            {
                if (!wallVariant.GetComponent<WallVariant>().IsJump)
                {
                    allowedWallVariants.Add(wallVariant);
                }
                else if (!noJump && wallVariant.GetComponent<WallVariant>().IsJump)
                {
                    allowedWallVariants.Add(wallVariant);
                }
            }
        }

        int wallVariantIndex = seedManager.RandomRange(0, allowedWallVariants.Count);

        GameObject wall = Instantiate(allowedWallVariants[wallVariantIndex], obstacleSpawnPoint.transform);
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

    public void GenerateAllWalls(WallVariant prevSetLastWall = null)
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        List<Transform> sortedChildren = new List<Transform>();
        sortedChildren.AddRange(children);
        sortedChildren = sortedChildren.OrderBy(o => o.position.z).ToList();
        foreach (Transform child in sortedChildren)
        {
            if (child.gameObject.CompareTag("ObstacleSpawnPoint"))
            {
                if (walls.Count > 0)
                {
                    prevSetLastWall = walls[walls.Count - 1];
                }
                
                if (prevSetLastWall != null)
                {
                    GenerateWall(child.gameObject, previousWallVariant: prevSetLastWall);
                }
                else
                {
                    GenerateWall(child.gameObject, noJump: true);
                }
            }
        }
    }
    
    void Awake()
    {
        walls = new List<WallVariant>();
        seedManager = GameObject.FindGameObjectWithTag("SeedManager").GetComponent<SeedManager>();
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

        int randomColourIndex = seedManager.RandomRange(0, colourOptions.Count);
        
        TintColour = colourOptions[randomColourIndex];

        MeshRenderer[] obstacles = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer obstacle in obstacles)
        {
            if (obstacle.materials[0].HasColor("_Colour"))
            {
                obstacle.materials[0].SetColor("_Colour", TintColour);
            }
        }
    }
}
