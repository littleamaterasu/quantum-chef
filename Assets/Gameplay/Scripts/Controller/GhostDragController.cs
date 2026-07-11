using Gameplay.Scripts.Data;
using Gameplay.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Scripts.Controller
{
    public class GhostDragController : Singleton<GhostDragController>
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform ghostTransform;
        [SerializeField] private Image ghostImage;

        private Camera uiCamera;

        public object DragData { get; private set; }

        public bool IsDragging => DragData != null;

        protected override void Awake()
        {
            base.Awake();

            uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            Hide();
        }

        private void Update()
        {
            if (!IsDragging)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                uiCamera,
                out var localPoint);

            ghostTransform.localPosition = localPoint;
        }

        public void BeginDrag<T>(T data, Sprite sprite)
        {
            DragData = data;

            ghostImage.sprite = sprite;
            ghostImage.SetNativeSize();

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;

            gameObject.SetActive(true);
        }

        public T GetDragData<T>()
        {
            return (T)DragData;
        }

        public void EndDrag()
        {
            Hide();
        }

        private void Hide()
        {
            DragData = null;

            ghostImage.sprite = null;

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}