using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Scripts.Data
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Level/Level Data Asset", order = 0)]
    public class LevelDataAsset : ScriptableObject
    {
        public FoodGraphAsset foodGraphAsset;

        [SerializeField]
        public List<string> initialFoodNodeIds = new List<string>();

        [SerializeField]
        public List<SerializedCustomerData> customers = new List<SerializedCustomerData>();

        /// <summary>
        /// Reconstructs and returns a runtime LevelData object from this asset's serialized data.
        /// </summary>
        public LevelData GetLevelData()
        {
            Dictionary<string, FoodNodeData> nodeMap = new Dictionary<string, FoodNodeData>(StringComparer.Ordinal);

            // 1. Gather nodes from assigned FoodGraphAsset or find all FoodGraphAssets in project/resources
            if (foodGraphAsset != null)
            {
                if (foodGraphAsset.Nodes == null || foodGraphAsset.Nodes.Count == 0)
                {
                    foodGraphAsset.ReconstructGraph();
                }

                foreach (var node in foodGraphAsset.Nodes)
                {
                    if (node != null && !string.IsNullOrEmpty(node.ID))
                    {
                        nodeMap[node.ID] = node;
                    }
                }
            }

            // Fallback: If nodeMap is empty or incomplete, try finding food graph assets in Editor/Resources
#if UNITY_EDITOR
            if (nodeMap.Count == 0)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:FoodGraphAsset");
                foreach (string guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var graphAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<FoodGraphAsset>(path);
                    if (graphAsset != null)
                    {
                        if (graphAsset.Nodes == null || graphAsset.Nodes.Count == 0)
                        {
                            graphAsset.ReconstructGraph();
                        }
                        foreach (var node in graphAsset.Nodes)
                        {
                            if (node != null && !string.IsNullOrEmpty(node.ID))
                            {
                                nodeMap[node.ID] = node;
                            }
                        }
                    }
                }
            }
#endif

            // 2. Reconstruct initial food nodes
            List<FoodNodeData> initialFoodNodes = new List<FoodNodeData>();
            if (initialFoodNodeIds != null)
            {
                foreach (var nodeId in initialFoodNodeIds)
                {
                    if (!string.IsNullOrEmpty(nodeId) && nodeMap.TryGetValue(nodeId, out var nodeData))
                    {
                        initialFoodNodes.Add(nodeData);
                    }
                }
            }

            // 3. Reconstruct customer data
            List<CustomerData> customerList = new List<CustomerData>();
            if (customers != null)
            {
                foreach (var sCustomer in customers)
                {
                    if (sCustomer == null) continue;

                    List<FoodNodeData> requiredFoods = new List<FoodNodeData>();
                    if (sCustomer.requireFoodNodeIds != null)
                    {
                        foreach (var foodId in sCustomer.requireFoodNodeIds)
                        {
                            if (!string.IsNullOrEmpty(foodId) && nodeMap.TryGetValue(foodId, out var reqFood))
                            {
                                requiredFoods.Add(reqFood);
                            }
                        }
                    }

                    customerList.Add(new CustomerData
                    {
                        id = string.IsNullOrEmpty(sCustomer.id) ? Guid.NewGuid().ToString() : sCustomer.id,
                        sprite = sCustomer.sprite,
                        requireFood = requiredFoods,
                        spawnTurn = sCustomer.spawnTurn,
                        served = false
                    });
                }
            }

            return new LevelData
            {
                initialFoodNodes = initialFoodNodes,
                allCustomerData = new AllCustomerData
                {
                    allCustomerData = customerList,
                    createdAtTurn = 0
                }
            };
        }
    }

    [Serializable]
    public class SerializedCustomerData
    {
        public string id = Guid.NewGuid().ToString();
        public Sprite sprite;
        public List<string> requireFoodNodeIds = new List<string>();
        public int spawnTurn = 0;
    }
}
