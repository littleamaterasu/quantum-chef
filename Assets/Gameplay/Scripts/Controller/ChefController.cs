using System.Collections.Generic;
using System.Linq;
using Gameplay.Scripts.Data;
using Gameplay.Scripts.Enum;
using Gameplay.Scripts.Event;
using Gameplay.Scripts.Utility;
using Unity.VisualScripting;

namespace Gameplay.Scripts.Controller
{
    public class ChefController : Singleton<ChefController>
    {
        protected ChefData chefData;

        public ChefData GetChefData()
        {
            return chefData;
        }

        public void SetChefData(ChefData chefData)
        {
            this.chefData = chefData;
        }

        public void Setup(ChefData chefData)
        {
        }

        public bool Interact(FoodNodeData creatingFood, List<FoodData> ingredients, int currentTurn)
        {
            // check thanh phan
            if (ingredients.Any(foodData => !creatingFood.Parents.Contains(foodData.BaseFood)))
            {
                return false;
            }

            // tao temp data
            var createdInTurn = currentTurn + creatingFood.TurnsToCreate;
            var newFoodData = FoodUtility.CreateFood(creatingFood, createdInTurn);

            // check kha nang nau
            bool interactionResult = EventBus.Request<CreatingFoodEvent, bool>(new CreatingFoodEvent()
            {
                targetFood = newFoodData,
                ingredients = ingredients,
                maximumCapacity = chefData.MaximumInteractingFoodPerTurn
            });

            return interactionResult;
        }

        public int GetMaximumRewindTurn()
        {
            return GetChefData().MaximumRewindTurns;
        }

        public ChefData UpdateTurn(int currentTurn)
        {
            var nextTurn = currentTurn + 1;
            var newChefData = chefData.Clone();
            newChefData.CreatedAtTurn = nextTurn;
            return newChefData;
        }
    }
}