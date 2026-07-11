using System;
using Gameplay.Scripts.Data;
using Gameplay.Scripts.Event;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Scripts.UI
{
    public class IngredientCard: MonoBehaviour
    {
        public Image image;
        protected FoodNodeData selfFoodNode;

        public void Setup(FoodNodeData selfFoodNode)
        {
            this.selfFoodNode = selfFoodNode;
        }
    }
}