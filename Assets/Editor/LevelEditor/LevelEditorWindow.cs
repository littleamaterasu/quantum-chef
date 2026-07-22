using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Scripts.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.LevelEditor
{
    public class LevelEditorWindow : EditorWindow
    {
        private LevelDataAsset currentAsset;
        private FoodGraphAsset foodGraphAsset;
        private bool isDirty = false;

        private List<FoodNodeData> availableFoodNodes = new List<FoodNodeData>();

        private Dictionary<string, FoodNodeData> foodNodeMap =
            new Dictionary<string, FoodNodeData>(StringComparer.Ordinal);

        private VisualElement mainScrollView;
        private VisualElement initialFoodsContainer;
        private VisualElement timelineContainer;
        private ObjectField assetObjectField;
        private ObjectField graphObjectField;

        [MenuItem("Tools/Level Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<LevelEditorWindow>();
            window.titleContent = new GUIContent("Level Editor");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        public static void OpenWithAsset(LevelDataAsset asset)
        {
            var window = GetWindow<LevelEditorWindow>();
            window.titleContent = new GUIContent("Level Editor");
            window.minSize = new Vector2(800, 600);
            window.LoadAsset(asset);
            window.Show();
        }

        private void OnEnable()
        {
            saveChangesMessage = "The Level Data has unsaved changes. Do you want to save them?";
            ConstructVisualTree();

            if (currentAsset == null)
            {
                NewLevel();
            }
        }

        private void ConstructVisualTree()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));

            // --- 1. Top Toolbar ---
            var toolbar = new Toolbar();
            toolbar.style.height = 32;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.paddingLeft = 6;
            toolbar.style.paddingRight = 6;

            var newBtn = new Button(NewLevelAction) { text = "New" };
            newBtn.style.height = 24;
            toolbar.Add(newBtn);

            var openBtn = new Button(OpenLevelAction) { text = "Open" };
            openBtn.style.height = 24;
            toolbar.Add(openBtn);

            var saveBtn = new Button(() => SaveLevel()) { text = "Save" };
            saveBtn.style.height = 24;
            toolbar.Add(saveBtn);

            var saveAsBtn = new Button(() => SaveAsLevel()) { text = "Save As" };
            saveAsBtn.style.height = 24;
            toolbar.Add(saveAsBtn);

            toolbar.Add(new ToolbarSpacer { flex = true });

            // Food Graph Asset Selector
            var graphLabel = new Label("Food Graph: ");
            graphLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            graphLabel.style.marginLeft = 8;
            toolbar.Add(graphLabel);

            graphObjectField = new ObjectField
            {
                objectType = typeof(FoodGraphAsset),
                value = foodGraphAsset
            };
            graphObjectField.style.width = 180;
            graphObjectField.RegisterValueChangedCallback(evt =>
            {
                foodGraphAsset = evt.newValue as FoodGraphAsset;
                if (currentAsset != null)
                {
                    currentAsset.foodGraphAsset = foodGraphAsset;
                    MarkDirty();
                }

                RefreshAvailableFoodNodes();
                RebuildUI();
            });
            toolbar.Add(graphObjectField);

            // Level Asset Selector
            var assetLabel = new Label(" Level Asset: ");
            assetLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            assetLabel.style.marginLeft = 8;
            toolbar.Add(assetLabel);

            assetObjectField = new ObjectField
            {
                objectType = typeof(LevelDataAsset),
                value = currentAsset
            };
            assetObjectField.style.width = 200;
            assetObjectField.RegisterValueChangedCallback(evt =>
            {
                var newAsset = evt.newValue as LevelDataAsset;
                if (newAsset != null && newAsset != currentAsset)
                {
                    LoadAsset(newAsset);
                }
            });
            toolbar.Add(assetObjectField);

            rootVisualElement.Add(toolbar);

            // --- 2. Main Scrollable Container ---
            var mainScroll = new ScrollView(ScrollViewMode.Vertical);
            mainScroll.style.flexGrow = 1;
            mainScroll.style.paddingLeft = 16;
            mainScroll.style.paddingRight = 16;
            mainScroll.style.paddingTop = 16;
            mainScroll.style.paddingBottom = 24;

            // --- SECTION 1: Initial Player Foods ---
            var section1 = CreateSectionBox("1. Initial Player Foods (Nguyên liệu ban đầu người chơi có)");

            initialFoodsContainer = new VisualElement();
            initialFoodsContainer.style.flexDirection = FlexDirection.Row;
            initialFoodsContainer.style.flexWrap = Wrap.Wrap;
            initialFoodsContainer.style.marginTop = 8;
            initialFoodsContainer.style.marginBottom = 12;

            section1.Add(initialFoodsContainer);

            var addInitialFoodBtn = new Button(ShowAddInitialFoodMenu) { text = "+ Add Initial Food Node" };
            addInitialFoodBtn.style.height = 28;
            addInitialFoodBtn.style.width = 180;
            addInitialFoodBtn.style.backgroundColor = new StyleColor(new Color(0.22f, 0.45f, 0.22f));
            addInitialFoodBtn.style.color = new StyleColor(Color.white);
            addInitialFoodBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            section1.Add(addInitialFoodBtn);

            mainScroll.Add(section1);

            // Spacer
            var spacer = new VisualElement();
            spacer.style.height = 20;
            mainScroll.Add(spacer);

            // --- SECTION 2: 24 Turns Customer Timeline ---
            var section2 = CreateSectionBox("2. Customer Timeline (24 Turns / 24 Giờ)");

            timelineContainer = new VisualElement();
            timelineContainer.style.marginTop = 8;
            section2.Add(timelineContainer);

            mainScroll.Add(section2);

            rootVisualElement.Add(mainScroll);

            RefreshAvailableFoodNodes();
            RebuildUI();
        }

        private VisualElement CreateSectionBox(string title)
        {
            var box = new VisualElement();
            box.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            box.style.borderTopLeftRadius = 6;
            box.style.borderTopRightRadius = 6;
            box.style.borderBottomLeftRadius = 6;
            box.style.borderBottomRightRadius = 6;
            box.style.borderTopWidth = 1;
            box.style.borderBottomWidth = 1;
            box.style.borderLeftWidth = 1;
            box.style.borderRightWidth = 1;
            box.style.borderTopColor = new StyleColor(new Color(0.35f, 0.35f, 0.35f));
            box.style.borderBottomColor = new StyleColor(new Color(0.35f, 0.35f, 0.35f));
            box.style.borderLeftColor = new StyleColor(new Color(0.35f, 0.35f, 0.35f));
            box.style.borderRightColor = new StyleColor(new Color(0.35f, 0.35f, 0.35f));
            box.style.paddingLeft = 14;
            box.style.paddingRight = 14;
            box.style.paddingTop = 12;
            box.style.paddingBottom = 14;

            var titleLabel = new Label(title);
            titleLabel.style.fontSize = 15;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.color = new StyleColor(new Color(0.9f, 0.9f, 0.9f));
            titleLabel.style.borderBottomWidth = 1;
            titleLabel.style.borderBottomColor = new StyleColor(new Color(0.35f, 0.35f, 0.35f));
            titleLabel.style.paddingBottom = 6;
            box.Add(titleLabel);

            return box;
        }

        // --- Data Management & Node Resolution ---

        private void RefreshAvailableFoodNodes()
        {
            availableFoodNodes.Clear();
            foodNodeMap.Clear();

            // Try from assigned foodGraphAsset
            if (foodGraphAsset == null && currentAsset != null && currentAsset.foodGraphAsset != null)
            {
                foodGraphAsset = currentAsset.foodGraphAsset;
                if (graphObjectField != null) graphObjectField.value = foodGraphAsset;
            }

            // Auto-find FoodGraphAsset in project if still null
            if (foodGraphAsset == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:FoodGraphAsset");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    foodGraphAsset = AssetDatabase.LoadAssetAtPath<FoodGraphAsset>(path);
                    if (graphObjectField != null) graphObjectField.value = foodGraphAsset;
                }
            }

            if (foodGraphAsset == null) return;
            if (foodGraphAsset.Nodes == null || foodGraphAsset.Nodes.Count == 0)
            {
                foodGraphAsset.ReconstructGraph();
            }

            foreach (var node in (foodGraphAsset.Nodes ?? new List<FoodNodeData>()).Where(node => node != null &&
                         !string.IsNullOrEmpty(node.ID) && node.Children != null &&
                         node.Children.Count != 0))
            {
                availableFoodNodes.Add(node);
                foodNodeMap[node.ID] = node;
            }
        }

        private void RebuildUI()
        {
            if (assetObjectField != null) assetObjectField.value = currentAsset;
            if (graphObjectField != null) graphObjectField.value = foodGraphAsset;

            RebuildInitialFoodsSection();
            RebuildTimelineSection();
        }

        // --- SECTION 1: Initial Foods ---

        private void RebuildInitialFoodsSection()
        {
            initialFoodsContainer.Clear();

            if (currentAsset == null || currentAsset.initialFoodNodeIds == null ||
                currentAsset.initialFoodNodeIds.Count == 0)
            {
                var emptyLabel =
                    new Label("No initial foods added yet. Click '+ Add Initial Food Node' below to add foods.");
                emptyLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
                emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                emptyLabel.style.marginTop = 6;
                emptyLabel.style.marginBottom = 6;
                initialFoodsContainer.Add(emptyLabel);
                return;
            }

            for (int i = 0; i < currentAsset.initialFoodNodeIds.Count; i++)
            {
                int index = i;
                string nodeId = currentAsset.initialFoodNodeIds[i];

                var card = new VisualElement();
                card.style.flexDirection = FlexDirection.Row;
                card.style.alignItems = Align.Center;
                card.style.backgroundColor = new StyleColor(new Color(0.28f, 0.28f, 0.28f));
                card.style.borderTopLeftRadius = 4;
                card.style.borderTopRightRadius = 4;
                card.style.borderBottomLeftRadius = 4;
                card.style.borderBottomRightRadius = 4;
                card.style.paddingLeft = 8;
                card.style.paddingRight = 6;
                card.style.paddingTop = 4;
                card.style.paddingBottom = 4;
                card.style.marginRight = 8;
                card.style.marginBottom = 8;

                // Food Icon
                var icon = new Image();
                icon.style.width = 64;
                icon.style.height = 64;
                icon.style.marginRight = 6;

                string nodeName = "Unknown Node";
                if (!string.IsNullOrEmpty(nodeId) && foodNodeMap.TryGetValue(nodeId, out var nodeData))
                {
                    nodeName = string.IsNullOrEmpty(nodeData.Name) ? nodeId : nodeData.Name;
                    if (nodeData.Sprite != null)
                    {
                        icon.image = nodeData.Sprite.texture;
                    }
                }

                card.Add(icon);

                var nameLabel = new Label(nodeName);
                nameLabel.style.fontSize = 12;
                nameLabel.style.color = new StyleColor(Color.white);
                nameLabel.style.marginRight = 8;
                card.Add(nameLabel);

                // [X] Delete Button
                var deleteBtn = new Button(() => RemoveInitialFood(index)) { text = "✕" };
                deleteBtn.style.height = 20;
                deleteBtn.style.width = 20;
                deleteBtn.style.paddingLeft = 0;
                deleteBtn.style.paddingRight = 0;
                deleteBtn.style.paddingTop = 0;
                deleteBtn.style.paddingBottom = 0;
                deleteBtn.style.backgroundColor = new StyleColor(new Color(0.7f, 0.2f, 0.2f));
                deleteBtn.style.color = new StyleColor(Color.white);
                deleteBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
                card.Add(deleteBtn);

                initialFoodsContainer.Add(card);
            }
        }

        private void ShowAddInitialFoodMenu()
        {
            if (availableFoodNodes.Count == 0)
            {
                EditorUtility.DisplayDialog("Warning",
                    "No FoodNodeData available! Please assign a valid FoodGraphAsset first.", "OK");
                return;
            }

            var menu = new GenericMenu();
            foreach (var node in availableFoodNodes)
            {
                string displayName = string.IsNullOrEmpty(node.Name) ? node.ID : node.Name;
                string nodeId = node.ID;
                menu.AddItem(new GUIContent(displayName), false, () => AddInitialFood(nodeId));
            }

            menu.ShowAsContext();
        }

        private void AddInitialFood(string nodeId)
        {
            if (currentAsset == null) return;
            RegisterUndo("Add Initial Food");
            currentAsset.initialFoodNodeIds.Add(nodeId);
            MarkDirty();
            RebuildInitialFoodsSection();
        }

        private void RemoveInitialFood(int index)
        {
            if (currentAsset == null || index < 0 || index >= currentAsset.initialFoodNodeIds.Count) return;
            RegisterUndo("Remove Initial Food");
            currentAsset.initialFoodNodeIds.RemoveAt(index);
            MarkDirty();
            RebuildInitialFoodsSection();
        }

        // --- SECTION 2: 24 Turns Customer Timeline ---

        private void RebuildTimelineSection()
        {
            timelineContainer.Clear();

            // Render 24 turn slots (0 to 23)
            for (int turn = 0; turn < 24; turn++)
            {
                int currentTurn = turn;

                // Gather customers for this turn
                List<SerializedCustomerData> turnCustomers = new List<SerializedCustomerData>();
                if (currentAsset != null && currentAsset.customers != null)
                {
                    foreach (var c in currentAsset.customers)
                    {
                        if (c != null && c.spawnTurn == currentTurn)
                        {
                            turnCustomers.Add(c);
                        }
                    }
                }

                var turnBox = new VisualElement();
                turnBox.style.backgroundColor = new StyleColor(new Color(0.24f, 0.24f, 0.24f));
                turnBox.style.borderTopLeftRadius = 4;
                turnBox.style.borderTopRightRadius = 4;
                turnBox.style.borderBottomLeftRadius = 4;
                turnBox.style.borderBottomRightRadius = 4;
                turnBox.style.paddingLeft = 10;
                turnBox.style.paddingRight = 10;
                turnBox.style.paddingTop = 8;
                turnBox.style.paddingBottom = 8;
                turnBox.style.marginBottom = 10;

                // Header bar for Turn
                var headerRow = new VisualElement();
                headerRow.style.flexDirection = FlexDirection.Row;
                headerRow.style.alignItems = Align.Center;

                var turnTitle = new Label($"Turn {currentTurn} (Hour {currentTurn + 1})");
                turnTitle.style.fontSize = 13;
                turnTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
                turnTitle.style.color = new StyleColor(new Color(1f, 0.85f, 0.4f));
                headerRow.Add(turnTitle);

                var badge = new Label($"  [{turnCustomers.Count} Customer{(turnCustomers.Count == 1 ? "" : "s")}]");
                badge.style.fontSize = 12;
                badge.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
                badge.style.marginRight = 12;
                headerRow.Add(badge);

                var addCustomerBtn = new Button(() => AddCustomerToTurn(currentTurn)) { text = "+ Add Customer" };
                addCustomerBtn.style.height = 22;
                addCustomerBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.45f, 0.6f));
                addCustomerBtn.style.color = new StyleColor(Color.white);
                headerRow.Add(addCustomerBtn);

                turnBox.Add(headerRow);

                // Customer list for this turn
                var customerListContainer = new VisualElement();
                customerListContainer.style.marginTop = 6;

                if (turnCustomers.Count == 0)
                {
                    var noCust = new Label("No customers spawning at this turn.");
                    noCust.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f));
                    noCust.style.unityFontStyleAndWeight = FontStyle.Italic;
                    noCust.style.fontSize = 11;
                    noCust.style.marginLeft = 4;
                    customerListContainer.Add(noCust);
                }
                else
                {
                    foreach (var customer in turnCustomers)
                    {
                        var custCard = BuildCustomerCard(customer);
                        customerListContainer.Add(custCard);
                    }
                }

                turnBox.Add(customerListContainer);
                timelineContainer.Add(turnBox);
            }
        }

        private VisualElement BuildCustomerCard(SerializedCustomerData customer)
        {
            var card = new VisualElement();
            card.style.backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f));
            card.style.borderTopLeftRadius = 4;
            card.style.borderTopRightRadius = 4;
            card.style.borderBottomLeftRadius = 4;
            card.style.borderBottomRightRadius = 4;
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = new StyleColor(new Color(0.32f, 0.32f, 0.32f));
            card.style.borderBottomColor = new StyleColor(new Color(0.32f, 0.32f, 0.32f));
            card.style.borderLeftColor = new StyleColor(new Color(0.32f, 0.32f, 0.32f));
            card.style.borderRightColor = new StyleColor(new Color(0.32f, 0.32f, 0.32f));
            card.style.paddingLeft = 10;
            card.style.paddingRight = 10;
            card.style.paddingTop = 8;
            card.style.paddingBottom = 8;
            card.style.marginTop = 6;
            card.style.marginBottom = 6;

            // Row 1: Header (Title + [X] Delete Customer)
            var topRow = new VisualElement();
            topRow.style.flexDirection = FlexDirection.Row;
            topRow.style.alignItems = Align.Center;

            string shortId = string.IsNullOrEmpty(customer.id)
                ? "???"
                : (customer.id.Length > 6 ? customer.id.Substring(0, 6) : customer.id);
            var custHeader = new Label($"Customer #{shortId}");
            custHeader.style.fontSize = 12;
            custHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            custHeader.style.color = new StyleColor(new Color(0.9f, 0.9f, 0.9f));
            topRow.Add(custHeader);

            var topSpacer = new VisualElement();
            topSpacer.style.flexGrow = 1;
            topRow.Add(topSpacer);

            // [X] Delete Customer Button
            var deleteCustBtn = new Button(() => RemoveCustomer(customer)) { text = "✕ Delete Customer" };
            deleteCustBtn.style.height = 20;
            deleteCustBtn.style.backgroundColor = new StyleColor(new Color(0.7f, 0.2f, 0.2f));
            deleteCustBtn.style.color = new StyleColor(Color.white);
            deleteCustBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            topRow.Add(deleteCustBtn);

            card.Add(topRow);

            // Row 2: Customer Sprite Field
            var spriteRow = new VisualElement();
            spriteRow.style.flexDirection = FlexDirection.Row;
            spriteRow.style.alignItems = Align.Center;
            spriteRow.style.marginTop = 6;

            var spriteLabel = new Label("Sprite: ");
            spriteLabel.style.width = 60;
            spriteRow.Add(spriteLabel);

            var spriteField = new ObjectField
            {
                objectType = typeof(Sprite),
                value = customer.sprite
            };
            spriteField.style.width = 160;
            spriteField.RegisterValueChangedCallback(evt =>
            {
                RegisterUndo("Change Customer Sprite");
                customer.sprite = evt.newValue as Sprite;
                MarkDirty();
                RebuildTimelineSection();
            });
            spriteRow.Add(spriteField);

            // Sprite preview
            var spritePreview = new Image();
            spritePreview.style.width = 64;
            spritePreview.style.height = 64;
            spritePreview.style.marginLeft = 8;
            if (customer.sprite != null)
            {
                spritePreview.image = customer.sprite.texture;
            }

            spriteRow.Add(spritePreview);

            card.Add(spriteRow);

            // Row 3: Required Foods
            var reqRow = new VisualElement();
            reqRow.style.flexDirection = FlexDirection.Column;
            reqRow.style.marginTop = 8;

            var reqHeaderRow = new VisualElement();
            reqHeaderRow.style.flexDirection = FlexDirection.Row;
            reqHeaderRow.style.alignItems = Align.Center;

            var reqLabel = new Label("Required Foods (Order):");
            reqLabel.style.fontSize = 11;
            reqLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            reqLabel.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            reqHeaderRow.Add(reqLabel);

            var addReqBtn = new Button(() => ShowAddRequiredFoodMenu(customer)) { text = "+ Add Food" };
            addReqBtn.style.height = 20;
            addReqBtn.style.marginLeft = 10;
            addReqBtn.style.backgroundColor = new StyleColor(new Color(0.22f, 0.45f, 0.22f));
            addReqBtn.style.color = new StyleColor(Color.white);
            reqHeaderRow.Add(addReqBtn);

            reqRow.Add(reqHeaderRow);

            // Chips container for required foods
            var chipsContainer = new VisualElement();
            chipsContainer.style.flexDirection = FlexDirection.Row;
            chipsContainer.style.flexWrap = Wrap.Wrap;
            chipsContainer.style.marginTop = 4;

            if (customer.requireFoodNodeIds == null || customer.requireFoodNodeIds.Count == 0)
            {
                var noFoodLabel = new Label("No required foods set.");
                noFoodLabel.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f));
                noFoodLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                noFoodLabel.style.fontSize = 11;
                chipsContainer.Add(noFoodLabel);
            }
            else
            {
                for (int i = 0; i < customer.requireFoodNodeIds.Count; i++)
                {
                    int foodIndex = i;
                    string foodId = customer.requireFoodNodeIds[i];

                    var chip = new VisualElement();
                    chip.style.flexDirection = FlexDirection.Row;
                    chip.style.alignItems = Align.Center;
                    chip.style.backgroundColor = new StyleColor(new Color(0.32f, 0.32f, 0.32f));
                    chip.style.borderTopLeftRadius = 3;
                    chip.style.borderTopRightRadius = 3;
                    chip.style.borderBottomLeftRadius = 3;
                    chip.style.borderBottomRightRadius = 3;
                    chip.style.paddingLeft = 6;
                    chip.style.paddingRight = 4;
                    chip.style.paddingTop = 2;
                    chip.style.paddingBottom = 2;
                    chip.style.marginRight = 6;
                    chip.style.marginBottom = 4;

                    var chipIcon = new Image();
                    chipIcon.style.width = 32;
                    chipIcon.style.height = 32;
                    chipIcon.style.marginRight = 4;

                    string foodName = "Unknown";
                    if (!string.IsNullOrEmpty(foodId) && foodNodeMap.TryGetValue(foodId, out var nodeData))
                    {
                        foodName = string.IsNullOrEmpty(nodeData.Name) ? foodId : nodeData.Name;
                        if (nodeData.Sprite != null) chipIcon.image = nodeData.Sprite.texture;
                    }

                    chip.Add(chipIcon);

                    var chipName = new Label(foodName);
                    chipName.style.fontSize = 11;
                    chipName.style.marginRight = 6;
                    chip.Add(chipName);

                    // [X] Remove Food Requirement
                    var removeFoodBtn = new Button(() => RemoveRequiredFood(customer, foodIndex)) { text = "✕" };
                    removeFoodBtn.style.height = 16;
                    removeFoodBtn.style.width = 16;
                    removeFoodBtn.style.paddingLeft = 0;
                    removeFoodBtn.style.paddingRight = 0;
                    removeFoodBtn.style.paddingTop = 0;
                    removeFoodBtn.style.paddingBottom = 0;
                    removeFoodBtn.style.backgroundColor = new StyleColor(new Color(0.7f, 0.2f, 0.2f));
                    removeFoodBtn.style.color = new StyleColor(Color.white);
                    removeFoodBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
                    chip.Add(removeFoodBtn);

                    chipsContainer.Add(chip);
                }
            }

            reqRow.Add(chipsContainer);
            card.Add(reqRow);

            return card;
        }

        private void AddCustomerToTurn(int turn)
        {
            if (currentAsset == null) return;
            RegisterUndo("Add Customer");
            if (currentAsset.customers == null) currentAsset.customers = new List<SerializedCustomerData>();

            currentAsset.customers.Add(new SerializedCustomerData
            {
                id = Guid.NewGuid().ToString(),
                spawnTurn = turn,
                sprite = null,
                requireFoodNodeIds = new List<string>()
            });

            MarkDirty();
            RebuildTimelineSection();
        }

        private void RemoveCustomer(SerializedCustomerData customer)
        {
            if (currentAsset == null || customer == null) return;
            RegisterUndo("Remove Customer");
            currentAsset.customers.Remove(customer);
            MarkDirty();
            RebuildTimelineSection();
        }

        private void ShowAddRequiredFoodMenu(SerializedCustomerData customer)
        {
            if (availableFoodNodes.Count == 0)
            {
                EditorUtility.DisplayDialog("Warning",
                    "No FoodNodeData available! Please assign a valid FoodGraphAsset first.", "OK");
                return;
            }

            var menu = new GenericMenu();
            foreach (var node in availableFoodNodes)
            {
                string displayName = string.IsNullOrEmpty(node.Name) ? node.ID : node.Name;
                string nodeId = node.ID;
                menu.AddItem(new GUIContent(displayName), false, () => AddRequiredFood(customer, nodeId));
            }

            menu.ShowAsContext();
        }

        private void AddRequiredFood(SerializedCustomerData customer, string nodeId)
        {
            if (customer == null) return;
            RegisterUndo("Add Required Food");
            if (customer.requireFoodNodeIds == null) customer.requireFoodNodeIds = new List<string>();

            customer.requireFoodNodeIds.Add(nodeId);
            MarkDirty();
            RebuildTimelineSection();
        }

        private void RemoveRequiredFood(SerializedCustomerData customer, int index)
        {
            if (customer == null || customer.requireFoodNodeIds == null || index < 0 ||
                index >= customer.requireFoodNodeIds.Count) return;
            RegisterUndo("Remove Required Food");
            customer.requireFoodNodeIds.RemoveAt(index);
            MarkDirty();
            RebuildTimelineSection();
        }

        // --- File Operations ---

        private void NewLevelAction()
        {
            if (!PromptSaveIfDirty()) return;
            NewLevel();
        }

        private void NewLevel()
        {
            currentAsset = CreateInstance<LevelDataAsset>();
            currentAsset.foodGraphAsset = foodGraphAsset;
            isDirty = false;
            RebuildUI();
        }

        private void OpenLevelAction()
        {
            if (!PromptSaveIfDirty()) return;

            string path = EditorUtility.OpenFilePanel("Open Level Data Asset", "Assets", "asset");
            if (string.IsNullOrEmpty(path)) return;

            string relativePath = FileUtil.GetProjectRelativePath(path);
            var asset = AssetDatabase.LoadAssetAtPath<LevelDataAsset>(relativePath);
            if (asset == null)
            {
                EditorUtility.DisplayDialog("Error", "Selected asset is not a valid LevelDataAsset.", "OK");
                return;
            }

            LoadAsset(asset);
        }

        public void LoadAsset(LevelDataAsset asset)
        {
            currentAsset = asset;
            if (currentAsset.foodGraphAsset != null)
            {
                foodGraphAsset = currentAsset.foodGraphAsset;
            }

            isDirty = false;
            RefreshAvailableFoodNodes();
            RebuildUI();
        }

        private bool SaveLevel()
        {
            if (currentAsset == null) return false;

            string path = AssetDatabase.GetAssetPath(currentAsset);
            if (string.IsNullOrEmpty(path))
            {
                return SaveAsLevel();
            }

            EditorUtility.SetDirty(currentAsset);
            AssetDatabase.SaveAssets();
            isDirty = false;
            return true;
        }

        private bool SaveAsLevel()
        {
            if (currentAsset == null) return false;

            string path = EditorUtility.SaveFilePanelInProject("Save Level Data Asset", "NewLevelData", "asset",
                "Select save location");
            if (string.IsNullOrEmpty(path)) return false;

            var existingAsset = AssetDatabase.LoadAssetAtPath<LevelDataAsset>(path);
            if (existingAsset != null && existingAsset != currentAsset)
            {
                EditorUtility.CopySerialized(currentAsset, existingAsset);
                currentAsset = existingAsset;
            }
            else
            {
                AssetDatabase.CreateAsset(currentAsset, path);
            }

            EditorUtility.SetDirty(currentAsset);
            AssetDatabase.SaveAssets();
            isDirty = false;
            RebuildUI();
            return true;
        }

        private bool PromptSaveIfDirty()
        {
            if (!isDirty || currentAsset == null) return true;

            int choice = EditorUtility.DisplayDialogComplex("Unsaved Changes",
                "You have unsaved changes in the level editor. Save before continuing?",
                "Save", "Don't Save", "Cancel");

            switch (choice)
            {
                case 0: return SaveLevel();
                case 1: return true;
                case 2: return false;
                default: return false;
            }
        }

        private void RegisterUndo(string name)
        {
            if (currentAsset != null)
            {
                Undo.RecordObject(currentAsset, name);
            }
        }

        private void MarkDirty()
        {
            isDirty = true;
            if (currentAsset != null)
            {
                EditorUtility.SetDirty(currentAsset);
            }
        }
    }
}