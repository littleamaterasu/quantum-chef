using Gameplay.Scripts.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.Scripts.Controller
{
    public class InGameController : MonoBehaviour
    {
        [Header("Scene Config")]
        [SerializeField] private string homeSceneName = "HomeScene";
        [SerializeField] private LevelDataAsset defaultLevelAsset;

        private LevelData currentLevelData;

        private void Start()
        {
            InitScene();
        }

        /// <summary>
        /// Initializes the InGame scene by acquiring selected level data and calling GameController.StartGame().
        /// </summary>
        public void InitScene()
        {
            Debug.Log("[InGameController] Initializing InGame Gameplay Scene...");

            LevelDataAsset assetToLoad = HomeController.SelectedLevelAsset != null
                ? HomeController.SelectedLevelAsset
                : defaultLevelAsset;

            if (assetToLoad != null)
            {
                currentLevelData = assetToLoad.GetLevelData();
                Debug.Log($"[InGameController] Starting Level {HomeController.SelectedLevelIndex} with Asset '{assetToLoad.name}'");
            }
            else
            {
                currentLevelData = new LevelData();
                Debug.LogWarning("[InGameController] No LevelDataAsset assigned! Starting with empty LevelData.");
            }

            if (GameController.Instance != null)
            {
                GameController.Instance.StartGame(currentLevelData);
            }
            else
            {
                Debug.LogError("[InGameController] GameController.Instance is null!");
            }
        }

        /// <summary>
        /// Finishes current level, evaluates stars, awards coins, and saves user progress.
        /// </summary>
        public GameFinishResult FinishLevel()
        {
            int levelIndex = HomeController.SelectedLevelIndex;
            GameFinishResult result = GameController.Instance.FinishCurrentGame(levelIndex);
            Debug.Log($"[InGameController] Level {levelIndex} Finished! Stars: {result.starsEarned}, Coins Earned: {result.coinsEarned}");
            return result;
        }

        /// <summary>
        /// Restarts the current level using Iris Wipe transition.
        /// </summary>
        public void RestartLevel()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            SceneTransitionManager.Instance.LoadScene(currentScene, () =>
            {
                InitScene();
            });
        }

        /// <summary>
        /// Transitions back to the Home Scene using Iris Wipe transition.
        /// </summary>
        public void LoadHomeScene()
        {
            SceneTransitionManager.Instance.LoadScene(homeSceneName);
        }
    }
}
