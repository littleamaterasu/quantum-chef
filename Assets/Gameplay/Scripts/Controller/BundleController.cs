using System.Collections.Generic;
using Gameplay.Scripts.Data;
using UnityEngine;

namespace Gameplay.Scripts.Controller
{
    /// <summary>
    /// Runtime controller that manages Steam DLC bundle ownership and content unlock checks.
    /// Attach to a persistent GameObject in the initial scene and assign a BundleRegistryAsset.
    ///
    /// Flow:
    ///   1. On game start, call <see cref="RefreshFromSteam"/> (or grant bundles manually via
    ///      <see cref="UserDataManager.AddBundle"/>) to populate owned bundle data.
    ///   2. Query <see cref="IsContentUnlocked"/> / typed sugar methods before showing/loading content.
    /// </summary>
    public class BundleController : Singleton<BundleController>
    {
        [Tooltip("The BundleRegistryAsset that lists all available bundles in the game.")]
        [SerializeField] private BundleRegistryAsset bundleRegistry;

        // -----------------------------------------------------------------------
        //  Registry access
        // -----------------------------------------------------------------------

        /// <summary>All bundles registered in the game (excluding base game).</summary>
        public IReadOnlyList<BundleInfo> AllBundles =>
            bundleRegistry != null ? bundleRegistry.Bundles : new List<BundleInfo>();

        /// <summary>The registry asset reference (useful for editor tooling).</summary>
        public BundleRegistryAsset Registry => bundleRegistry;

        // -----------------------------------------------------------------------
        //  Core unlock check
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns <c>true</c> if content tagged with <paramref name="bundleId"/> is accessible.
        /// <list type="bullet">
        ///   <item>Empty / null bundleId → Base Game content → always unlocked.</item>
        ///   <item>Non-empty bundleId → checks <see cref="UserDataManager"/> ownership.</item>
        /// </list>
        /// </summary>
        public bool IsContentUnlocked(string bundleId)
        {
            // Base game content is always available
            if (string.IsNullOrEmpty(bundleId)) return true;

            // TODO: Verify with Steamworks SDK (e.g. SteamApps.BIsDlcInstalled(appId))
            //       once the Steamworks.NET or Facepunch.Steamworks package is integrated.
            //       For now we rely on UserDataManager which is populated after a purchase flow.

            if (UserDataManager.Instance == null)
            {
                Debug.LogWarning("[BundleController] UserDataManager not found. Treating content as locked.");
                return false;
            }

            return UserDataManager.Instance.HasBundle(bundleId);
        }

        // -----------------------------------------------------------------------
        //  Typed sugar methods
        // -----------------------------------------------------------------------

        /// <summary>Returns true if the given tool is unlocked (base game or owned bundle).</summary>
        public bool IsToolUnlocked(ToolData tool)
        {
            if (tool == null) return false;
            return IsContentUnlocked(tool.BundleId);
        }

        /// <summary>Returns true if the given food node is unlocked (base game or owned bundle).</summary>
        public bool IsFoodNodeUnlocked(FoodNodeData node)
        {
            if (node == null) return false;
            return IsContentUnlocked(node.BundleId);
        }

        /// <summary>Returns true if the given level is unlocked (base game or owned bundle).</summary>
        public bool IsLevelUnlocked(LevelDataAsset level)
        {
            if (level == null) return false;
            return IsContentUnlocked(level.bundleId);
        }

        // -----------------------------------------------------------------------
        //  Steam integration hook (call this after Steamworks init)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Queries Steam DLC ownership for all registered bundles and grants them locally.
        /// Call this once after Steamworks has been initialized (e.g. in an InitialSceneController).
        ///
        /// <para>
        /// NOTE: This method currently only logs a placeholder. Replace the body of the
        /// inner loop with actual Steamworks DLC queries when the SDK is integrated.
        /// </para>
        /// </summary>
        public void RefreshFromSteam()
        {
            if (bundleRegistry == null)
            {
                Debug.LogWarning("[BundleController] No BundleRegistryAsset assigned. Skipping Steam refresh.");
                return;
            }

            foreach (var bundle in bundleRegistry.Bundles)
            {
                if (string.IsNullOrEmpty(bundle.BundleId)) continue;

                // TODO: Replace the following with a real Steamworks DLC check, e.g.:
                //   bool owned = SteamApps.BIsDlcInstalled(new AppId_t(uint.Parse(bundle.BundleId)));
                //   if (owned) UserDataManager.Instance.AddBundle(bundle.BundleId);

                Debug.Log($"[BundleController] Steam refresh placeholder for bundle '{bundle.BundleId}' ({bundle.DisplayName}).");
            }
        }
    }
}
