using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Scripts.Data
{
    /// <summary>
    /// ScriptableObject that acts as the single source of truth for all bundles in the project.
    /// Place one instance at Assets/GameData/Bundles/BundleRegistry.asset.
    /// All Editor windows and the BundleController reference this asset automatically.
    /// </summary>
    [CreateAssetMenu(fileName = "BundleRegistry", menuName = "Bundle/Bundle Registry", order = 0)]
    public class BundleRegistryAsset : ScriptableObject
    {
        [SerializeField]
        private List<BundleInfo> bundles = new List<BundleInfo>();

        /// <summary>
        /// The list of all registered bundles (excluding base game).
        /// </summary>
        public List<BundleInfo> Bundles => bundles;

        /// <summary>
        /// Tries to find a BundleInfo by its id. Returns null if not found.
        /// </summary>
        public BundleInfo GetBundle(string bundleId)
        {
            if (string.IsNullOrEmpty(bundleId)) return null;
            foreach (var b in bundles)
            {
                if (b.BundleId == bundleId) return b;
            }
            return null;
        }

        /// <summary>
        /// Returns true if the given bundleId exists in the registry.
        /// </summary>
        public bool Contains(string bundleId)
        {
            return GetBundle(bundleId) != null;
        }
    }
}
