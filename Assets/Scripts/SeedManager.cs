using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SeedManager : MonoBehaviour
{
    private int _seed {get; set;}

    // Start is called before the first frame update
    void Start()
    {
        _seed = (int)DateTime.Now.Ticks;
        Random.InitState(_seed);
    }

    public int RandomRange(int minInclusive, int maxExclusive)
    {
        return Random.Range(minInclusive, maxExclusive);
    }
    
    public float RandomRange(float minInclusive, float maxExclusive)
    {
        return Random.Range(minInclusive, maxExclusive);
    }
}
