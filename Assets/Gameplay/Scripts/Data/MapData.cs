using System.Collections.Generic;

namespace Gameplay.Scripts.Data
{
    public class MapData
    {
        protected string id = "";
        protected int createdAtTurn = 0;
        protected List<FoodData> foodData = new();
        protected Dictionary<FoodData, List<FoodData>> usedFood = new();
        protected List<FoodData> creatingFood = new();

        public string ID
        {
            get => id;
            set => id = value;
        }

        public int CreatedAtTurn
        {
            get => createdAtTurn;
            set => createdAtTurn = value;
        }

        public List<FoodData> FoodData
        {
            get => foodData;
            set => foodData = value;
        }

        public Dictionary<FoodData, List<FoodData>> UsedFood
        {
            get => usedFood;
            set => usedFood = value;
        }

        public List<FoodData> CreatingFood
        {
            get => creatingFood;
            set => creatingFood = value;
        }

        public MapData Clone()
        {
            MapData clone = (MapData)MemberwiseClone();

            var foodMap = new Dictionary<FoodData, FoodData>();

            clone.foodData = new List<FoodData>(foodData.Count);

            foreach (var food in foodData)
            {
                var newFood = food.Clone();
                clone.foodData.Add(newFood);
                foodMap[food] = newFood;
            }

            clone.creatingFood = creatingFood.ConvertAll(f => foodMap[f]);

            clone.usedFood = new Dictionary<FoodData, List<FoodData>>();

            foreach (var pair in usedFood)
            {
                clone.usedFood.Add(
                    foodMap[pair.Key],
                    pair.Value.ConvertAll(f => foodMap[f]));
            }

            return clone;
        }
    }
}