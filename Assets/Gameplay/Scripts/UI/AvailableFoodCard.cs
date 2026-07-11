using Gameplay.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Scripts.UI
{
    public class AvailableFoodCard : MonoBehaviour
    {
        public FoodData selfFoodData;
        public Image foodImage;
        public TMP_Text autoTransformTurnRemain;
        public TMP_Text autoDestroyTurnRemain;

        public void Setup(FoodData foodData)
        {
            selfFoodData = foodData;
            foodImage.sprite = selfFoodData.BaseFood.Sprite;
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