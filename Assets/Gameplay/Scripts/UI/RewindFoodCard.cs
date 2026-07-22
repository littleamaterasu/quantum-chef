using Gameplay.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.Scripts.UI
{
    /// <summary>
    /// Card hiển thị một FoodData từ lịch sử trong RewindMapPopup.
    /// Click để toggle select.
    /// </summary>
    public class RewindFoodCard : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image foodImage;
        [SerializeField] private TMP_Text foodNameText;
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private GameObject selectedOverlay;

        public FoodData SelfFoodData { get; private set; }
        public bool IsSelected { get; private set; }

        private RewindMapPopup owner;

        /// <summary>
        /// Khởi tạo card với dữ liệu food và turn mà nó thuộc về.
        /// </summary>
        public void Setup(FoodData foodData, int sourceTurn, RewindMapPopup popup)
        {
            SelfFoodData = foodData;
            owner = popup;
            IsSelected = false;

            foodImage.sprite = foodData.BaseFood.Sprite;
            foodNameText.text = foodData.BaseFood.Name;
            turnText.text = $"Turn {sourceTurn}";

            selectedOverlay.SetActive(false);
        }

        /// <summary>
        /// Toggle trạng thái selected và thông báo cho popup.
        /// </summary>
        public void ToggleSelect()
        {
            IsSelected = !IsSelected;
            selectedOverlay.SetActive(IsSelected);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            owner.OnCardClicked(this);
        }
    }
}
