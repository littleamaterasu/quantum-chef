using System.Collections.Generic;
using Gameplay.Scripts.Data;
using Gameplay.Scripts.Event;
using UnityEngine;

namespace Gameplay.Scripts.UI
{
    public class AvailableFoodPanel : MonoBehaviour
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
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AddFoodEvent>(OnAddFood);
            EventBus.Unsubscribe<RemoveFoodEvent>(OnRemoveFood);
        }

        /// <summary>
        /// Rebuild toàn bộ UI từ state hiện tại.
        /// Gọi sau khi rewind hoặc UpdateTurn().
        /// </summary>
        public void Refresh(List<FoodData> foods)
        {
            foreach (AvailableFoodCard card in foodCards.Values)
            {
                pool.Release(card);
            }

            foodCards.Clear();

            foreach (FoodData food in foods)
            {
                AvailableFoodCard card = pool.Get(content);
                card.Setup(food);

                foodCards.Add(food.ID, card);
            }
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
    }
}