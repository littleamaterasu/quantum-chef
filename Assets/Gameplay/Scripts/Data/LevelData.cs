using System;
using System.Collections.Generic;

namespace Gameplay.Scripts.Data
{
    [Serializable]
    public class LevelData
    {
        public List<FoodNodeData> initialFoodNodes = new List<FoodNodeData>();
        public AllCustomerData allCustomerData = new AllCustomerData();

        public LevelData Clone()
        {
            List<FoodNodeData> newInitialFoodNodes = initialFoodNodes != null
                ? new List<FoodNodeData>(initialFoodNodes)
                : new List<FoodNodeData>();

            return new LevelData
            {
                initialFoodNodes = newInitialFoodNodes,
                allCustomerData = allCustomerData != null ? allCustomerData.Clone() : new AllCustomerData()
            };
        }
    }
}
