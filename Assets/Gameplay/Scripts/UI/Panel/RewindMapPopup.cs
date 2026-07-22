using System.Collections.Generic;
using Gameplay.Scripts.Controller;
using Gameplay.Scripts.Data;
using UnityEngine;

namespace Gameplay.Scripts.UI
{
    /// <summary>
    /// Popup hiển thị tất cả FoodData từ các turn lịch sử (previousMaps).
    /// Người dùng click để select/deselect từng card, rồi Confirm để
    /// rewind các food đó về turn hiện tại.
    /// </summary>
    public class RewindMapPopup : Singleton<RewindMapPopup>
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform poolRoot;
        [SerializeField] private RewindFoodCard rewindFoodCardPrefab;

        private ObjectPool<RewindFoodCard> pool;

        private readonly List<RewindFoodCard> cards = new();
        private readonly List<FoodData> selectedFoods = new();

        private void Awake()
        {
            pool = new ObjectPool<RewindFoodCard>(rewindFoodCardPrefab, poolRoot);
        }

        // ------------------------------------------------------------------ //
        //  Public API
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Mở popup và spawn card cho tất cả FoodData trong previousMaps.
        /// Mỗi card gắn nhãn turn tương ứng với MapData chứa nó.
        /// </summary>
        public void Open()
        {
            ClearCards();
            selectedFoods.Clear();

            IReadOnlyList<MapData> previousMaps = GameController.Instance.GetPreviousMaps();

            for (int i = 0; i < previousMaps.Count; i++)
            {
                MapData mapData = previousMaps[i];

                foreach (FoodData food in mapData.FoodData)
                {
                    RewindFoodCard card = pool.Get(content);
                    card.Setup(food, mapData.CreatedAtTurn, this);
                    cards.Add(card);
                }
            }

            Show();
        }

        /// <summary>
        /// Đóng popup và trả toàn bộ card về pool.
        /// </summary>
        public void Close()
        {
            ClearCards();
            selectedFoods.Clear();
            Hide();
        }

        /// <summary>
        /// Được gọi khi người dùng click vào một RewindFoodCard.
        /// Toggle select trên card và cập nhật danh sách selectedFoods.
        /// </summary>
        public void OnCardClicked(RewindFoodCard card)
        {
            card.ToggleSelect();

            if (card.IsSelected)
                selectedFoods.Add(card.SelfFoodData);
            else
                selectedFoods.Remove(card.SelfFoodData);
        }

        /// <summary>
        /// Xác nhận rewind: gọi GameController để xử lý logic,
        /// sau đó đóng popup.
        /// </summary>
        public void ConfirmRewind()
        {
            GameController.Instance.ConfirmRewindFood(selectedFoods);
            Close();
        }

        // ------------------------------------------------------------------ //
        //  Private helpers
        // ------------------------------------------------------------------ //

        private void ClearCards()
        {
            foreach (RewindFoodCard card in cards)
                pool.Release(card);

            cards.Clear();
        }

        // ------------------------------------------------------------------ //
        //  Show / Hide
        // ------------------------------------------------------------------ //

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
