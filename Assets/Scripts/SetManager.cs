using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
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
                
                // Find the the parent set's last wall, if any
                List<WallVariant> wallVariants = this.OwnSet.walls;
                WallVariant lastWallVariant = null;
                if (wallVariants.Count > 0)
                {
                    lastWallVariant = wallVariants[wallVariants.Count - 1];
                }
                
                // Pick a random set from those among the allowed sets
                SeedManager seedManager = GameObject.FindGameObjectWithTag("SeedManager").GetComponent<SeedManager>();

                if (lastWallVariant)
                {
                    List<Set> newAllowedSets = new List<Set>();
                    foreach (Set set in allowedSets)
                    {
                        bool isBanned = false;
                        
                        foreach (Set bannedSet in lastWallVariant.BannedSets)
                        {
                            if (set.name == bannedSet.name)
                            {
                                isBanned = true;
                            }
                        }

                        if (!isBanned)
                        {
                            newAllowedSets.Add(set);
                        }
                    }

                    allowedSets = newAllowedSets;
                }
                
                int newSetIndex = seedManager.RandomRange(0, allowedSets.Count);
                Set newSet = allowedSets[newSetIndex];
                
                // Create the new set and set its position
                GameObject newSetObject = Instantiate(newSet.gameObject);
                newSetObject.transform.position = setSPObject.transform.position;
                newSetObject.transform.Translate(0, -1, 0);

                newSetObject.GetComponent<Set>().GenerateAllWalls(lastWallVariant);

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

        public List<SetNode> GetEndNodes(Set set, List<SetNode> endNodes = null)
        {
            if (endNodes == null)
            {
                endNodes = new List<SetNode>();
            }

            foreach (SetNode child in Children)
            {
                if (!child.IsExpanded)
                {
                    endNodes.Add(child);
                }
                else
                {
                    child.GetEndNodes(set, endNodes);
                }
            }

            return endNodes;
        }
    }

    [SerializeField]
    public List<GameObject> wallVariants = new List<GameObject>();

    [SerializeField] public List<Set> setVariants = new List<Set>();

    public SetNode tree;

    private List<SetNode> unloadQueue;

    private Transform playerTransform;

    private SeedManager seedManager;

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
        seedManager = GameObject.FindGameObjectWithTag("SeedManager").GetComponent<SeedManager>();
        unloadQueue = new List<SetNode>();
        Time.timeScale = 1;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        int randomRootSetIndex = seedManager.RandomRange(0, setVariants.Count);
        GameObject rootSet = Instantiate(setVariants[randomRootSetIndex].gameObject);
        rootSet.transform.position.Set(0.0f, 0.0f, 0.0f);
        tree = new SetNode(rootSet.GetComponent<Set>());
        tree.OwnSet.GenerateAllWalls();
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
