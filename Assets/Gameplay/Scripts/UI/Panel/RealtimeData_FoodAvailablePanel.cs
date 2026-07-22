using System.Collections.Generic;
using Gameplay.Scripts.Data;
using Gameplay.Scripts.Event;
using UnityEngine;

namespace Gameplay.Scripts.UI
{
    public class AvailableFoodPanel : Singleton<AvailableFoodPanel>
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform poolRoot;
        [SerializeField] private AvailableFoodCard foodCardPrefab;

        private readonly Dictionary<string, AvailableFoodCard> foodCards = new();

        private ObjectPool<AvailableFoodCard> pool;

        private void Awake()
        {
            pool = new ObjectPool<AvailableFoodCard>(foodCardPrefab, poolRoot);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<AddFoodEvent>(OnAddFood);
            EventBus.Subscribe<RemoveFoodEvent>(OnRemoveFood);
            EventBus.Subscribe<UpdateAvailableFoodListEvent>(Clear);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AddFoodEvent>(OnAddFood);
            EventBus.Unsubscribe<RemoveFoodEvent>(OnRemoveFood);
            EventBus.Unsubscribe<UpdateAvailableFoodListEvent>(Clear);
        }

        /// <summary>
        /// Rebuild toàn bộ UI từ state hiện tại.
        /// Gọi sau khi rewind hoặc UpdateTurn().
        /// </summary>
        public void Clear(UpdateAvailableFoodListEvent e)
        {
            foreach (AvailableFoodCard card in foodCards.Values)
            {
                pool.Release(card);
            }

            foodCards.Clear();
        }

        private void OnAddFood(AddFoodEvent evt)
        {
            if (foodCards.ContainsKey(evt.foodData.ID))
                return;

            AvailableFoodCard card = pool.Get(content);
            card.Setup(evt.foodData);

            foodCards.Add(evt.foodData.ID, card);
        }

        private void OnRemoveFood(RemoveFoodEvent evt)
        {
            if (!foodCards.Remove(evt.foodData.ID, out AvailableFoodCard card))
                return;

            pool.Release(card);
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