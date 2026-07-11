using System.Collections.Generic;

namespace Gameplay.Scripts.Data
{
    public class FoodData
    {
        protected string id = "";
        protected FoodNodeData baseFood = null;
        protected int amount = 1;
        protected int createdAtTurn = 0;

        public FoodNodeData BaseFood
        {
            get => baseFood;
            set => baseFood = value;
        }

        public int Amount
        {
            get => amount;
            set => amount = value;
        }

        public int CreatedAtTurn
        {
            get => createdAtTurn;
            set => createdAtTurn = value;
        }

        public string ID
        {
            get => id;
            set => id = value;
        }

        public FoodData Clone()
        {
            return new FoodData
            {
                id = id,
                baseFood = baseFood,
                amount = amount,
                createdAtTurn = createdAtTurn
            };
        }
    }
}