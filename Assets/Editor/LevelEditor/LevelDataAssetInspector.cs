using Gameplay.Scripts.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.LevelEditor
{
    [CustomEditor(typeof(LevelDataAsset))]
    public class LevelDataAssetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(12);

            GUI.backgroundColor = new Color(0.2f, 0.6f, 1f);
            if (GUILayout.Button("Open in Level Editor", GUILayout.Height(32)))
            {
                var asset = (LevelDataAsset)target;
                LevelEditorWindow.OpenWithAsset(asset);
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
