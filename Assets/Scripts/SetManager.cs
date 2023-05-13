using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class SetManager : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> wallVariants = new List<GameObject>();

    [SerializeField] public List<Set> setVariants = new List<Set>();

    IEnumerator LoadSets()
    {
        Debug.Log("Loading sets...");
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        GameObject[] loadedSets = GameObject.FindGameObjectsWithTag("Set");

        bool wasDestroyed = false;
        if (loadedSets.Length > 0)
        {
            foreach (GameObject set in loadedSets)
            {
                if (set.transform.position.z + 12.0f < playerTransform.position.z)
                {
                    Destroy(set);
                    wasDestroyed = true;
                }
            }
        }

        if (wasDestroyed)
        {
            SpawnNewSets();
        }

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(LoadSets());
    }

    public async Task SpawnNewSets()
    {
        GameObject[] setSpawnPoints = GameObject.FindGameObjectsWithTag("SetSpawnPoint");

        foreach (GameObject setSPObject in setSpawnPoints)
        {
            SetSpawnPoint setSpawnPoint = setSPObject.GetComponent<SetSpawnPoint>();

            bool isSetAbove = false;
            bool isSetBelow = false;

            GameObject[] sets = GameObject.FindGameObjectsWithTag("Set");
            foreach (GameObject set in sets)
            {
                if (Math.Floor(set.transform.position.z) == Math.Floor(setSPObject.transform.position.z))
                {
                    if (set.transform.position.y > setSPObject.transform.position.y)
                    {
                        isSetAbove = true;
                    }
                    
                    if (set.transform.position.y < setSPObject.transform.position.y)
                    {
                        isSetBelow = true;
                    }
                }
            }

            List<Set> allowedSets = new List<Set>();
            foreach (Set set in setVariants)
            {
                if ((!setSpawnPoint.isUpperLevelAllowed && set.containsUpperLevel) ||
                    (isSetAbove && set.containsUpperLevel))
                { } 
                else if (!setSpawnPoint.isLowerLevelAllowed && set.containsLowerLevel ||
                         isSetBelow && set.containsLowerLevel)
                { }
                else
                {
                    allowedSets.Add(set);
                }
            }

            int newSetIndex = Random.Range(0, allowedSets.Count);
            Set newSet = allowedSets[newSetIndex];
            GameObject newSetObject = Instantiate(newSet.gameObject);
            newSetObject.transform.position = setSPObject.transform.position;
            newSetObject.transform.Translate(0, -1, 0);
            
            setSpawnPoint.gameObject.SetActive(false);
            Destroy(setSpawnPoint.gameObject);
        }
        
        Debug.Log("Num Spawn Points After: " + setSpawnPoints.Length);
    }

    async Task InitialSetSpawn()
    {
        await SpawnNewSets();
        await SpawnNewSets();
        await SpawnNewSets();
        await SpawnNewSets();
    }

    public void Start()
    {
        Random.InitState((int)DateTime.Now.Ticks);
        InitialSetSpawn();
        StartCoroutine(LoadSets());
    }
}
