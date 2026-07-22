using System.Collections;
using Gameplay.Scripts.Data;
using UnityEngine;

namespace Gameplay.Scripts.Controller
{
    public class InitialSceneController : MonoBehaviour
    {
        [Header("Scene Config")]
        [SerializeField] private string homeSceneName = "HomeScene";
        [SerializeField] private float autoTransitionDelay = 0.5f;

        private void Start()
        {
            InitScene();
        }

        /// <summary>
        /// Initializes scene data, user data, asset services, and bootstraps the application.
        /// </summary>
        public void InitScene()
        {
            Debug.Log("[InitialSceneController] Initializing application & user data...");

            // Load user data
            UserData userData = UserDataManager.Instance.Data;
            Debug.Log($"[InitialSceneController] User Data loaded. AccountId: {userData.AccountId}, Coins: {userData.coin}, ReachedLevel: {userData.reachedLevel}");

            StartCoroutine(CoAutoTransitionHome());
        }

        private IEnumerator CoAutoTransitionHome()
        {
            if (autoTransitionDelay > 0)
            {
                yield return new WaitForSeconds(autoTransitionDelay);
            }

            LoadHomeScene();
        }

        public void LoadHomeScene()
        {
            SceneTransitionManager.Instance.LoadScene(homeSceneName, () =>
            {
                Debug.Log("[InitialSceneController] Successfully transitioned to HomeScene.");
            });
        }
    }
}
