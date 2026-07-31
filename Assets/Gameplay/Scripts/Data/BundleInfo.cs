using System;
using UnityEngine;

namespace Gameplay.Scripts.Data
{
    /// <summary>
    /// Represents a single Steam DLC / content bundle entry.
    /// Leave bundleId empty to indicate "Base Game" content (always unlocked).
    /// </summary>
    [Serializable]
    public class BundleInfo
    {
        [SerializeField] private string bundleId = "";
        [SerializeField] private string displayName = "";

        /// <summary>
        /// Unique bundle identifier. Maps to Steam DLC App ID or a custom string key.
        /// An empty string means "Base Game" (always unlocked).
        /// </summary>
        public string BundleId
        {
            get => bundleId;
            set => bundleId = value;
        }

        /// <summary>
        /// Human-readable bundle name shown in editor dropdowns and UI.
        /// </summary>
        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }
    }
}
