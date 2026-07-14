using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Scripts.Data
{
    [CreateAssetMenu(fileName = "NewFoodGraph", menuName = "Food Graph/Asset", order = 0)]
    public class FoodGraphAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<SerializedNode> serializedNodes = new List<SerializedNode>();

        [SerializeField]
        private List<SerializedEdge> serializedEdges = new List<SerializedEdge>();

        // Runtime node list with reconstructed references.
        [NonSerialized]
        public List<FoodNodeData> Nodes = new List<FoodNodeData>();

        public List<SerializedNode> SerializedNodes => serializedNodes;
        public List<SerializedEdge> SerializedEdges => serializedEdges;

        public void OnBeforeSerialize()
        {
            // Handled explicitly by the Editor serializer on save.
        }

        public void OnAfterDeserialize()
        {
            ReconstructGraph();
        }

        public void ReconstructGraph()
        {
            Nodes.Clear();
            if (serializedNodes == null) return;

            // 1. Create instances of FoodNodeData and copy primitive values.
            Dictionary<string, FoodNodeData> nodeMap = new Dictionary<string, FoodNodeData>(StringComparer.Ordinal);
            foreach (var sNode in serializedNodes)
            {
                if (string.IsNullOrEmpty(sNode.id)) continue;

                var node = new FoodNodeData
                {
                    ID = sNode.id,
                    Name = sNode.name,
                    Sprite = sNode.sprite,
                    TurnsToCreate = sNode.turnsToCreate,
                    BaseBuyInCost = sNode.baseBuyInCost,
                    AutoTransformIn = sNode.autoTransformIn,
                    AutoDestroyIn = sNode.autoDestroyIn,
                    Enabled = sNode.enabled
                };

                // Initialize empty parent/child lists.
                node.Parents = new List<FoodNodeData>();
                node.Children = new List<FoodNodeData>();

                nodeMap[sNode.id] = node;
                Nodes.Add(node);
            }

            // 2. Resolve AutoTransform reference relationships.
            foreach (var sNode in serializedNodes)
            {
                if (string.IsNullOrEmpty(sNode.id) || !nodeMap.TryGetValue(sNode.id, out var node)) continue;

                if (!string.IsNullOrEmpty(sNode.autoTransformNodeId) && nodeMap.TryGetValue(sNode.autoTransformNodeId, out var targetNode))
                {
                    node.AutoTransform = targetNode;
                }
                else
                {
                    node.AutoTransform = null;
                }
            }

            // 3. Reconstruct connected parent/child edges.
            if (serializedEdges != null)
            {
                foreach (var edge in serializedEdges)
                {
                    if (string.IsNullOrEmpty(edge.parentId) || string.IsNullOrEmpty(edge.childId)) continue;

                    if (nodeMap.TryGetValue(edge.parentId, out var parentNode) &&
                        nodeMap.TryGetValue(edge.childId, out var childNode))
                    {
                        if (!parentNode.Children.Contains(childNode))
                        {
                            parentNode.Children.Add(childNode);
                        }

                        if (!childNode.Parents.Contains(parentNode))
                        {
                            childNode.Parents.Add(parentNode);
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class SerializedNode
    {
        public string id;
        public string name;
        public Sprite sprite;
        public int turnsToCreate;
        public int baseBuyInCost;
        public int autoTransformIn;
        public string autoTransformNodeId;
        public int autoDestroyIn;
        public bool enabled;
        public Vector2 position;
    }

    [Serializable]
    public class SerializedEdge
    {
        public string parentId;
        public string childId;
    }
}
