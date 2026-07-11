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
        protected int maximumInteractingFoodPerTurn = 1;

        public List<ToolData> Tools
        {
            get => tools;
            set => tools = value;
        }

        public int MaximumInteractingFoodPerTurn
        {
            get => maximumInteractingFoodPerTurn;
            set => maximumInteractingFoodPerTurn = value;
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
            get => maximumRewindTurns;
        }

        public ChefData Clone()
        {
            return new ChefData()
            {
                tools = this.tools,
                maximumInteractingFoodPerTurn = this.maximumInteractingFoodPerTurn,
                createdAtTurn = this.createdAtTurn
            };
        }
    }
}