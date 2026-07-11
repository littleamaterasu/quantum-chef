using System.Collections.Generic;
using Gameplay.Scripts.Data;

namespace Gameplay.Scripts.Event
{
    public struct CreatingFoodEvent
    {
        public FoodData targetFood;
        public List<FoodData> ingredients;
        public int maximumCapacity;
    }
}