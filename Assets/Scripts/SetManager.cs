using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class SetManager : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> wallVariants = new List<GameObject>();
    
    // A list containing each unique set
    [SerializeField] private GameObject setPrefab;

    IEnumerator LoadSets()
    {
        Debug.Log("Loading sets...");
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        GameObject[] loadedSets = GameObject.FindGameObjectsWithTag("Set");

        if (loadedSets.Length > 0)
        {
            GameObject farthestSet = loadedSets[0];

            GameObject currentSet = null;
            foreach (GameObject set in loadedSets)
            {
                BoxCollider setCollider = set.GetComponent<BoxCollider>();

                // Doesn't matter how high/low the player is,
                // therefore, use the set's y value

                Vector3 point = new Vector3(
                    set.transform.position.x,
                    set.transform.position.y,
                    playerTransform.position.z
                );

                if (setCollider.bounds.Contains(point))
                {
                    currentSet = set;
                    break;
                }
            }

            if (currentSet != null)
            {
                foreach (GameObject set in loadedSets)
                {
                    if (set == currentSet)
                        continue;

                    if (set.transform.position.z > currentSet.transform.position.z)
                    {
                        if (set.transform.position.z > farthestSet.transform.position.z)
                        {
                            farthestSet = set;
                        }
                    }
                    else
                    {
                        Destroy(set);
                    }
                }
            }

            while (GameObject.FindGameObjectsWithTag("Set").Length < 3)
            {
                GameObject newSet = Instantiate(setPrefab);
                newSet.transform.position = farthestSet.transform.position;
                newSet.transform.Translate(0.0f, 0.0f, 16.0f);
                farthestSet = newSet;
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(LoadSets());
    }

    public void Start()
    {
        Random.InitState((int)DateTime.Now.Ticks);
        StartCoroutine(LoadSets());
    }
}
