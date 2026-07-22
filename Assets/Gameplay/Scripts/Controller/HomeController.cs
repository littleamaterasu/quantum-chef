using Gameplay.Scripts.Data;
using Gameplay.Scripts.UI.Panel;
using UnityEngine;

namespace Gameplay.Scripts.Controller
{
    public class HomeController : Singleton<HomeController>
    {
        [Header("Scene Config")]
        [SerializeField] private string inGameSceneName = "InGameScene";
        [SerializeField] private LevelSelectionPanel levelSelectionPanel;

        public static LevelDataAsset SelectedLevelAsset { get; set; }
        public static int SelectedLevelIndex { get; set; } = 0;

        private void Start()
        {
            InitScene();
        }

        /// <summary>
        /// Initializes the Home Scene, refreshing user stats (coins, stars, reached level) and spawning level cards.
        /// </summary>
        public void InitScene()
        {
            Debug.Log("[HomeController] Initializing Home Scene...");
            int coins = UserDataManager.Instance.Coin;
            int reachedLevel = UserDataManager.Instance.ReachedLevel;
            Debug.Log($"[HomeController] Home UI Updated - Coins: {coins}, ReachedLevel: {reachedLevel}");

            if (levelSelectionPanel == null)
            {
                levelSelectionPanel = LevelSelectionPanel.Instance;
            }

            if (levelSelectionPanel != null)
            {
                levelSelectionPanel.InitLevelList();
            }
        }

        /// <summary>
        /// Selects a level by index and LevelDataAsset, then transitions to InGameScene using Iris Wipe.
        /// </summary>
        public void LoadLevel(int levelIndex, LevelDataAsset levelDataAsset)
        {
            SelectedLevelIndex = levelIndex;
            SelectedLevelAsset = levelDataAsset;

            Debug.Log($"[HomeController] Launching Level {levelIndex} with asset '{(levelDataAsset != null ? levelDataAsset.name : "null")}'");

            SceneTransitionManager.Instance.LoadScene(inGameSceneName, () =>
            {
                Debug.Log($"[HomeController] Transition to {inGameSceneName} complete.");
            });
        }

        /// <summary>
        /// Convenience overload to load level with a LevelDataAsset.
        /// </summary>
        public void LoadLevel(LevelDataAsset levelDataAsset)
        {
            LoadLevel(SelectedLevelIndex, levelDataAsset);
        }
    }
}
