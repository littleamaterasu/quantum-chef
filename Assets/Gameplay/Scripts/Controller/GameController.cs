using System;
using System.Collections.Generic;
using Gameplay.Scripts.Data;
using Gameplay.Scripts.Utility;
using UnityEditor;

namespace Gameplay.Scripts.Controller
{
    public class GameController : Singleton<GameController>
    {
        protected MapController mapController;
        protected ChefController chefController;
        protected CustomerController customerController;

        private void Start()
        {
            mapController = MapController.Instance;
            chefController = ChefController.Instance;
            customerController = CustomerController.Instance;
        }

        // ---------- World History ----------
        protected readonly List<MapData> previousMaps = new();
        protected readonly List<ChefData> previousChefs = new();

        // ---------- Customer History ----------
        protected readonly List<AllCustomerData> previousCustomers = new();

        protected int currentTurn = 0;
        protected int currentCustomerTurn = 0;

        // 24 hours
        protected const int MAX_TURN = 24;

        public void StartGame(LevelData levelData)
        {
            currentTurn = 0;
            currentCustomerTurn = 0;

            previousMaps.Clear();
            previousChefs.Clear();
            previousCustomers.Clear();

            List<ToolData> tools = AssetController.Instance != null ? AssetController.Instance.GetTools() : new List<ToolData>();
            ChefData chef = new ChefData()
            {
                ID = Guid.NewGuid().ToString(),
                CreatedAtTurn = 0,
                Tools = tools
            };
            chefController.SetChefData(chef);

            List<FoodData> initialFoods = new List<FoodData>();
            if (levelData != null && levelData.initialFoodNodes != null)
            {
                foreach (var foodNodeData in levelData.initialFoodNodes)
                {
                    if (foodNodeData != null)
                    {
                        initialFoods.Add(FoodUtility.CreateFood(foodNodeData, 0));
                    }
                }
            }

            MapData initialMap = new MapData()
            {
                ID = Guid.NewGuid().ToString(),
                CreatedAtTurn = 0,
                FoodData = initialFoods,
                CreatingFood = new List<FoodData>(),
                UsedFood = new Dictionary<FoodData, List<FoodData>>()
            };
            mapController.SetMapData(initialMap);

            if (levelData != null && levelData.allCustomerData != null)
            {
                customerController.SetCustomerData(levelData.allCustomerData);
            }
            else
            {
                customerController.SetCustomerData(new AllCustomerData());
            }
        }
        
        public void ExecuteTurn()
        {
            ExecuteWorldTurn();
            ExecuteCustomerTurn();
        }

        #region Execute

        protected void ExecuteWorldTurn()
        {
            MapData map = mapController.UpdateTurn(currentTurn);
            ChefData chef = chefController.UpdateTurn(currentTurn);

            previousMaps.Add(map);
            previousChefs.Add(chef);

            TrimWorldHistory();
            currentTurn++;
        }

        protected void ExecuteCustomerTurn()
        {
            AllCustomerData customer = customerController.UpdateTurn(currentCustomerTurn);

            previousCustomers.Add(customer);

            TrimCustomerHistory();
            currentCustomerTurn++;
        }

        /// <summary>
        /// Kết thúc game/level hiện tại, tính toán số sao và thưởng coin dựa trên % số lượng khách đã phục vụ thành công.
        /// </summary>
        public GameFinishResult FinishCurrentGame(int levelIndex)
        {
            int served = customerController.GetServedCustomerCount();
            int total = customerController.GetTotalCustomerCount();

            return UserDataManager.Instance.FinishGame(levelIndex, served, total);
        }

        #endregion

        #region Trim

        protected void TrimWorldHistory()
        {
            int max = chefController.GetMaximumRewindTurn();

            while (previousMaps.Count > max)
            {
                previousMaps.RemoveAt(0);
                previousChefs.RemoveAt(0);
            }
        }

        protected void TrimCustomerHistory()
        {
            int max = chefController.GetMaximumRewindTurn();

            while (previousCustomers.Count > max)
            {
                previousCustomers.RemoveAt(0);
            }
        }

        #endregion

        #region Rewind

        /// <summary>
        /// Trả về danh sách readonly các MapData lịch sử để RewindMapPopup hiển thị.
        /// </summary>
        public IReadOnlyList<MapData> GetPreviousMaps()
        {
            return previousMaps;
        }

        /// <summary>
        /// Xóa các FoodData được chọn khỏi tất cả các MapData lịch sử,
        /// cập nhật createdAtTurn về turn hiện tại, rồi thêm vào map hiện tại.
        /// </summary>
        public void ConfirmRewindFood(List<FoodData> selectedFoods)
        {
            if (selectedFoods == null || selectedFoods.Count == 0)
                return;

            // Xóa khỏi các map lịch sử theo ID
            HashSet<string> selectedIds = new HashSet<string>();
            foreach (var food in selectedFoods)
                selectedIds.Add(food.ID);

            foreach (var map in previousMaps)
            {
                map.FoodData.RemoveAll(f => selectedIds.Contains(f.ID));
            }

            // Cập nhật createdAtTurn và thêm vào map hiện tại
            foreach (var food in selectedFoods)
            {
                food.CreatedAtTurn = currentTurn;
                mapController.AddFood(food);
            }
        }

        public void RewindCustomer(int turn)
        {
            if (turn <= 0)
                return;

            turn = System.Math.Min(turn, chefController.GetMaximumRewindTurn());

            if (turn > previousCustomers.Count)
                return;

            int targetIndex = previousCustomers.Count - turn;

            customerController.SetCustomerData(previousCustomers[targetIndex]);

            previousCustomers.RemoveRange(targetIndex, turn);
            currentCustomerTurn -= turn;
        }

        #endregion

        #region Skip

        public void SkipWorld(int turn)
        {
            if (turn <= 0)
                return;

            for (int i = 0; i < turn; i++)
            {
                ExecuteWorldTurn();
                currentTurn++;
            }
        }

        public void SkipCustomer(int turn)
        {
            if (turn <= 0)
                return;

            for (int i = 0; i < turn; i++)
            {
                ExecuteCustomerTurn();
                currentCustomerTurn++;
            }
        }

        #endregion

        public int CurrentTurn
        {
            get => currentTurn;
            set => currentTurn = value;
        }

        public int CurrentCustomerTurn
        {
            get => currentCustomerTurn;
            set => currentCustomerTurn = value;
        }
    }
}