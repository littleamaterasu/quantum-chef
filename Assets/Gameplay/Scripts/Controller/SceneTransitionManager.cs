using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gameplay.Scripts.Controller
{
    public class SceneTransitionManager : PersistentSingleton<SceneTransitionManager>
    {
        [Header("Transition Settings")]
        [SerializeField] private float transitionDuration = 0.45f;
        [SerializeField] private Color maskColor = Color.black;

        private Canvas transitionCanvas;
        private Image transitionImage;
        private Material irisMaterial;
        private static readonly int RadiusProp = Shader.PropertyToID("_Radius");
        private static readonly int ColorProp = Shader.PropertyToID("_Color");

        private bool isTransitioning = false;

        public bool IsTransitioning => isTransitioning;

        protected override void Awake()
        {
            base.Awake();
            SetupTransitionCanvas();
        }

        private void SetupTransitionCanvas()
        {
            if (transitionCanvas != null) return;

            // 1. Create Canvas GameObject
            var canvasObj = new GameObject("[SceneTransitionCanvas]");
            canvasObj.transform.SetParent(transform);

            transitionCanvas = canvasObj.AddComponent<Canvas>();
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            transitionCanvas.sortingOrder = 9999; // Always render on top

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // 2. Create Full-Screen Transition Image
            var imageObj = new GameObject("IrisMaskImage");
            imageObj.transform.SetParent(canvasObj.transform, false);

            var rectTransform = imageObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.one;

            transitionImage = imageObj.AddComponent<Image>();

            // 3. Load or Create IrisWipe Material
            Shader irisShader = Shader.Find("UI/IrisWipe");
            if (irisShader != null)
            {
                irisMaterial = new Material(irisShader);
                irisMaterial.SetColor(ColorProp, maskColor);
                transitionImage.material = irisMaterial;
            }
            else
            {
                Debug.LogWarning("[SceneTransitionManager] UI/IrisWipe Shader not found, using default image color fallback.");
                transitionImage.color = maskColor;
            }

            // Start fully open (transparent/visible)
            SetIrisRadius(1.3f);
            transitionImage.raycastTarget = false;
        }

        /// <summary>
        /// Loads scene by name with an Iris Wipe transition effect.
        /// </summary>
        public void LoadScene(string sceneName, Action onSceneLoaded = null)
        {
            if (isTransitioning) return;
            StartCoroutine(CoTransitionScene(sceneName, -1, onSceneLoaded));
        }

        /// <summary>
        /// Loads scene by index with an Iris Wipe transition effect.
        /// </summary>
        public void LoadScene(int sceneIndex, Action onSceneLoaded = null)
        {
            if (isTransitioning) return;
            StartCoroutine(CoTransitionScene(null, sceneIndex, onSceneLoaded));
        }

        private IEnumerator CoTransitionScene(string sceneName, int sceneIndex, Action onSceneLoaded)
        {
            isTransitioning = true;
            if (transitionImage != null) transitionImage.raycastTarget = true;

            // Step 1: Iris Wipe Close (Circle shrinks 1.3 -> 0.0)
            yield return StartCoroutine(CoAnimateIris(1.3f, 0.0f, transitionDuration));

            // Step 2: Asynchronously load target scene
            AsyncOperation asyncOperation;
            if (!string.IsNullOrEmpty(sceneName))
            {
                asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            }
            else
            {
                asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
            }

            if (asyncOperation != null)
            {
                while (!asyncOperation.isDone)
                {
                    yield return null;
                }
            }

            // Step 3: Invoke scene initialization callback
            onSceneLoaded?.Invoke();

            // Wait 1 frame for camera/scene setup
            yield return null;

            // Step 4: Iris Wipe Open (Circle expands 0.0 -> 1.3)
            yield return StartCoroutine(CoAnimateIris(0.0f, 1.3f, transitionDuration));

            if (transitionImage != null) transitionImage.raycastTarget = false;
            isTransitioning = false;
        }

        private IEnumerator CoAnimateIris(float startRadius, float endRadius, float duration)
        {
            float elapsed = 0f;
            SetIrisRadius(startRadius);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                float currentRadius = Mathf.Lerp(startRadius, endRadius, smoothT);
                SetIrisRadius(currentRadius);
                yield return null;
            }

            SetIrisRadius(endRadius);
        }

        private void SetIrisRadius(float radius)
        {
            if (irisMaterial != null)
            {
                irisMaterial.SetFloat(RadiusProp, radius);
            }
            else if (transitionImage != null)
            {
                // Fallback for default Image color if shader is missing
                Color c = transitionImage.color;
                c.a = Mathf.Clamp01(1f - radius);
                transitionImage.color = c;
            }
        }
    }
}
