using System.Collections.Generic;
using Gameplay.Scripts.Enum;

namespace Gameplay.Scripts.Data
{
    public class ChefData
    {
        protected int id = 0;
        protected int createdAtTurn = 0;
        protected List<ToolData> tools = new List<ToolData>();
        protected const int maximumRewindTurns = 2;
        protected const int maximumInteractingFoodPerTurn = 1;

        public List<ToolData> Tools
        {
            get => tools;
            set => tools = value;
        }

        public int GetReduceCreateTurn()
        {
            int bonus = 0;
            foreach (var toolData in tools)
            {
                bonus += toolData.ReduceCreateTurn;
            }

            return bonus;
        }

        public int MaximumInteractingFoodPerTurn
        {
            get
            {
                int bonus = 0;
                foreach (var toolData in tools)
                {
                    bonus += toolData.MaximumFoodInteractPerTurnBonus;
                }

                return maximumInteractingFoodPerTurn + bonus;
            }
        }

        public int CreatedAtTurn
        {
            get => createdAtTurn;
            set => createdAtTurn = value;
        }

        public int ID
        {
            get => id;
            set => id = value;
        }

        public int MaximumRewindTurns
        {
            get
            {
                int bonus = 0;
                foreach (var toolData in tools)
                {
                    bonus += toolData.BonusRewindTurn;
                }

                return maximumRewindTurns + bonus;
            }
        }

        public ChefData Clone()
        {
            return new ChefData()
            {
                tools = this.tools,
                createdAtTurn = this.createdAtTurn
            };
        }
    }
}