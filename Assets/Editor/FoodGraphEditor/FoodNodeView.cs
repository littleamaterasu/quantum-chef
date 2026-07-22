using System;
using System.Collections.Generic;
using Gameplay.Scripts.Data;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.FoodGraphEditor
{
    public class FoodNodeView : Node
    {
        public FoodNodeData NodeData { get; private set; }
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }

        public Action<string> OnRequestUndoRegistration;
        public Action OnNodeModified;
        public Action OnNameChanged;

        private DropdownField autoTransformDropdown;
        private List<FoodNodeView> autoTransformNodeChoices = new List<FoodNodeView>();

        // Block callbacks when programmatically setting values
        private bool isUpdatingUI = false;

        public FoodNodeView(FoodNodeData nodeData)
        {
            NodeData = nodeData;

            // Set Node ID if it's empty
            if (string.IsNullOrEmpty(NodeData.ID))
            {
                NodeData.ID = FoodGraphUtility.GenerateGuid();
            }

            title = string.IsNullOrEmpty(nodeData.Name) ? "Food Node" : nodeData.Name;

            // Apply custom styling for premium looks
            ApplyStyles();

            // Create input/output ports
            CreatePorts();

            // Build node contents/editor controls
            CreateControls();
        }

        private void ApplyStyles()
        {
            // Title container styling
            titleContainer.style.backgroundColor =
                new StyleColor(new Color(0.85f, 0.35f, 0.25f, 1f)); // Culinary warm orange
            titleContainer.style.borderTopLeftRadius = 6;
            titleContainer.style.borderTopRightRadius = 6;

            var titleLabel = titleContainer.Q<Label>();
            if (titleLabel != null)
            {
                titleLabel.style.color = new StyleColor(Color.white);
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.fontSize = 14;
            }

            // Main container styling (dark glassmorphism vibe)
            mainContainer.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 0.95f));
            mainContainer.style.borderTopWidth = 1;
            mainContainer.style.borderBottomWidth = 1;
            mainContainer.style.borderLeftWidth = 1;
            mainContainer.style.borderRightWidth = 1;
            mainContainer.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            mainContainer.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            mainContainer.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            mainContainer.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            mainContainer.style.borderTopLeftRadius = mainContainer.style.borderBottomLeftRadius =
                mainContainer.style.borderBottomRightRadius = mainContainer.style.borderTopRightRadius = 6;
        }

        private void CreatePorts()
        {
            // Input Port: "Parents" (incoming connections, parent -> child)
            InputPort = Port.Create<FoodEdge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi,
                typeof(FoodNodeData));
            InputPort.portName = "Parents";
            InputPort.portColor = new Color(0.4f, 0.7f, 1f); // Sky blue for inputs
            inputContainer.Add(InputPort);

            // Output Port: "Children" (outgoing connections, parent -> child)
            OutputPort = Port.Create<FoodEdge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi,
                typeof(FoodNodeData));
            OutputPort.portName = "Children";
            OutputPort.portColor = new Color(0.3f, 0.85f, 0.45f); // Green for outputs
            outputContainer.Add(OutputPort);
        }

        private void CreateControls()
        {
            var customContainer = new VisualElement();
            customContainer.style.paddingLeft = 8;
            customContainer.style.paddingRight = 8;
            customContainer.style.paddingTop = 8;
            customContainer.style.paddingBottom = 8;
            customContainer.style.width = 240; // Clean set width for uniformity

            // Read-only Node ID
            var idLabel = new Label($"ID: {NodeData.ID.Substring(0, Mathf.Min(NodeData.ID.Length, 8))}...")
            {
                tooltip = $"Full GUID: {NodeData.ID}"
            };
            idLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
            idLabel.style.fontSize = 10;
            idLabel.style.marginBottom = 4;
            customContainer.Add(idLabel);

            // Name Field
            var nameField = new TextField("Name") { value = NodeData.Name };
            nameField.RegisterValueChangedCallback(evt =>
            {
                if (isUpdatingUI) return;
                OnRequestUndoRegistration?.Invoke("Change Node Name");
                NodeData.Name = evt.newValue;
                title = string.IsNullOrEmpty(evt.newValue) ? "Food Node" : evt.newValue;
                OnNameChanged?.Invoke();
                OnNodeModified?.Invoke();
            });
            customContainer.Add(nameField);

            // Enabled Status
            var enabledToggle = new Toggle("Enabled") { value = NodeData.Enabled };
            enabledToggle.RegisterValueChangedCallback(evt =>
            {
                if (isUpdatingUI) return;
                OnRequestUndoRegistration?.Invoke("Toggle Node Enabled");
                NodeData.Enabled = evt.newValue;
                OnNodeModified?.Invoke();
            });
            customContainer.Add(enabledToggle);

            // Sprite selector
            var spriteField = new ObjectField("Sprite")
            {
                objectType = typeof(Sprite),
                value = NodeData.Sprite
            };

            // Sprite preview image container
            var spritePreview = new Image();
            spritePreview.style.width = 128;
            spritePreview.style.height = 128;
            spritePreview.style.marginTop = 4;
            spritePreview.style.marginBottom = 4;
            spritePreview.style.alignSelf = Align.Center;
            spritePreview.style.borderTopWidth = 1;
            spritePreview.style.borderBottomWidth = 1;
            spritePreview.style.borderLeftWidth = 1;
            spritePreview.style.borderRightWidth = 1;
            spritePreview.style.borderTopColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            spritePreview.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            spritePreview.style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            spritePreview.style.borderRightColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            spritePreview.style.borderBottomLeftRadius = 4;
            spritePreview.style.borderBottomRightRadius = 4;
            spritePreview.style.borderTopLeftRadius = 4;
            spritePreview.style.borderTopRightRadius = 4;

            Action<Sprite> updateSpritePreview = (Sprite s) =>
            {
                if (s != null)
                {
                    spritePreview.image = s.texture;
                    spritePreview.style.display = DisplayStyle.Flex;
                }
                else
                {
                    spritePreview.image = null;
                    spritePreview.style.display = DisplayStyle.None;
                }
            };

            // Set initial state
            updateSpritePreview(NodeData.Sprite);

            spriteField.RegisterValueChangedCallback(evt =>
            {
                if (isUpdatingUI) return;
                var newSprite = evt.newValue as Sprite;
                OnRequestUndoRegistration?.Invoke("Change Node Sprite");
                NodeData.Sprite = newSprite;
                updateSpritePreview(newSprite);
                OnNodeModified?.Invoke();
            });
            customContainer.Add(spriteField);
            customContainer.Add(spritePreview);

            // Turns to Create
            var turnsField = new IntegerField("Turns To Create") { value = NodeData.TurnsToCreate };
            turnsField.RegisterValueChangedCallback(evt =>
            {
                if (isUpdatingUI) return;
                OnRequestUndoRegistration?.Invoke("Change Turns To Create");
                NodeData.TurnsToCreate = evt.newValue;
                OnNodeModified?.Invoke();
            });
            customContainer.Add(turnsField);

            // Base Buy-In Cost
            var costField = new IntegerField("Base Buy-In Cost") { value = NodeData.BaseBuyInCost };
            costField.RegisterValueChangedCallback(evt =>
            {
                if (isUpdatingUI) return;
                OnRequestUndoRegistration?.Invoke("Change Buy-In Cost");
                NodeData.BaseBuyInCost = evt.newValue;
                OnNodeModified?.Invoke();
            });
            customContainer.Add(costField);

            // Auto Transform In
            var transformInField = new IntegerField("Auto Transform In") { value = NodeData.AutoTransformIn };
            transformInField.RegisterValueChangedCallback(evt =>
            {
                if (isUpdatingUI) return;
                OnRequestUndoRegistration?.Invoke("Change Auto Transform In");
                NodeData.AutoTransformIn = evt.newValue;
                OnNodeModified?.Invoke();
            });
            customContainer.Add(transformInField);

            // Auto Destroy In
            var destroyInField = new IntegerField("Auto Destroy In") { value = NodeData.AutoDestroyIn };
            destroyInField.RegisterValueChangedCallback(evt =>
            {
                if (isUpdatingUI) return;
                OnRequestUndoRegistration?.Invoke("Change Auto Destroy In");
                NodeData.AutoDestroyIn = evt.newValue;
                OnNodeModified?.Invoke();
            });
            customContainer.Add(destroyInField);

            // Auto Transform Target Dropdown
            autoTransformDropdown = new DropdownField("Auto Transform");
            autoTransformDropdown.RegisterValueChangedCallback(evt =>
            {
                if (isUpdatingUI) return;
                int idx = autoTransformDropdown.index;
                if (idx <= 0 || autoTransformNodeChoices == null || idx - 1 >= autoTransformNodeChoices.Count)
                {
                    if (NodeData.AutoTransform != null)
                    {
                        OnRequestUndoRegistration?.Invoke("Change Auto Transform Target");
                        NodeData.AutoTransform = null;
                        OnNodeModified?.Invoke();
                    }
                }
                else
                {
                    var targetNode = autoTransformNodeChoices[idx - 1].NodeData;
                    if (NodeData.AutoTransform != targetNode)
                    {
                        OnRequestUndoRegistration?.Invoke("Change Auto Transform Target");
                        NodeData.AutoTransform = targetNode;
                        OnNodeModified?.Invoke();
                    }
                }
            });
            customContainer.Add(autoTransformDropdown);

            extensionContainer.Add(customContainer);
            RefreshExpandedState();
        }

        public void UpdateAutoTransformChoices(List<FoodNodeView> allNodeViews)
        {
            isUpdatingUI = true;
            try
            {
                List<string> choices = new List<string> { "None" };
                autoTransformNodeChoices.Clear();

                foreach (var nv in allNodeViews)
                {
                    if (nv != this)
                    {
                        autoTransformNodeChoices.Add(nv);
                        string displayName = string.IsNullOrEmpty(nv.NodeData.Name) ? "Unnamed" : nv.NodeData.Name;
                        choices.Add(
                            $"{displayName} ({nv.NodeData.ID.Substring(0, Mathf.Min(nv.NodeData.ID.Length, 8))})");
                    }
                }

                autoTransformDropdown.choices = choices;

                // Sync the selected item
                if (NodeData.AutoTransform == null || string.IsNullOrEmpty(NodeData.AutoTransform.ID))
                {
                    autoTransformDropdown.index = 0;
                }
                else
                {
                    int foundIdx = autoTransformNodeChoices.FindIndex(n => n.NodeData.ID == NodeData.AutoTransform.ID);
                    if (foundIdx >= 0)
                    {
                        autoTransformDropdown.index = foundIdx + 1; // offset by 1 for "None"
                    }
                    else
                    {
                        autoTransformDropdown.index = 0;
                    }
                }
            }
            finally
            {
                isUpdatingUI = false;
            }
        }
    }
}