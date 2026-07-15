using System.Collections.Generic;
using Gameplay.Scripts.Data;

namespace Gameplay.Scripts.Controller
{
    public class GameController : Singleton<GameController>
    {
        protected MapController mapController;
        protected ChefController chefController;
        protected CustomerController customerController;
        
        // ---------- World History ----------
        protected readonly List<MapData> previousMaps = new();
        protected readonly List<ChefData> previousChefs = new();

        // ---------- Customer History ----------
        protected readonly List<AllCustomerData> previousCustomers = new();

        protected int currentTurn = 0;
        protected int currentCustomerTurn = 0;

        // 24 hours
        protected const int MAX_TURN = 24;

        public void TestData()
        {
            List<FoodNodeData> demoFood = AssetController.Instance.GetRootNodes();
            List<ToolData> tools = AssetController.Instance.GetTools();
            int demoGameMode = 1;
            StartGame(demoGameMode);
        }

        // gameMode: 0-easy, 1-normal, 2-hard
        public void StartGame(int gameMode = 0)
        {
            currentTurn = 0;
            currentCustomerTurn = 0;

            previousMaps.Clear();
            previousChefs.Clear();
            previousCustomers.Clear();

            // Generate randomized customers based on difficulty
            List<FoodNodeData> edibleFood = AssetController.Instance.GetLeafNodes();
            customerController.GenerateCustomers(gameMode, MAX_TURN, edibleFood);
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

        public void RewindWorld(int turn)
        {
            if (turn <= 0)
                return;

            turn = System.Math.Min(turn, chefController.GetMaximumRewindTurn());

            if (turn > previousMaps.Count)
                return;

            int targetIndex = previousMaps.Count - turn;

            mapController.SetMapData(previousMaps[targetIndex]);
            chefController.SetChefData(previousChefs[targetIndex]);

            previousMaps.RemoveRange(targetIndex, turn);
            previousChefs.RemoveRange(targetIndex, turn);
            currentTurn -= turn;
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