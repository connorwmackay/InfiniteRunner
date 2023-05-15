using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class SetManager : MonoBehaviour
{
    public class SetNode
    {
        private static int nextId = 0;
        
        public int Id { get; set; }
        
        public SetNode Parent { get; set; }
        public List<SetNode> Children { get; set; }
        
        public Set OwnSet { get; set; }
        
        public bool IsExpanded { get; set; }

        public SetNode(Set ownSet, SetNode parent = null, List<SetNode> children = null)
        {
            Parent = parent;
            OwnSet = ownSet;
            IsExpanded = false;
            Id = nextId++;
            
            if (children == null)
            {
                Children = new List<SetNode>();
            }
            else
            {
                Children = children;
            }
        }

        public void Expand(List<Set> setVariants)
        {
            if (IsExpanded)
            {
                // Stop expanding the node
                Debug.LogWarning("Node has already been expanded.");
                return;
            }
            
            // Continue to expand the node
            SetSpawnPoint[] setSpawnPoints = OwnSet.gameObject.GetComponentsInChildren<SetSpawnPoint>();
            foreach (SetSpawnPoint setSpawnPoint in setSpawnPoints)
            {
                GameObject setSPObject = setSpawnPoint.gameObject;

                // Remove all sets that would cause a conflict
                List<Set> allowedSets = new List<Set>();
                foreach (Set set in setVariants)
                {
                    if ((!setSpawnPoint.isUpperLevelAllowed && set.containsUpperLevel))
                    { } 
                    else if (!setSpawnPoint.isLowerLevelAllowed && set.containsLowerLevel)
                    { }
                    else
                    {
                        allowedSets.Add(set);
                    }
                }
                
                // Pick a random set from those among the allowed sets
                int newSetIndex = Random.Range(0, allowedSets.Count);
                Set newSet = allowedSets[newSetIndex];
                
                // Create the new set and set its position
                GameObject newSetObject = Instantiate(newSet.gameObject);
                newSetObject.transform.position = setSPObject.transform.position;
                newSetObject.transform.Translate(0, -1, 0);

                // Add the set to the tree
                SetNode newChild = new SetNode(newSetObject.GetComponent<Set>(), this);
                Children.Add(newChild);
            }
            
            // Flag this node as being expanded
            IsExpanded = true;
        }

        public void ExpandEndNodes(List<Set> setVariants)
        {
            foreach (SetNode child in Children)
            {
                if (!child.IsExpanded)
                {
                    child.Expand(setVariants);
                }
                else
                {
                    child.ExpandEndNodes(setVariants);
                }
            }
        }

        public SetNode FindNewRoot()
        {
            Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            
            // Copy of player position
            Vector3 playerPosition = new Vector3(
                playerTransform.position.x, 
                playerTransform.position.y, 
                playerTransform.position.z
            );
            
            SetNode newRootNode = this;
            
            // Correct the position to be the start of the current set
            playerPosition.z += playerPosition.z % 12;
            playerPosition.z -= 12.0f;

            float playerRootDistance = Vector3.Distance(
                newRootNode.OwnSet.gameObject.transform.position,
                playerPosition
            );
            
            foreach (SetNode child in Children)
            {
                SetNode potentialRootNode = child.FindNewRoot();

                float playerChildDistance = Vector3.Distance(
                    child.OwnSet.gameObject.transform.position,
                    playerPosition
                );
                
                float playerPRootDistance = Vector3.Distance(
                    potentialRootNode.OwnSet.gameObject.transform.position,
                    playerPosition
                );
                
                if (playerChildDistance < playerPRootDistance)
                {
                    playerPRootDistance = playerChildDistance;
                }

                if (playerPRootDistance < playerRootDistance)
                {
                    newRootNode = potentialRootNode;
                    playerRootDistance = playerPRootDistance;
                }
            }
            
            return newRootNode;
        }
        
        public void Unload(SetNode ignoreBranch, ref List<SetNode> unloadQueue)
        {
            Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            
            // Copy of player position
            Vector3 playerPosition = new Vector3(
                playerTransform.position.x, 
                playerTransform.position.y, 
                playerTransform.position.z
            );

            // Correct the position to be the start of the current set
            playerPosition.z += playerPosition.z % 12;
            playerPosition.z -= 12.0f;
            
            Parent = null;
            foreach (SetNode child in Children)
            {
                if (child.Id != ignoreBranch.Id)
                {
                    child.Unload(ignoreBranch, ref unloadQueue);
                }
            }
            
            unloadQueue.Add(this);
        }
    }

    [SerializeField]
    public List<GameObject> wallVariants = new List<GameObject>();

    [SerializeField] public List<Set> setVariants = new List<Set>();

    private SetNode tree;

    private List<SetNode> unloadQueue;

    private Transform playerTransform;

    IEnumerator TraverseSets()
    {
        // Find the new tree
        SetNode newTree = tree.FindNewRoot();
        
        // If the tree has changed
        if (newTree.Id != tree.Id)
        {
            // Delete the new root node's parent set
            newTree.Parent = null;
            
            // Expand unexpanded nodes in new tree
            if (!newTree.IsExpanded)
            {
                newTree.Expand(setVariants);
            }
            else
            {
                newTree.ExpandEndNodes(setVariants);
            }

            // Update the tree
            UnloadOldTree(newTree);
            Vector3 currentSetTransform = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y,
                playerTransform.position.z
            );
            currentSetTransform.z += currentSetTransform.z % 12;
            float setLength = 12.0f;
            currentSetTransform.z -= setLength * 2;
            
            for (int i=0; i < unloadQueue.Count; i++)
            {
                SetNode node = unloadQueue[i];

                float distanceFromPlayer = Vector3.Distance(
                    new Vector3(
                        playerTransform.position.x,
                        playerTransform.position.y,
                        currentSetTransform.z + setLength
                    ),
                    node.OwnSet.transform.position
                );
                
                if (node.OwnSet.transform.position.z < currentSetTransform.z)
                {
                    unloadQueue.RemoveAt(i);
                    Destroy(node.OwnSet.gameObject);
                }
            }
            
            tree = newTree;
        }

        // Wait 0.5 seconds
        yield return new WaitForSeconds(0.5f);
        
        // Call this function again after the wait.
        StartCoroutine(TraverseSets());
    }

    void UnloadOldTree(SetNode newTree)
    {
        SetNode oldTree = tree;
        oldTree.Unload(newTree, ref unloadQueue);
    }

    async Task InitialSetSpawn()
    {
        tree.Expand(setVariants);
        foreach (SetNode child in tree.Children)
        {
            child.Expand(setVariants);
        }
    }

    public void Start()
    {
        unloadQueue = new List<SetNode>();
        Time.timeScale = 1;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        Set rootSet = GameObject.FindWithTag("Set").GetComponent<Set>();
        tree = new SetNode(rootSet);
        Random.InitState((int)DateTime.Now.Ticks);
        InitialSetSpawn();
        StartCoroutine(TraverseSets());
    }

    public void Update()
    {
        Vector3 playerPosition = playerTransform.position;
        if (Vector3.Distance(playerPosition, tree.OwnSet.transform.position) > 50.0f)
        {
            GameObject scoreManagerObject = GameObject.FindGameObjectWithTag("ScoreManager");
            ScoreManager scoreManager = scoreManagerObject.GetComponent<ScoreManager>();
            int score = scoreManager.GetScore();
            DeathScreen deathScreen = GameObject.FindGameObjectWithTag("DeathMenu").GetComponent<DeathScreen>();
            deathScreen.ShowDeathScreen(score);
        }
    }
}
