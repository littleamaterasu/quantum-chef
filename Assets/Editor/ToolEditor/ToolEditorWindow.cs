using System.Collections.Generic;
using Gameplay.Scripts.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.ToolEditor
{
    public class ToolEditorWindow : EditorWindow
    {
        private ToolCollectionAsset currentAsset;
        private bool isDirty = false;

        private ListView toolListView;
        private VisualElement detailPanel;
        private int selectedIndex = -1;

        [MenuItem("Tools/Tool Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<ToolEditorWindow>();
            window.titleContent = new GUIContent("Tool Editor");
            window.minSize = new Vector2(700, 400);
            window.Show();
        }

        private void OnEnable()
        {
            saveChangesMessage = "The tool collection has unsaved changes. Do you want to save them?";
            ConstructVisualTree();
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown);

            if (currentAsset == null)
            {
                NewCollection();
            }
        }

        private void ConstructVisualTree()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            // --- Toolbar ---
            var toolbar = new Toolbar();
            toolbar.style.height = 28;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.paddingLeft = 4;
            toolbar.style.paddingRight = 4;

            toolbar.Add(MakeToolbarButton("New", NewCollectionAction));
            toolbar.Add(MakeToolbarButton("Open", OpenCollectionAction));
            toolbar.Add(MakeToolbarButton("Save", SaveCollection));
            toolbar.Add(MakeToolbarButton("Save As", SaveAsCollection));
            toolbar.Add(new ToolbarSpacer { flex = true });
            toolbar.Add(MakeToolbarButton("+ Add Tool", AddTool));

            rootVisualElement.Add(toolbar);

            // --- Main split panel ---
            var splitPanel = new TwoPaneSplitView(0, 220, TwoPaneSplitViewOrientation.Horizontal);
            splitPanel.style.flexGrow = 1;

            // Left: list
            var listContainer = new VisualElement();
            listContainer.style.minWidth = 160;
            listContainer.style.backgroundColor = new StyleColor(new Color(0.16f, 0.16f, 0.16f));

            toolListView = new ListView
            {
                fixedItemHeight = 48,
                selectionType = SelectionType.Single,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showBorder = false
            };
            toolListView.style.flexGrow = 1;

            toolListView.makeItem = () =>
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.paddingLeft = 8;
                row.style.paddingRight = 8;
                row.style.paddingTop = 4;
                row.style.paddingBottom = 4;

                var icon = new Image();
                icon.name = "tool-icon";
                icon.style.width = 32;
                icon.style.height = 32;
                icon.style.marginRight = 8;
                icon.style.borderBottomLeftRadius = 4;
                icon.style.borderBottomRightRadius = 4;
                icon.style.borderTopLeftRadius = 4;
                icon.style.borderTopRightRadius = 4;
                icon.style.borderTopWidth = 1;
                icon.style.borderBottomWidth = 1;
                icon.style.borderLeftWidth = 1;
                icon.style.borderRightWidth = 1;
                icon.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                icon.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                icon.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                icon.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                row.Add(icon);

                var nameLabel = new Label();
                nameLabel.name = "tool-name";
                nameLabel.style.fontSize = 13;
                nameLabel.style.color = new StyleColor(Color.white);
                nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(nameLabel);

                return row;
            };

            toolListView.bindItem = (element, index) =>
            {
                if (currentAsset == null || index >= currentAsset.Tools.Count) return;
                var tool = currentAsset.Tools[index];

                var icon = element.Q<Image>("tool-icon");
                if (tool.Icon != null)
                {
                    icon.image = tool.Icon.texture;
                    icon.style.display = DisplayStyle.Flex;
                }
                else
                {
                    icon.image = null;
                    icon.style.display = DisplayStyle.None;
                }

                var nameLabel = element.Q<Label>("tool-name");
                nameLabel.text = string.IsNullOrEmpty(tool.Name) ? $"Tool #{tool.ID}" : tool.Name;
            };

            toolListView.selectedIndicesChanged += indices =>
            {
                var enumerator = indices.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    selectedIndex = enumerator.Current;
                    BuildDetailPanel(selectedIndex);
                }
                else
                {
                    selectedIndex = -1;
                    ClearDetailPanel();
                }
            };

            toolListView.itemIndexChanged += (oldIndex, newIndex) =>
            {
                RegisterUndo("Reorder Tools");
                MarkDirty();
            };

            listContainer.Add(toolListView);
            splitPanel.Add(listContainer);

            // Right: detail panel
            detailPanel = new VisualElement();
            detailPanel.style.flexGrow = 1;
            detailPanel.style.paddingLeft = 16;
            detailPanel.style.paddingRight = 16;
            detailPanel.style.paddingTop = 12;
            detailPanel.style.paddingBottom = 12;
            detailPanel.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));

            var placeholder = new Label("Select a tool from the list to edit.");
            placeholder.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f));
            placeholder.style.fontSize = 14;
            placeholder.style.unityFontStyleAndWeight = FontStyle.Italic;
            placeholder.style.marginTop = 40;
            placeholder.style.unityTextAlign = TextAnchor.MiddleCenter;
            detailPanel.Add(placeholder);

            splitPanel.Add(detailPanel);

            rootVisualElement.Add(splitPanel);
        }

        private Button MakeToolbarButton(string text, System.Action action)
        {
            var btn = new Button(action) { text = text };
            btn.style.height = 22;
            return btn;
        }

        // --- Detail Panel ---

        private void ClearDetailPanel()
        {
            detailPanel.Clear();
            var placeholder = new Label("Select a tool from the list to edit.");
            placeholder.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f));
            placeholder.style.fontSize = 14;
            placeholder.style.unityFontStyleAndWeight = FontStyle.Italic;
            placeholder.style.marginTop = 40;
            placeholder.style.unityTextAlign = TextAnchor.MiddleCenter;
            detailPanel.Add(placeholder);
        }

        private void BuildDetailPanel(int index)
        {
            detailPanel.Clear();

            if (currentAsset == null || index < 0 || index >= currentAsset.Tools.Count) return;

            var tool = currentAsset.Tools[index];

            // Header
            var header = new Label("Edit Tool");
            header.style.fontSize = 18;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.color = new StyleColor(new Color(0.95f, 0.6f, 0.2f));
            header.style.marginBottom = 12;
            detailPanel.Add(header);

            // ID
            var idField = new IntegerField("ID") { value = tool.ID };
            idField.RegisterValueChangedCallback(evt =>
            {
                RegisterUndo("Change Tool ID");
                tool.ID = evt.newValue;
                MarkDirty();
                RefreshList();
            });
            detailPanel.Add(idField);

            // Name
            var nameField = new TextField("Name") { value = tool.Name ?? "" };
            nameField.RegisterValueChangedCallback(evt =>
            {
                RegisterUndo("Change Tool Name");
                tool.Name = evt.newValue;
                MarkDirty();
                RefreshList();
            });
            detailPanel.Add(nameField);

            // Icon (ObjectField + preview)
            var iconField = new ObjectField("Icon")
            {
                objectType = typeof(Sprite),
                value = tool.Icon
            };

            var iconPreview = new Image();
            iconPreview.style.width = 64;
            iconPreview.style.height = 64;
            iconPreview.style.marginTop = 4;
            iconPreview.style.marginBottom = 8;
            iconPreview.style.alignSelf = Align.FlexStart;
            iconPreview.style.borderBottomLeftRadius = 4;
            iconPreview.style.borderBottomRightRadius = 4;
            iconPreview.style.borderTopLeftRadius = 4;
            iconPreview.style.borderTopRightRadius = 4;
            iconPreview.style.borderTopWidth = 1;
            iconPreview.style.borderBottomWidth = 1;
            iconPreview.style.borderLeftWidth = 1;
            iconPreview.style.borderRightWidth = 1;
            iconPreview.style.borderTopColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            iconPreview.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            iconPreview.style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            iconPreview.style.borderRightColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

            UpdateIconPreview(iconPreview, tool.Icon);

            iconField.RegisterValueChangedCallback(evt =>
            {
                RegisterUndo("Change Tool Icon");
                var newSprite = evt.newValue as Sprite;
                tool.Icon = newSprite;
                UpdateIconPreview(iconPreview, newSprite);
                MarkDirty();
                RefreshList();
            });
            detailPanel.Add(iconField);
            detailPanel.Add(iconPreview);

            // Bonus Rewind Turn
            var bonusField = new IntegerField("Bonus Rewind Turn") { value = tool.BonusRewindTurn };
            bonusField.RegisterValueChangedCallback(evt =>
            {
                RegisterUndo("Change Bonus Rewind Turn");
                tool.BonusRewindTurn = evt.newValue;
                MarkDirty();
            });
            detailPanel.Add(bonusField);

            // Reduce Create Turn
            var reduceField = new IntegerField("Reduce Create Turn") { value = tool.ReduceCreateTurn };
            reduceField.RegisterValueChangedCallback(evt =>
            {
                RegisterUndo("Change Reduce Create Turn");
                tool.ReduceCreateTurn = evt.newValue;
                MarkDirty();
            });
            detailPanel.Add(reduceField);

            // Spacer
            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            detailPanel.Add(spacer);

            // Delete button at bottom
            var deleteBtn = new Button(() => DeleteTool(index))
            {
                text = "Delete Tool"
            };
            deleteBtn.style.backgroundColor = new StyleColor(new Color(0.7f, 0.15f, 0.15f));
            deleteBtn.style.color = new StyleColor(Color.white);
            deleteBtn.style.height = 28;
            deleteBtn.style.marginTop = 8;
            deleteBtn.style.borderBottomLeftRadius = 4;
            deleteBtn.style.borderBottomRightRadius = 4;
            deleteBtn.style.borderTopLeftRadius = 4;
            deleteBtn.style.borderTopRightRadius = 4;
            detailPanel.Add(deleteBtn);
        }

        private static void UpdateIconPreview(Image preview, Sprite sprite)
        {
            if (sprite != null)
            {
                preview.image = sprite.texture;
                preview.style.display = DisplayStyle.Flex;
            }
            else
            {
                preview.image = null;
                preview.style.display = DisplayStyle.None;
            }
        }

        // --- CRUD Operations ---

        private void AddTool()
        {
            RegisterUndo("Add Tool");

            int nextId = 0;
            if (currentAsset != null && currentAsset.Tools.Count > 0)
            {
                foreach (var t in currentAsset.Tools)
                {
                    if (t.ID >= nextId) nextId = t.ID + 1;
                }
            }

            var newTool = new ToolData { ID = nextId, Name = "New Tool" };
            currentAsset.Tools.Add(newTool);
            MarkDirty();
            RefreshList();

            // Select the newly added item
            toolListView.selectedIndex = currentAsset.Tools.Count - 1;
        }

        private void DeleteTool(int index)
        {
            if (currentAsset == null || index < 0 || index >= currentAsset.Tools.Count) return;

            var tool = currentAsset.Tools[index];
            string toolName = string.IsNullOrEmpty(tool.Name) ? $"Tool #{tool.ID}" : tool.Name;

            if (!EditorUtility.DisplayDialog("Delete Tool", $"Are you sure you want to delete \"{toolName}\"?", "Delete", "Cancel"))
                return;

            RegisterUndo("Delete Tool");
            currentAsset.Tools.RemoveAt(index);
            MarkDirty();
            selectedIndex = -1;
            ClearDetailPanel();
            RefreshList();
        }

        // --- List refresh ---

        private void RefreshList()
        {
            if (currentAsset == null)
            {
                toolListView.itemsSource = new List<ToolData>();
            }
            else
            {
                toolListView.itemsSource = currentAsset.Tools;
            }
            toolListView.Rebuild();
        }

        // --- Toolbar actions ---

        private void NewCollectionAction()
        {
            if (PromptToSaveIfDirty())
                NewCollection();
        }

        private void OpenCollectionAction()
        {
            if (PromptToSaveIfDirty())
                OpenCollection();
        }

        private void NewCollection()
        {
            currentAsset = CreateInstance<ToolCollectionAsset>();
            currentAsset.name = "New Tool Collection";
            isDirty = false;
            hasUnsavedChanges = false;
            selectedIndex = -1;
            ClearDetailPanel();
            RefreshList();
            UpdateWindowTitle();
        }

        private void OpenCollection()
        {
            CreateDefaultDirectories();
            string path = EditorUtility.OpenFilePanel("Open Tool Collection", "Assets/GameData/ToolCollections", "asset");
            if (string.IsNullOrEmpty(path)) return;

            if (path.StartsWith(Application.dataPath))
                path = "Assets" + path.Substring(Application.dataPath.Length);

            var asset = AssetDatabase.LoadAssetAtPath<ToolCollectionAsset>(path);
            if (asset != null)
            {
                currentAsset = asset;
                isDirty = false;
                hasUnsavedChanges = false;
                selectedIndex = -1;
                ClearDetailPanel();
                RefreshList();
                UpdateWindowTitle();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Could not load selected asset. Is it a ToolCollectionAsset?", "OK");
            }
        }

        public override void SaveChanges()
        {
            SaveCollection();
            base.SaveChanges();
        }

        private void SaveCollection()
        {
            if (currentAsset == null || AssetDatabase.GetAssetPath(currentAsset) == "")
            {
                SaveAsCollection();
            }
            else
            {
                EditorUtility.SetDirty(currentAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                isDirty = false;
                hasUnsavedChanges = false;
                UpdateWindowTitle();
            }
        }

        private void SaveAsCollection()
        {
            CreateDefaultDirectories();
            string defaultName = currentAsset != null ? currentAsset.name : "NewToolCollection";
            string path = EditorUtility.SaveFilePanelInProject("Save Tool Collection", defaultName, "asset",
                "Save Tool Collection Asset", "Assets/GameData/ToolCollections");
            if (string.IsNullOrEmpty(path)) return;

            var asset = CreateInstance<ToolCollectionAsset>();
            AssetDatabase.CreateAsset(asset, path);

            // Copy data from in-memory asset
            asset.Tools.Clear();
            if (currentAsset != null)
            {
                foreach (var t in currentAsset.Tools)
                {
                    asset.Tools.Add(new ToolData
                    {
                        ID = t.ID,
                        Name = t.Name,
                        Icon = t.Icon,
                        BonusRewindTurn = t.BonusRewindTurn,
                        ReduceCreateTurn = t.ReduceCreateTurn
                    });
                }
            }

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            currentAsset = asset;
            isDirty = false;
            hasUnsavedChanges = false;
            RefreshList();
            UpdateWindowTitle();
        }

        // --- Undo ---

        private void RegisterUndo(string actionName)
        {
            if (currentAsset != null)
            {
                Undo.RegisterCompleteObjectUndo(currentAsset, actionName);
            }
        }

        private void MarkDirty()
        {
            isDirty = true;
            hasUnsavedChanges = true;
            UpdateWindowTitle();
            if (currentAsset != null)
                EditorUtility.SetDirty(currentAsset);
        }

        // --- Key shortcuts ---

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
            {
                SaveCollection();
                evt.StopPropagation();
            }
        }

        // --- Helpers ---

        private void UpdateWindowTitle()
        {
            string assetName = currentAsset == null ? "New Collection" : currentAsset.name;
            string dirtyMarker = isDirty ? "*" : "";
            titleContent = new GUIContent($"Tool Editor - {assetName}{dirtyMarker}");
        }

        private bool PromptToSaveIfDirty()
        {
            if (!isDirty) return true;
            int option = EditorUtility.DisplayDialogComplex("Unsaved Changes",
                "The current tool collection has unsaved changes. Do you want to save them?",
                "Save", "Don't Save", "Cancel");
            if (option == 0) { SaveCollection(); return !isDirty; }
            if (option == 1) return true;
            return false;
        }

        private void CreateDefaultDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/GameData"))
                AssetDatabase.CreateFolder("Assets", "GameData");
            if (!AssetDatabase.IsValidFolder("Assets/GameData/ToolCollections"))
                AssetDatabase.CreateFolder("Assets/GameData", "ToolCollections");
        }
    }
}
