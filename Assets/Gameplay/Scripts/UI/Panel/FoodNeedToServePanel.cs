using System.Collections.Generic;
using Gameplay.Scripts.Data;
using Gameplay.Scripts.Event;
using UnityEngine;

namespace Gameplay.Scripts.UI
{
    public class FoodNeedToServePanel : Singleton<FoodNeedToServePanel>
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform poolRoot;
        [SerializeField] private FoodNeedToServeCard foodCardPrefab;

        private readonly List<FoodNeedToServeCard> foodCards = new();
        private ObjectPool<FoodNeedToServeCard> pool;

        private void Awake()
        {
            pool = new ObjectPool<FoodNeedToServeCard>(foodCardPrefab, poolRoot);
            EventBus.Subscribe<UpdateCustomerListEvent>(e => Setup(e.foodNeedToServe));
        }

        public void Setup(List<FoodNodeData> foods)
        {
            Clear();

            foreach (var food in foods)
            {
                var card = pool.Get(content);
                card.Setup(food);

                foodCards.Add(card);
            }
        }

        private void Clear()
        {
            foreach (var card in foodCards)
            {
                pool.Release(card);
            }

            foodCards.Clear();
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