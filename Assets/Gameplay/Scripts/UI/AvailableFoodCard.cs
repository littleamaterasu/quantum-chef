using Gameplay.Scripts.Controller;
using Gameplay.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.Scripts.UI
{
    public class AvailableFoodCard : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public FoodData selfFoodData;
        public Image foodImage;
        public TMP_Text autoTransformTurnRemain;
        public TMP_Text autoDestroyTurnRemain;

        protected bool isDraggable;

        public void Setup(FoodData foodData, bool isDraggable = false)
        {
            selfFoodData = foodData;
            this.isDraggable = isDraggable;

            foodImage.sprite = selfFoodData.BaseFood.Sprite;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isDraggable)
                return;

            GhostDragController.Instance.BeginDrag(selfFoodData, foodImage.sprite);
        }

        public void OnDrag(PointerEventData eventData)
        {
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDraggable)
                return;

            ChooseIngredientPanel.Instance.OnEndDrag();
        }

        public void UpdateTurn(int turn)
        {
            UpdateTurnRemain(
                autoTransformTurnRemain,
                selfFoodData.BaseFood.AutoTransformIn,
                turn);

            UpdateTurnRemain(
                autoDestroyTurnRemain,
                selfFoodData.BaseFood.AutoDestroyIn,
                turn);
        }

        private void UpdateTurnRemain(TMP_Text text, int maxTurn, int currentTurn)
        {
            if (maxTurn == -1)
            {
                text.gameObject.SetActive(false);
                return;
            }

            int remain = maxTurn - (currentTurn - selfFoodData.CreatedAtTurn);
            text.gameObject.SetActive(true);
            text.text = remain.ToString();
        }
    }
}