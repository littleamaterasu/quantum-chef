using System.Collections.Generic;
using System.Linq;
using Gameplay.Scripts.Data;
using Gameplay.Scripts.Event;
using Gameplay.Scripts.Utility;
using AddFoodEvent = Gameplay.Scripts.Event.AddFoodEvent;

namespace Gameplay.Scripts.Controller
{
    public class MapController : Singleton<MapController>
    {
        protected MapData mapData;

        public MapData MapData
        {
            get => mapData;
            set => mapData = value;
        }

        public void SetMapData(MapData mapData)
        {
            this.mapData = mapData;
            EventBus.UnsubscribeRequest<CreatingFoodEvent>();
            EventBus.SubscribeRequest<CreatingFoodEvent, bool>(TryCreatingFood);
            EventBus.Publish(new UpdateAvailableFoodListEvent());
            foreach (var foodData in mapData.FoodData)
            {
                AddFood(foodData);
            }
            
            foreach (var foodData in mapData.CreatingFood)
            {
                AddCreatingFood(foodData);
            }
        }

        protected virtual bool TryCreatingFood(CreatingFoodEvent e)
        {
            int usedCount = mapData.UsedFood.Values.Sum(x => x.Count);

            if (usedCount + e.ingredients.Count > e.maximumCapacity)
                return false;

            AddCreatingFood(e.targetFood);

            foreach (var ingredient in e.ingredients)
                RemoveFood(ingredient);

            mapData.UsedFood[e.targetFood] = e.ingredients;

            return true;
        }

        public MapData UpdateTurn(int currentTurn)
        {
            int nextTurn = currentTurn + 1;

            UpdateCreatingFoods(nextTurn);
            UpdateExistingFoods(nextTurn);

            MapData newData = mapData.Clone();
            newData.CreatedAtTurn = nextTurn;
            return newData;
        }

        protected virtual void UpdateCreatingFoods(int nextTurn)
        {
            var completedFoods = mapData.CreatingFood
                .Where(f => f.CreatedAtTurn == nextTurn)
                .ToList();

            foreach (var food in completedFoods)
                FinishCreatingFood(food);
        }

        protected virtual void UpdateExistingFoods(int nextTurn)
        {
            var foods = mapData.FoodData.ToList();

            foreach (var food in foods)
            {
                if (ShouldDestroy(food, nextTurn))
                {
                    RemoveFood(food);
                    continue;
                }

                if (ShouldTransform(food, nextTurn))
                {
                    TransformFood(food, nextTurn);
                }
            }
        }

        protected virtual bool ShouldDestroy(FoodData food, int nextTurn)
        {
            return food.BaseFood.AutoDestroyIn != -1 &&
                   food.CreatedAtTurn + food.BaseFood.AutoDestroyIn <= nextTurn;
        }

        protected virtual bool ShouldTransform(FoodData food, int nextTurn)
        {
            return food.BaseFood.AutoTransformIn != -1 &&
                   food.CreatedAtTurn + food.BaseFood.AutoTransformIn <= nextTurn;
        }

        protected virtual void FinishCreatingFood(FoodData food)
        {
            mapData.UsedFood.Remove(food);

            RemoveCreatingFood(food);
            AddFood(food);
        }

        protected virtual void TransformFood(FoodData food, int nextTurn)
        {
            FoodData transformed =
                FoodUtility.CreateFood(food.BaseFood.AutoTransform, nextTurn);

            RemoveFood(food);
            AddFood(transformed);
        }

        public virtual void AddFood(FoodData food)
        {
            mapData.FoodData.Add(food);

            EventBus.Publish(new AddFoodEvent
            {
                foodData = food
            });
        }

        protected virtual void RemoveFood(FoodData food)
        {
            mapData.FoodData.Remove(food);

            EventBus.Publish(new RemoveFoodEvent
            {
                foodData = food
            });
        }

        protected virtual void AddCreatingFood(FoodData food)
        {
            mapData.CreatingFood.Add(food);

            EventBus.Publish(new AddCreatingFoodEvent
            {
                foodData = food
            });
        }

        protected virtual void RemoveCreatingFood(FoodData food)
        {
            mapData.CreatingFood.Remove(food);

            EventBus.Publish(new RemoveCreatingFoodEvent
            {
                foodData = food
            });
        }

        public List<FoodData> GetAllFood()
        {
            return mapData.FoodData;
        }
    }
}