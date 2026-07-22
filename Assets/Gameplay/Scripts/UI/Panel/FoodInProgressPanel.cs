using System.Collections.Generic;
using Gameplay.Scripts.Data;
using Gameplay.Scripts.Event;
using UnityEngine;

namespace Gameplay.Scripts.UI
{
    public class FoodInProgressPanel : Singleton<FoodInProgressPanel>
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform poolRoot;
        [SerializeField] private InProgressFoodCard foodCardPrefab;

        private readonly Dictionary<FoodData, InProgressFoodCard> foodCards = new();
        private ObjectPool<InProgressFoodCard> pool;

        private void Awake()
        {
            pool = new ObjectPool<InProgressFoodCard>(foodCardPrefab, poolRoot);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<AddCreatingFoodEvent>(OnAddFood);
            EventBus.Subscribe<RemoveCreatingFoodEvent>(OnRemoveFood);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AddCreatingFoodEvent>(OnAddFood);
            EventBus.Unsubscribe<RemoveCreatingFoodEvent>(OnRemoveFood);
        }

        private void OnAddFood(AddCreatingFoodEvent evt)
        {
            if (foodCards.ContainsKey(evt.foodData))
                return;

            var card = pool.Get(content);
            card.Setup(evt.foodData);

            foodCards.Add(evt.foodData, card);
        }

        private void OnRemoveFood(RemoveCreatingFoodEvent evt)
        {
            if (!foodCards.TryGetValue(evt.foodData, out var card))
                return;

            foodCards.Remove(evt.foodData);
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