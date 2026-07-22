using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Scripts.Data
{
    public class CustomerData
    {
        public string id;
        public Sprite sprite;
        public List<FoodNodeData> requireFood = null;
        public int spawnTurn = 0;
        public bool served = false;
        public bool successfullyServed = false;
        public CustomerData Clone()
        {
            return new CustomerData()
            {
                id = this.id,
                sprite = this.sprite,
                requireFood = this.requireFood,
                spawnTurn = this.spawnTurn,
                served = this.served,
                successfullyServed = this.successfullyServed
            };
        }
    }
}