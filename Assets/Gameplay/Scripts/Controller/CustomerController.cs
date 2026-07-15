using System;
using System.Collections.Generic;
using Gameplay.Scripts.Data;

namespace Gameplay.Scripts.Controller
{
    public class CustomerController : Singleton<CustomerController>
    {
        protected AllCustomerData allCustomerData;

        protected ObjectPool<Customer> customerPool;

        // Customer đang active trên scene
        protected readonly Dictionary<string, Customer> customers = new();

        // Lookup CustomerData theo id
        protected readonly Dictionary<string, CustomerData> customerDataMap = new();

        // ==========================================================
        // CONFIG
        // ==========================================================

        private const int EASY_CUSTOMER_COUNT = 5;
        private const int NORMAL_CUSTOMER_COUNT = 7;
        private const int HARD_CUSTOMER_COUNT = 9;

        private const int START_SERVE_TIME = 6;

        private const int EASY_MIN_FOODS = 1;
        private const int EASY_MAX_FOODS = 1;

        private const int NORMAL_MIN_FOODS = 1;
        private const int NORMAL_MAX_FOODS = 2;

        private const int HARD_MIN_FOODS = 1;
        private const int HARD_MAX_FOODS = 3;

        // ==========================================================

        public void SetCustomerData(AllCustomerData allCustomerData)
        {
            this.allCustomerData = allCustomerData;
        }

        public AllCustomerData UpdateTurn(int currentTurn)
        {
            allCustomerData = allCustomerData.Clone();

            // Rebuild toàn bộ customer trên scene
            foreach (Customer customer in customers.Values)
            {
                customerPool.Release(customer);
            }

            customers.Clear();
            customerDataMap.Clear();

            foreach (CustomerData data in allCustomerData.allCustomerData)
            {
                // Khách turn trước chưa được serve
                if (data.spawnTurn == currentTurn - 1 && !data.served)
                {
                    Customer customer = customerPool.Get(null);
                    customer.Setup(data);
                    customer.OnServeFail();

                    // nếu OnServeFail() không tự release thì:
                    customerPool.Release(customer);

                    continue;
                }

                // Khách xuất hiện ở turn hiện tại
                if (data.spawnTurn == currentTurn && !data.served)
                {
                    Customer customer = customerPool.Get(null);

                    customer.Setup(data);

                    customers[data.id] = customer;
                    customerDataMap[data.id] = data;
                }
            }

            allCustomerData.createdAtTurn = currentTurn;

            return allCustomerData;
        }

        public void Serve(CustomerData customerData)
        {
            if (customerData == null)
                return;

            if (!customers.TryGetValue(customerData.id, out Customer customer))
                return;

            if (!customerDataMap.TryGetValue(customerData.id, out CustomerData data))
                return;

            if (data.served)
                return;

            data.served = true;

            customer.OnServe();

            customers.Remove(data.id);
            customerDataMap.Remove(data.id);

            // nếu OnServe() không tự release thì:
            customerPool.Release(customer);
        }

        // ==========================================================

        public AllCustomerData GenerateCustomers(int gameMode, int maxTurn, List<FoodNodeData> edibleFood)
        {
            Random rng = new Random();

            int customerCount = GetCustomerCount(gameMode);

            if (edibleFood.Count == 0)
                return new AllCustomerData();

            List<CustomerData> customers = new(customerCount);

            for (int i = 0; i < customerCount; i++)
            {
                int spawnTurn = rng.Next(START_SERVE_TIME, maxTurn);

                List<FoodNodeData> requiredFoods =
                    PickRandomFoods(edibleFood, gameMode, rng);

                customers.Add(new CustomerData
                {
                    id = Guid.NewGuid().ToString(),
                    spawnTurn = spawnTurn,
                    requireFood = requiredFoods,
                    served = false,
                    sprite = AssetController.Instance.GetRandomCustomerSprite()
                });
            }

            customers.Sort((a, b) => a.spawnTurn.CompareTo(b.spawnTurn));

            return new AllCustomerData
            {
                allCustomerData = customers
            };
        }

        // ==========================================================

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

        private List<FoodNodeData> PickRandomFoods(
            List<FoodNodeData> leafNodes,
            int gameMode,
            Random rng)
        {
            int minFoods;
            int maxFoods;

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

            List<FoodNodeData> result = new(foodCount);

            for (int i = 0; i < foodCount; i++)
            {
                result.Add(leafNodes[rng.Next(leafNodes.Count)]);
            }

            return result;
        }
    }
}