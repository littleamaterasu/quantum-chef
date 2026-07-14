using System.IO;
using Gameplay.Scripts.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.FoodGraphEditor
{
    public class FoodGraphEditorWindow : EditorWindow
    {
        private FoodGraphView graphView;
        private FoodGraphAsset currentAsset;
        private bool isDirty = false;

        [MenuItem("Tools/Food Graph Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<FoodGraphEditorWindow>();
            window.titleContent = new GUIContent("Food Graph Editor");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnEnable()
        {
            // Set save changes prompt details
            saveChangesMessage = "The food graph has unsaved changes. Do you want to save them?";

            // Initialize or retrieve the canvas view
            ConstructVisualTree();

            // Register undo/redo callback
            Undo.undoRedoPerformed += OnUndoRedo;

            // Setup shortcut callback for saving/framing
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown);

            // Reconstruct the graph view
            if (currentAsset == null)
            {
                NewGraph();
            }
            else
            {
                LoadFromAsset(currentAsset);
            }
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void ConstructVisualTree()
        {
            rootVisualElement.Clear();

            // Create Toolbar
            var toolbar = new Toolbar();
            toolbar.style.height = 24;
            toolbar.style.alignItems = Align.Center;

            var newButton = new Button(NewGraphAction) { text = "New" };
            newButton.style.height = 20;

            var openButton = new Button(OpenGraphAction) { text = "Open" };
            openButton.style.height = 20;

            var saveButton = new Button(SaveGraph) { text = "Save" };
            saveButton.style.height = 20;

            var saveAsButton = new Button(SaveAsGraph) { text = "Save As" };
            saveAsButton.style.height = 20;

            var centerButton = new Button(CenterView) { text = "Center View" };
            centerButton.style.height = 20;

            toolbar.Add(newButton);
            toolbar.Add(openButton);
            toolbar.Add(saveButton);
            toolbar.Add(saveAsButton);
            toolbar.Add(centerButton);

            rootVisualElement.Add(toolbar);

            // Create GraphView
            graphView = new FoodGraphView(this);
            graphView.style.flexGrow = 1;
            rootVisualElement.Add(graphView);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
            {
                SaveGraph();
                evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.F && !evt.ctrlKey && !evt.shiftKey && !evt.altKey)
            {
                // Only frame selection if the keyboard focus is not inside a text entry box
                if (!(rootVisualElement.focusController.focusedElement is TextField))
                {
                    graphView.FrameSelection();
                    evt.StopPropagation();
                }
            }
        }

        // --- Undo / Redo integration ---

        public void RegisterUndo(string actionName)
        {
            if (currentAsset != null)
            {
                // Sync latest positions/values before registering the undo state
                FoodGraphSerializer.SaveToAsset(currentAsset, graphView);
                Undo.RegisterCompleteObjectUndo(currentAsset, actionName);
            }
        }

        public void OnGraphModified()
        {
            isDirty = true;
            hasUnsavedChanges = true;
            UpdateWindowTitle();

            if (currentAsset != null)
            {
                FoodGraphSerializer.SaveToAsset(currentAsset, graphView);
                EditorUtility.SetDirty(currentAsset);
            }
        }

        private void OnUndoRedo()
        {
            if (currentAsset != null)
            {
                // Read from reconstructed asset and reload visuals
                FoodGraphSerializer.LoadFromAsset(currentAsset, graphView);
                
                // Clear dirty state on undo since we reverted back
                isDirty = false;
                hasUnsavedChanges = false;
                UpdateWindowTitle();
            }
        }

        // --- Toolbar Actions ---

        private void NewGraphAction()
        {
            if (PromptToSaveIfDirty())
            {
                NewGraph();
            }
        }

        private void OpenGraphAction()
        {
            if (PromptToSaveIfDirty())
            {
                OpenGraph();
            }
        }

        private void NewGraph()
        {
            // Clear current canvas and create an in-memory asset
            graphView.ClearGraphElements();
            currentAsset = CreateInstance<FoodGraphAsset>();
            currentAsset.name = "New Graph";
            
            isDirty = false;
            hasUnsavedChanges = false;
            UpdateWindowTitle();
        }

        private void OpenGraph()
        {
            // Ensure default directory exists
            CreateDefaultDirectories();

            string path = EditorUtility.OpenFilePanel("Open Food Graph", "Assets/GameData/FoodGraphs", "asset");
            if (string.IsNullOrEmpty(path)) return;

            // Make path relative to project folder
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            var asset = AssetDatabase.LoadAssetAtPath<FoodGraphAsset>(path);
            if (asset != null)
            {
                LoadFromAsset(asset);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Could not load selected asset. Is it a FoodGraphAsset?", "OK");
            }
        }

        private void LoadFromAsset(FoodGraphAsset asset)
        {
            currentAsset = asset;
            FoodGraphSerializer.LoadFromAsset(currentAsset, graphView);
            
            isDirty = false;
            hasUnsavedChanges = false;
            UpdateWindowTitle();
        }

        public override void SaveChanges()
        {
            SaveGraph();
            base.SaveChanges();
        }

        private void SaveGraph()
        {
            // Check if currentAsset is an in-memory unsaved instance
            if (currentAsset == null || AssetDatabase.GetAssetPath(currentAsset) == "")
            {
                SaveAsGraph();
            }
            else
            {
                FoodGraphSerializer.SaveToAsset(currentAsset, graphView);
                EditorUtility.SetDirty(currentAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                isDirty = false;
                hasUnsavedChanges = false;
                UpdateWindowTitle();
            }
        }

        private void SaveAsGraph()
        {
            CreateDefaultDirectories();

            string defaultName = currentAsset != null ? currentAsset.name : "NewGraph";
            string path = EditorUtility.SaveFilePanelInProject("Save Food Graph", defaultName, "asset", "Save Food Graph Asset", "Assets/GameData/FoodGraphs");
            if (string.IsNullOrEmpty(path)) return;

            // Instantiate asset on disk
            var asset = CreateInstance<FoodGraphAsset>();
            AssetDatabase.CreateAsset(asset, path);

            // Write current elements to asset
            FoodGraphSerializer.SaveToAsset(asset, graphView);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            currentAsset = asset;
            isDirty = false;
            hasUnsavedChanges = false;
            UpdateWindowTitle();
        }

        private void CenterView()
        {
            if (graphView.nodes.ToList().Count > 0)
            {
                graphView.FrameAll();
            }
            else
            {
                graphView.UpdateViewTransform(Vector3.zero, Vector3.one);
            }
        }

        // --- Helpers ---

        private void UpdateWindowTitle()
        {
            string assetName = currentAsset == null ? "New Graph" : currentAsset.name;
            string dirtyMarker = isDirty ? "*" : "";
            titleContent = new GUIContent($"Food Graph Editor - {assetName}{dirtyMarker}");
        }

        private bool PromptToSaveIfDirty()
        {
            if (!isDirty) return true;

            int option = EditorUtility.DisplayDialogComplex(
                "Unsaved Changes",
                "The current food graph has unsaved changes. Do you want to save them?",
                "Save",
                "Don't Save",
                "Cancel"
            );

            if (option == 0) // Save
            {
                SaveGraph();
                return !isDirty; // True if saved successfully (isDirty became false)
            }
            if (option == 1) // Don't Save
            {
                return true;
            }
            return false; // Cancel
        }

        private void CreateDefaultDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/GameData"))
            {
                AssetDatabase.CreateFolder("Assets", "GameData");
            }
            if (!AssetDatabase.IsValidFolder("Assets/GameData/FoodGraphs"))
            {
                AssetDatabase.CreateFolder("Assets/GameData", "FoodGraphs");
            }
        }
    }
}
