using System.Collections.Generic;
using Gameplay.Scripts.Controller;
using Gameplay.Scripts.Data;
using UnityEngine;

namespace Gameplay.Scripts.UI
{
    public class ChooseIngredientPanel : Singleton<ChooseIngredientPanel>
    {
        [Header("Ingredients")]
        [SerializeField] private RectTransform ingredientContent;
        [SerializeField] private RectTransform ingredientPoolRoot;
        [SerializeField] private IngredientCard ingredientCardPrefab;

        [Header("Available Foods")]
        [SerializeField] private RectTransform availableFoodContent;
        [SerializeField] private RectTransform availableFoodPoolRoot;
        [SerializeField] private AvailableFoodCard availableFoodCardPrefab;
        
        private ObjectPool<IngredientCard> ingredientPool;
        private ObjectPool<AvailableFoodCard> availableFoodPool;

        private readonly List<IngredientCard> ingredientCards = new();
        private readonly List<AvailableFoodCard> availableFoodCards = new();

        private FoodNodeData foodNodeData;

        private void Awake()
        {
            ingredientPool = new ObjectPool<IngredientCard>(
                ingredientCardPrefab,
                ingredientPoolRoot);

            availableFoodPool = new ObjectPool<AvailableFoodCard>(
                availableFoodCardPrefab,
                availableFoodPoolRoot);
        }

        public void Setup(FoodNodeData nodeData)
        {
            foodNodeData = nodeData;

            Clear();

            // Ingredient cần cho recipe
            foreach (var ingredient in nodeData.Parents)
            {
                var card = ingredientPool.Get(ingredientContent);
                card.Setup(ingredient);
                ingredientCards.Add(card);
            }

            // Toàn bộ food đang có
            foreach (var food in MapController.Instance.GetAllFood())
            {
                var card = availableFoodPool.Get(availableFoodContent);
                card.Setup(food);
                availableFoodCards.Add(card);
            }
        }

        private void Clear()
        {
            foreach (var card in ingredientCards)
                ingredientPool.Release(card);
            ingredientCards.Clear();

            foreach (var card in availableFoodCards)
                availableFoodPool.Release(card);
            availableFoodCards.Clear();
        }
        
        private IngredientCard hoveredIngredient;

        public void OnIngredientPointerEnter(IngredientCard ingredient)
        {
            hoveredIngredient = ingredient;
        }

        public void OnIngredientPointerExit(IngredientCard ingredient)
        {
            if (hoveredIngredient == ingredient)
                hoveredIngredient = null;
        }

        public void OnEndDrag()
        {
            if (!GhostDragController.Instance.IsDragging)
                return;

            if (hoveredIngredient != null)
            {
                var food = GhostDragController.Instance.GetDragData<FoodData>();

                if (hoveredIngredient.CanAccept(food))
                {
                    hoveredIngredient.Enable(food);
                }
            }

            GhostDragController.Instance.EndDrag();
        }

        public void ClearSelection()
        {
            foreach (var ingredient in ingredientCards)
                ingredient.Disable();
        }

        public void Cook()
        {
            List<FoodData> ingredients = new();

            foreach (var ingredientCard in ingredientCards)
            {
                if (ingredientCard.SelectedFood == null)
                    return; // Chưa chọn đủ nguyên liệu

                ingredients.Add(ingredientCard.SelectedFood);
            }

            int currentTurn = GameController.Instance.CurrentTurn;

            bool canCook = ChefController.Instance.Interact(
                foodNodeData,
                ingredients,
                currentTurn);

            if (canCook)
            {
                // Có thể đóng panel hoặc reset lựa chọn
                // gameObject.SetActive(false);
                // ClearSelection();
            }
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