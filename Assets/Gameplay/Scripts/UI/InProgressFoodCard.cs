using Gameplay.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Scripts.UI
{
    public class InProgressFoodCard : MonoBehaviour
    {
        protected FoodData selfFoodData;
        public Image foodImage;
        public TMP_Text turnToFinish;

        public void Setup(FoodData foodData)
        {
            selfFoodData = foodData;
            foodImage.sprite = foodData.BaseFood.Sprite;
        }

        public void UpdateTurn(int turn)
        {
            int remain = selfFoodData.BaseFood.TurnsToCreate - (turn - selfFoodData.CreatedAtTurn);

            turnToFinish.text = Mathf.Max(0, remain).ToString();
        }
    }
}