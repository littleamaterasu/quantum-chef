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

        private readonly Dictionary<FoodData, AvailableFoodCard> foodCards = new();
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

        private void OnAddFood(AddFoodEvent evt)
        {
            if (foodCards.ContainsKey(evt.foodData))
                return;

            var card = pool.Get(content);
            card.Setup(evt.foodData);

            foodCards.Add(evt.foodData, card);
        }

        private void OnRemoveFood(RemoveFoodEvent evt)
        {
            if (!foodCards.Remove(evt.foodData, out var card))
                return;

            pool.Release(card);
        }
    }
}