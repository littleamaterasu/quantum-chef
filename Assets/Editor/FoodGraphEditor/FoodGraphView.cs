using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Scripts.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.FoodGraphEditor
{
    public class FoodGraphView : GraphView
    {
        private readonly FoodGraphEditorWindow window;
        private Dictionary<string, Vector2> preDragPositions = new Dictionary<string, Vector2>(StringComparer.Ordinal);

        public FoodGraphView(FoodGraphEditorWindow window)
        {
            this.window = window;

            // Setup Zoom
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Add manipulators for panning and dragging
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            // Add grid background
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            // Set handlers
            graphViewChanged = OnGraphViewChanged;

            // Copy-Paste handlers
            serializeGraphElements = SerializeElements;
            unserializeAndPaste = UnserializeAndPaste;
            canPasteSerializedData = CanPaste;

            // Register event to record positions before drag starts
            RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0) // Left click
                {
                    RecordNodePositions();
                }
            });
        }

        public void BindNodeViewCallbacks(FoodNodeView nodeView)
        {
            nodeView.OnRequestUndoRegistration = actionName => window.RegisterUndo(actionName);
            nodeView.OnNodeModified = () => window.OnGraphModified();
            nodeView.OnNameChanged = () => RefreshAutoTransformDropdowns();
        }

        private void RecordNodePositions()
        {
            preDragPositions.Clear();
            var nodeViews = nodes.ToList().Cast<FoodNodeView>();
            foreach (var nv in nodeViews)
            {
                if (nv.NodeData != null && !string.IsNullOrEmpty(nv.NodeData.ID))
                {
                    var rect = nv.GetPosition();
                    preDragPositions[nv.NodeData.ID] = new Vector2(rect.x, rect.y);
                }
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var curr = evt.target as VisualElement;
            while (curr != null)
            {
                if (curr is Node || curr is Edge)
                {
                    return;
                }
                curr = curr.parent;
            }

            var mousePos = evt.mousePosition;
            var localPos = contentViewContainer.WorldToLocal(mousePos);
            evt.menu.AppendAction("Create Node", action => CreateNode("New Node", localPos));
        }

        public void CreateNode(string nodeName, Vector2 position)
        {
            window.RegisterUndo("Create Node");

            var nodeData = new FoodNodeData
            {
                ID = FoodGraphUtility.GenerateGuid(),
                Name = nodeName,
                Enabled = true,
                Parents = new List<FoodNodeData>(),
                Children = new List<FoodNodeData>()
            };

            var nodeView = new FoodNodeView(nodeData);
            nodeView.SetPosition(new Rect(position.x, position.y, 0, 0));
            
            AddElement(nodeView);
            BindNodeViewCallbacks(nodeView);

            RefreshAutoTransformDropdowns();
            window.OnGraphModified();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    // Check if a connection already exists to prevent duplicates
                    bool duplicate = false;
                    foreach (var edge in edges)
                    {
                        if ((edge.input == startPort && edge.output == port) || 
                            (edge.input == port && edge.output == startPort))
                        {
                            duplicate = true;
                            break;
                        }
                    }

                    if (!duplicate)
                    {
                        compatiblePorts.Add(port);
                    }
                }
            });
            return compatiblePorts;
        }

        public override EventPropagation DeleteSelection()
        {
            window.RegisterUndo("Delete Selection");

            return base.DeleteSelection();
        }

        public void ClearGraphElements()
        {
            var tempCallback = graphViewChanged;
            graphViewChanged = null;
            try
            {
                DeleteElements(edges.ToList());
                DeleteElements(nodes.ToList());
            }
            finally
            {
                graphViewChanged = tempCallback;
            }
        }

        public void RefreshAutoTransformDropdowns()
        {
            var nodeViews = nodes.ToList().Cast<FoodNodeView>().ToList();
            foreach (var nodeView in nodeViews)
            {
                nodeView.UpdateAutoTransformChoices(nodeViews);
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            bool modified = false;

            // 1. Handle elements to remove (Edges / Nodes)
            if (change.elementsToRemove != null && change.elementsToRemove.Count > 0)
            {
                var edgesToRemove = change.elementsToRemove.OfType<FoodEdge>().ToList();
                var nodesToRemove = change.elementsToRemove.OfType<FoodNodeView>().ToList();

                if (edgesToRemove.Count > 0 || nodesToRemove.Count > 0)
                {
                    // Remove parent-child references for removed edges
                    foreach (var edge in edgesToRemove)
                    {
                        if (edge.output?.node is FoodNodeView parentNode && edge.input?.node is FoodNodeView childNode)
                        {
                            parentNode.NodeData.Children.Remove(childNode.NodeData);
                            childNode.NodeData.Parents.Remove(parentNode.NodeData);
                        }
                    }

                    // Clean up any deleted node IDs in autoTransform fields of remaining nodes
                    if (nodesToRemove.Count > 0)
                    {
                        var deletedIds = new HashSet<string>(nodesToRemove.Select(n => n.NodeData.ID));
                        var remainingNodeViews = nodes.ToList().Cast<FoodNodeView>()
                            .Where(n => !deletedIds.Contains(n.NodeData.ID)).ToList();

                        foreach (var nv in remainingNodeViews)
                        {
                            if (nv.NodeData.AutoTransform != null && deletedIds.Contains(nv.NodeData.AutoTransform.ID))
                            {
                                nv.NodeData.AutoTransform = null;
                            }
                        }
                    }

                    modified = true;
                }
            }

            // 2. Handle edges to create
            if (change.edgesToCreate != null && change.edgesToCreate.Count > 0)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    if (edge.output?.node is FoodNodeView parentNode && edge.input?.node is FoodNodeView childNode)
                    {
                        if (!parentNode.NodeData.Children.Contains(childNode.NodeData))
                        {
                            parentNode.NodeData.Children.Add(childNode.NodeData);
                        }
                        if (!childNode.NodeData.Parents.Contains(parentNode.NodeData))
                        {
                            childNode.NodeData.Parents.Add(parentNode.NodeData);
                        }
                    }
                }
                modified = true;
            }

            // 3. Handle moved elements (Node positions changed)
            if (change.movedElements != null && change.movedElements.Count > 0)
            {
                var movedNodes = change.movedElements.OfType<FoodNodeView>().ToList();
                if (movedNodes.Count > 0)
                {
                    var currentPositions = new Dictionary<string, Vector2>(StringComparer.Ordinal);
                    foreach (var nv in movedNodes)
                    {
                        var rect = nv.GetPosition();
                        currentPositions[nv.NodeData.ID] = new Vector2(rect.x, rect.y);

                        // Reset to pre-drag position to record the undo state correctly
                        if (preDragPositions.TryGetValue(nv.NodeData.ID, out var oldPos))
                        {
                            nv.SetPosition(new Rect(oldPos.x, oldPos.y, rect.width, rect.height));
                        }
                    }

                    // Capture the pre-drag state
                    window.RegisterUndo("Move Nodes");

                    // Re-apply final drag position
                    foreach (var nv in movedNodes)
                    {
                        if (currentPositions.TryGetValue(nv.NodeData.ID, out var newPos))
                        {
                            var rect = nv.GetPosition();
                            nv.SetPosition(new Rect(newPos.x, newPos.y, rect.width, rect.height));
                        }
                    }
                    modified = true;
                }
            }

            if (modified)
            {
                RefreshAutoTransformDropdowns();
                window.OnGraphModified();
            }

            return change;
        }

        // --- Copy Paste Implementation ---

        [Serializable]
        private class CopyPasteData
        {
            public List<SerializedNode> nodes = new List<SerializedNode>();
            public List<SerializedEdge> edges = new List<SerializedEdge>();
        }

        private string SerializeElements(IEnumerable<GraphElement> elements)
        {
            var copiedNodes = new List<SerializedNode>();
            var copiedEdges = new List<SerializedEdge>();

            var elementsList = elements.ToList();
            var nodeViews = elementsList.OfType<FoodNodeView>().ToList();
            var edgeViews = elementsList.OfType<FoodEdge>().ToList();

            foreach (var nv in nodeViews)
            {
                var rect = nv.GetPosition();
                copiedNodes.Add(new SerializedNode
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
                    autoTransformNodeId = nv.NodeData.AutoTransform != null ? nv.NodeData.AutoTransform.ID : string.Empty
                });
            }

            var nodeIds = new HashSet<string>(nodeViews.Select(n => n.NodeData.ID));
            foreach (var ev in edgeViews)
            {
                if (ev.output?.node is FoodNodeView parentNode && ev.input?.node is FoodNodeView childNode)
                {
                    // Copy edge only if both parent and child are selected and copied
                    if (nodeIds.Contains(parentNode.NodeData.ID) && nodeIds.Contains(childNode.NodeData.ID))
                    {
                        copiedEdges.Add(new SerializedEdge
                        {
                            parentId = parentNode.NodeData.ID,
                            childId = childNode.NodeData.ID
                        });
                    }
                }
            }

            var copyData = new CopyPasteData { nodes = copiedNodes, edges = copiedEdges };
            return JsonUtility.ToJson(copyData);
        }

        private bool CanPaste(string data)
        {
            if (string.IsNullOrEmpty(data)) return false;
            try
            {
                var copyData = JsonUtility.FromJson<CopyPasteData>(data);
                return copyData != null && copyData.nodes != null && copyData.nodes.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        private void UnserializeAndPaste(string operationName, string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            try
            {
                var copyData = JsonUtility.FromJson<CopyPasteData>(data);
                if (copyData == null || copyData.nodes == null || copyData.nodes.Count == 0) return;

                window.RegisterUndo("Paste Selection");

                var idMapping = new Dictionary<string, string>(StringComparer.Ordinal);
                var pastedNodeViews = new List<FoodNodeView>();

                // 1. Create duplicate FoodNodeData and FoodNodeView instances
                foreach (var sNode in copyData.nodes)
                {
                    var newId = FoodGraphUtility.GenerateGuid();
                    idMapping[sNode.id] = newId;

                    var runtimeNode = new FoodNodeData
                    {
                        ID = newId,
                        Name = sNode.name + " (Copy)",
                        Sprite = sNode.sprite,
                        TurnsToCreate = sNode.turnsToCreate,
                        BaseBuyInCost = sNode.baseBuyInCost,
                        AutoTransformIn = sNode.autoTransformIn,
                        AutoDestroyIn = sNode.autoDestroyIn,
                        Enabled = sNode.enabled
                    };

                    var nodeView = new FoodNodeView(runtimeNode);
                    nodeView.SetPosition(new Rect(sNode.position.x + 40, sNode.position.y + 40, 0, 0)); // Offset

                    AddElement(nodeView);
                    pastedNodeViews.Add(nodeView);
                    
                    BindNodeViewCallbacks(nodeView);
                }

                // 2. Map AutoTransform references
                for (int i = 0; i < copyData.nodes.Count; i++)
                {
                    var oldNode = copyData.nodes[i];
                    var newNodeView = pastedNodeViews[i];

                    if (!string.IsNullOrEmpty(oldNode.autoTransformNodeId))
                    {
                        if (idMapping.TryGetValue(oldNode.autoTransformNodeId, out var mappedId))
                        {
                            var targetView = pastedNodeViews.Find(n => n.NodeData.ID == mappedId);
                            newNodeView.NodeData.AutoTransform = targetView?.NodeData;
                        }
                        else
                        {
                            // Try to map to an existing node in the graph if it was not pasted
                            var existingNodeView = nodes.ToList().Cast<FoodNodeView>()
                                .FirstOrDefault(n => n.NodeData.ID == oldNode.autoTransformNodeId);
                            newNodeView.NodeData.AutoTransform = existingNodeView?.NodeData;
                        }
                    }
                }

                // 3. Create visual and data edges for pasted connections
                var activeNodesMap = nodes.ToList().Cast<FoodNodeView>().ToDictionary(n => n.NodeData.ID);
                foreach (var sEdge in copyData.edges)
                {
                    string newParentId = idMapping.TryGetValue(sEdge.parentId, out var pId) ? pId : null;
                    string newChildId = idMapping.TryGetValue(sEdge.childId, out var cId) ? cId : null;

                    if (newParentId != null && newChildId != null)
                    {
                        if (activeNodesMap.TryGetValue(newParentId, out var parentView) &&
                            activeNodesMap.TryGetValue(newChildId, out var childView))
                        {
                            var edge = new FoodEdge
                            {
                                output = parentView.OutputPort,
                                input = childView.InputPort
                            };

                            edge.input.Connect(edge);
                            edge.output.Connect(edge);
                            AddElement(edge);

                            // Reconstruct data lists
                            if (!parentView.NodeData.Children.Contains(childView.NodeData))
                            {
                                parentView.NodeData.Children.Add(childView.NodeData);
                            }
                            if (!childView.NodeData.Parents.Contains(parentView.NodeData))
                            {
                                childView.NodeData.Parents.Add(parentView.NodeData);
                            }
                        }
                    }
                }

                // Change selection to the pasted objects
                ClearSelection();
                foreach (var nv in pastedNodeViews)
                {
                    AddToSelection(nv);
                }

                RefreshAutoTransformDropdowns();
                window.OnGraphModified();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error pasting graph elements: {ex}");
            }
        }
    }
}
