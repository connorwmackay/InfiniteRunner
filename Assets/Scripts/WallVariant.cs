using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallVariant : MonoBehaviour
{
    [SerializeField] public bool IsJump = false;
    [SerializeField] public List<GameObject> AllowedFollowUpWalls = new List<GameObject>();
    [SerializeField] public List<Set> BannedSets = new List<Set>();
}
