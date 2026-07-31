using Gameplay.Scripts.Data;
using UnityEditor;

namespace Editor
{
    /// <summary>
    /// Shared Editor utility for bundle dropdown generation.
    /// Used by ToolEditorWindow, FoodNodeView, and LevelEditorWindow.
    /// </summary>
    public static class BundleEditorHelper
    {
        private const string NoneLabel = "None (Base Game)";

        // -----------------------------------------------------------------------
        //  Asset loading
        // -----------------------------------------------------------------------

        /// <summary>
        /// Finds and loads the first BundleRegistryAsset in the project.
        /// Returns null if none exists yet.
        /// </summary>
        public static BundleRegistryAsset FindOrLoadBundleRegistry()
        {
            string[] guids = AssetDatabase.FindAssets("t:BundleRegistryAsset");
            if (guids.Length == 0) return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<BundleRegistryAsset>(path);
        }

        // -----------------------------------------------------------------------
        //  Dropdown helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds the string array for a DropdownField.
        /// Index 0 is always "None (Base Game)" which maps to an empty bundleId.
        /// Subsequent entries correspond to bundles in the registry.
        /// </summary>
        public static string[] BuildDropdownOptions(BundleRegistryAsset registry)
        {
            if (registry == null || registry.Bundles == null || registry.Bundles.Count == 0)
                return new[] { NoneLabel };

            var options = new string[registry.Bundles.Count + 1];
            options[0] = NoneLabel;
            for (int i = 0; i < registry.Bundles.Count; i++)
            {
                var b = registry.Bundles[i];
                options[i + 1] = string.IsNullOrEmpty(b.DisplayName) ? b.BundleId : b.DisplayName;
            }
            return options;
        }

        /// <summary>
        /// Converts a bundleId stored in data to a dropdown index.
        /// Empty / null bundleId → 0 (None / Base Game).
        /// </summary>
        public static int BundleIdToIndex(string bundleId, BundleRegistryAsset registry)
        {
            if (string.IsNullOrEmpty(bundleId) || registry == null) return 0;

            for (int i = 0; i < registry.Bundles.Count; i++)
            {
                if (registry.Bundles[i].BundleId == bundleId)
                    return i + 1; // +1 for "None" at index 0
            }

            return 0; // Not found → base game
        }

        /// <summary>
        /// Converts a dropdown index back to a bundleId string.
        /// Index 0 (None) → empty string.
        /// </summary>
        public static string IndexToBundleId(int index, BundleRegistryAsset registry)
        {
            if (index <= 0 || registry == null || registry.Bundles == null) return "";
            int bundleIndex = index - 1;
            if (bundleIndex >= registry.Bundles.Count) return "";
            return registry.Bundles[bundleIndex].BundleId;
        }
    }
}
