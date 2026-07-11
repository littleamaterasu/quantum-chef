using System;
using Gameplay.Scripts.Data;
using Gameplay.Scripts.Event;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.Scripts.UI
{
    public class IngredientCard: MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        public Image image;
        public CanvasGroup canvasGroup;
        protected FoodNodeData selfFoodNode;
        
        protected FoodData selectedFood = null;
        public FoodData SelectedFood { get; private set; }

        public void Setup(FoodNodeData selfFoodNode)
        {
            this.selfFoodNode = selfFoodNode;
        }

        public void Enable(FoodData foodData)
        {
            SelectedFood = foodData;
            canvasGroup.alpha = 1;
        }

        public void Disable()
        {
            canvasGroup.alpha = 0.25f;
        }

        public bool CanAccept(FoodData foodData)
        {
            if (selfFoodNode == null) return false;
            return foodData.BaseFood.Equals(selfFoodNode);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            ChooseIngredientPanel.Instance.OnIngredientPointerEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ChooseIngredientPanel.Instance.OnIngredientPointerExit(this);
        }
    }
}