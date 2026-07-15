using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Scripts.Data;

namespace Gameplay.Scripts.Controller
{
    public class CustomerController : Singleton<CustomerController>
    {
        protected CustomerData customerData;

        // Number of customers per difficulty
        private const int EASY_CUSTOMER_COUNT = 5;
        private const int NORMAL_CUSTOMER_COUNT = 7;
        private const int HARD_CUSTOMER_COUNT = 9;

        // Number of required foods per customer per difficulty
        private const int EASY_MIN_FOODS = 1;
        private const int EASY_MAX_FOODS = 1;
        private const int NORMAL_MIN_FOODS = 1;
        private const int NORMAL_MAX_FOODS = 2;
        private const int HARD_MIN_FOODS = 1;
        private const int HARD_MAX_FOODS = 3;

        public void SetCustomerData(CustomerData customerData)
        {
            this.customerData = customerData;
        }

        public CustomerData UpdateTurn(int currentTurn)
        {
            customerData = customerData.Clone();
            return customerData;
        }

        /// <summary>
        /// Generates a randomized list of customers for the given game mode.
        /// Each customer spawns at a random turn and requires leaf-node foods from the FoodGraph.
        /// </summary>
        /// <param name="gameMode">0 = easy, 1 = normal, 2 = hard</param>
        /// <param name="maxTurn">Maximum turn number (inclusive upper bound for spawn turns)</param>
        /// <param name="allNodes">All FoodNodeData nodes from the FoodGraph</param>
        /// <returns>List of CustomerData sorted by spawnTurn ascending</returns>
        public List<CustomerData> GenerateCustomers(int gameMode, int maxTurn, List<FoodNodeData> allNodes)
        {
            Random rng = new Random();

            int customerCount = GetCustomerCount(gameMode);
            List<FoodNodeData> leafNodes = GetLeafNodes(allNodes);

            if (leafNodes.Count == 0)
                return new List<CustomerData>();

            List<CustomerData> customers = new List<CustomerData>(customerCount);

            for (int i = 0; i < customerCount; i++)
            {
                int spawnTurn = rng.Next(1, maxTurn + 1);
                List<FoodNodeData> requiredFoods = PickRandomFoods(leafNodes, gameMode, rng);

                CustomerData customer = new CustomerData
                {
                    id = Guid.NewGuid().ToString(),
                    spawnTurn = spawnTurn,
                    requireFood = requiredFoods
                };

                customers.Add(customer);
            }

            // Sort by spawn turn ascending
            customers.Sort((a, b) => a.spawnTurn.CompareTo(b.spawnTurn));

            return customers;
        }

        /// <summary>
        /// Returns the number of customers for the given game mode.
        /// </summary>
        private int GetCustomerCount(int gameMode)
        {
            switch (gameMode)
            {
                case 0: return EASY_CUSTOMER_COUNT;
                case 1: return NORMAL_CUSTOMER_COUNT;
                case 2: return HARD_CUSTOMER_COUNT;
                default: return EASY_CUSTOMER_COUNT;
            }
        }

        /// <summary>
        /// Picks a random number of leaf-node foods based on difficulty.
        /// </summary>
        private List<FoodNodeData> PickRandomFoods(List<FoodNodeData> leafNodes, int gameMode, Random rng)
        {
            int minFoods, maxFoods;

            switch (gameMode)
            {
                case 0:
                    minFoods = EASY_MIN_FOODS;
                    maxFoods = EASY_MAX_FOODS;
                    break;
                case 1:
                    minFoods = NORMAL_MIN_FOODS;
                    maxFoods = NORMAL_MAX_FOODS;
                    break;
                case 2:
                    minFoods = HARD_MIN_FOODS;
                    maxFoods = HARD_MAX_FOODS;
                    break;
                default:
                    minFoods = EASY_MIN_FOODS;
                    maxFoods = EASY_MAX_FOODS;
                    break;
            }

            int foodCount = rng.Next(minFoods, maxFoods + 1);
            List<FoodNodeData> selectedFoods = new List<FoodNodeData>(foodCount);

            for (int i = 0; i < foodCount; i++)
            {
                int index = rng.Next(leafNodes.Count);
                selectedFoods.Add(leafNodes[index]);
            }

            return selectedFoods;
        }

        /// <summary>
        /// Extracts all leaf nodes from the FoodGraph.
        /// A leaf node is a node with no children.
        /// </summary>
        private List<FoodNodeData> GetLeafNodes(List<FoodNodeData> allNodes)
        {
            return allNodes
                .Where(node => node.Children == null || node.Children.Count == 0)
                .ToList();
        }
    }
}