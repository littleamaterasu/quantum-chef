using System.Collections.Generic;
using Gameplay.Scripts.Controller;
using Gameplay.Scripts.Data;
using UnityEngine;

namespace Gameplay.Scripts.UI.Panel
{
    public class LevelSelectionPanel : Singleton<LevelSelectionPanel>
    {
        [Header("UI Container")]
        [SerializeField] private RectTransform levelContainer;
        [SerializeField] private LevelCard levelCardPrefab;

        [Header("Level Data Assets")]
        [SerializeField] private List<LevelDataAsset> levelAssets = new List<LevelDataAsset>();

        public IReadOnlyList<LevelDataAsset> LevelAssets => levelAssets;

        private void Start()
        {
            InitLevelList();
        }

        /// <summary>
        /// Clears previous level cards and spawns level cards into the levelContainer RectTransform.
        /// Unlocks cards where levelIndex <= UserDataManager.Instance.ReachedLevel.
        /// </summary>
        public void InitLevelList()
        {
            if (levelContainer == null)
            {
                Debug.LogError("[LevelSelectionPanel] levelContainer RectTransform is not assigned!");
                return;
            }

            if (levelCardPrefab == null)
            {
                Debug.LogError("[LevelSelectionPanel] levelCardPrefab is not assigned!");
                return;
            }

            // 1. Clear existing spawned card children
            foreach (Transform child in levelContainer)
            {
                Destroy(child.gameObject);
            }

            // 2. Fetch reached level
            int reachedLevel = UserDataManager.Instance.ReachedLevel;

            // Auto-load level assets from Resources or Editor if list is empty
            if (levelAssets == null || levelAssets.Count == 0)
            {
                AutoFindLevelAssets();
            }

            // 3. Spawn level cards
            for (int i = 0; i < levelAssets.Count; i++)
            {
                int levelIndex = i;
                LevelDataAsset asset = levelAssets[i];
                bool isUnlocked = (levelIndex <= reachedLevel);

                LevelCard card = Instantiate(levelCardPrefab, levelContainer);
                card.Setup(levelIndex, asset, isUnlocked, OnLevelCardClicked);
            }

            Debug.Log($"[LevelSelectionPanel] Spawned {levelAssets.Count} level cards into levelContainer. ReachedLevel: {reachedLevel}");
        }

        private void OnLevelCardClicked(int levelIndex, LevelDataAsset levelAsset)
        {
            Debug.Log($"[LevelSelectionPanel] Level card clicked: Level {levelIndex} ('{(levelAsset != null ? levelAsset.name : "null")}')");
            HomeController.Instance.LoadLevel(levelIndex, levelAsset);
        }

        private void AutoFindLevelAssets()
        {
            levelAssets = new List<LevelDataAsset>();

#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:LevelDataAsset");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDataAsset>(path);
                if (asset != null)
                {
                    levelAssets.Add(asset);
                }
            }
#endif
        }
    }
}
