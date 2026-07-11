using System.Collections.Generic;
using Gameplay.Scripts.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Scripts.UI
{
    public class FoodNeedToServeCard : MonoBehaviour
    {
        [Header("Food")]
        [SerializeField] private Image foodImage;

        [Header("Ingredients")]
        [SerializeField] private RectTransform ingredientContent;
        [SerializeField] private RectTransform ingredientPoolRoot;
        [SerializeField] private IngredientCard ingredientCardPrefab;

        private readonly List<IngredientCard> ingredientCards = new();
        private ObjectPool<IngredientCard> ingredientPool;

        private FoodNodeData foodNodeData;

        private void Awake()
        {
            ingredientPool = new ObjectPool<IngredientCard>(
                ingredientCardPrefab,
                ingredientPoolRoot);
        }

        public void Setup(FoodNodeData nodeData)
        {
            foodNodeData = nodeData;

            foodImage.sprite = nodeData.Sprite;

            ClearIngredients();

            foreach (var ingredient in nodeData.Parents)
            {
                var card = ingredientPool.Get(ingredientContent);
                card.Setup(ingredient);

                ingredientCards.Add(card);
            }
        }

        private void ClearIngredients()
        {
            foreach (var card in ingredientCards)
                ingredientPool.Release(card);

            ingredientCards.Clear();
        }
    }
}