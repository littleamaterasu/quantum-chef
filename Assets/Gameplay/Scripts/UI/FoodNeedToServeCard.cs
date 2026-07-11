using System.Collections.Generic;
using Gameplay.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Scripts.UI
{
    public class FoodNeedToServeCard : MonoBehaviour
    {
        [Header("Food")]
        [SerializeField] protected Image foodImage;
        [SerializeField] protected TMP_Text turnToFinishTMP;

        [Header("Ingredients")]
        [SerializeField] protected RectTransform ingredientContent;
        [SerializeField] protected RectTransform ingredientPoolRoot;
        [SerializeField] protected IngredientCard ingredientCardPrefab;

        protected readonly List<IngredientCard> ingredientCards = new();
        protected ObjectPool<IngredientCard> ingredientPool;

        protected FoodNodeData foodNodeData;

        protected void Awake()
        {
            ingredientPool = new ObjectPool<IngredientCard>(
                ingredientCardPrefab,
                ingredientPoolRoot);
        }

        public void Setup(FoodNodeData nodeData)
        {
            foodNodeData = nodeData;

            foodImage.sprite = nodeData.Sprite;
            turnToFinishTMP.text = nodeData.TurnsToCreate.ToString();

            ClearIngredients();

            foreach (var ingredient in nodeData.Parents)
            {
                var card = ingredientPool.Get(ingredientContent);
                card.Setup(ingredient);

                ingredientCards.Add(card);
            }
        }

        protected void ClearIngredients()
        {
            foreach (var card in ingredientCards)
                ingredientPool.Release(card);

            ingredientCards.Clear();
        }
    }
}