using System.Collections.Generic;
using System.Linq;
using Gameplay.Scripts.Data;
using Rect = UnityEngine.Rect;
using Vector2 = UnityEngine.Vector2;

namespace Editor.FoodGraphEditor
{
    public static class FoodGraphSerializer
    {
        /// <summary>
        /// Translates the visual node positions and connections in the GraphView to the serialized lists in FoodGraphAsset.
        /// </summary>
        public static void SaveToAsset(FoodGraphAsset asset, FoodGraphView view)
        {
            if (asset == null || view == null) return;

            asset.SerializedNodes.Clear();
            asset.SerializedEdges.Clear();

            var nodeViews = view.nodes.ToList().Cast<FoodNodeView>().ToList();
            var edgeViews = view.edges.ToList().Cast<FoodEdge>().ToList();

            // 1. Serialize all node data and positions
            foreach (var nv in nodeViews)
            {
                var rect = nv.GetPosition();
                var sNode = new SerializedNode
                {
                    id = nv.NodeData.ID,
                    name = nv.NodeData.Name,
                    sprite = nv.NodeData.Sprite,
                    turnsToCreate = nv.NodeData.TurnsToCreate,
                    baseBuyInCost = nv.NodeData.BaseBuyInCost,
                    autoTransformIn = nv.NodeData.AutoTransformIn,
                    autoDestroyIn = nv.NodeData.AutoDestroyIn,
                    enabled = nv.NodeData.Enabled,
                    position = new Vector2(rect.x, rect.y),
                    autoTransformNodeId = nv.NodeData.AutoTransform != null ? nv.NodeData.AutoTransform.ID : string.Empty,
                    bundleId = nv.NodeData.BundleId
                };

                asset.SerializedNodes.Add(sNode);
            }

            // 2. Serialize all parent-child edge connections
            foreach (var ev in edgeViews)
            {
                if (ev.output?.node is FoodNodeView parentNode && ev.input?.node is FoodNodeView childNode)
                {
                    var sEdge = new SerializedEdge
                    {
                        parentId = parentNode.NodeData.ID,
                        childId = childNode.NodeData.ID
                    };
                    asset.SerializedEdges.Add(sEdge);
                }
            }
        }

        /// <summary>
        /// Reads the serialization lists in FoodGraphAsset, builds visual nodes/edges, and places them in FoodGraphView.
        /// </summary>
        public static void LoadFromAsset(FoodGraphAsset asset, FoodGraphView view)
        {
            if (asset == null || view == null) return;

            // Clear visual elements first
            view.ClearGraphElements();

            // Reconstruct the reference graph within the asset
            asset.ReconstructGraph();

            // 1. Recreate node views mapping back to the reconstructed runtime instances
            var nodeMap = new Dictionary<string, FoodNodeView>();
            foreach (var sNode in asset.SerializedNodes)
            {
                var runtimeNode = asset.Nodes.Find(n => n.ID == sNode.id);
                if (runtimeNode == null) continue;

                var nodeView = new FoodNodeView(runtimeNode);
                nodeView.SetPosition(new Rect(sNode.position.x, sNode.position.y, 0, 0));
                
                view.AddElement(nodeView);
                nodeMap[sNode.id] = nodeView;
                
                // Subscribe to callbacks
                view.BindNodeViewCallbacks(nodeView);
            }

            // 2. Recreate edge views
            foreach (var sEdge in asset.SerializedEdges)
            {
                if (nodeMap.TryGetValue(sEdge.parentId, out var parentView) &&
                    nodeMap.TryGetValue(sEdge.childId, out var childView))
                {
                    var edge = new FoodEdge
                    {
                        output = parentView.OutputPort,
                        input = childView.InputPort
                    };

                    edge.input.Connect(edge);
                    edge.output.Connect(edge);
                    view.AddElement(edge);
                }
            }

            // 3. Re-evaluate dropdown choices for AutoTransform fields across all nodes
            view.RefreshAutoTransformDropdowns();
        }
    }
}
