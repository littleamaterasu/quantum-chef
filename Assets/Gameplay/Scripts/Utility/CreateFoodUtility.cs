using System;
using Gameplay.Scripts.Data;

namespace Gameplay.Scripts.Utility
{
    public static class FoodUtility
    {
        public static FoodData CreateFood(FoodNodeData target, int createdAtTurn)
        {
            return new FoodData
            {
                Amount = 1,
                BaseFood = target,
                CreatedAtTurn = createdAtTurn,
                ID = Guid.NewGuid().ToString("N") // ví dụ: 4b7d8b6a4f8d4b0d8f3c4b1e2a9c7d6f
            };
        }
    }
}