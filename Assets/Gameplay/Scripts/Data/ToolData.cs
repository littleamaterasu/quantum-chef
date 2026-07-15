using System;
using UnityEngine;

namespace Gameplay.Scripts.Data
{
    [Serializable]
    public class ToolData
    {
        [SerializeField] protected int id;
        [SerializeField] protected string name;
        [SerializeField] protected Sprite icon;
        [SerializeField] protected int rewindTurnBonus = 0;
        [SerializeField] protected int reduceCreateTurnBonus = 0;
        [SerializeField] protected int maximumFoodInteractPerTurnBonus = 0;

        public int ID
        {
            get => id;
            set => id = value;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public Sprite Icon
        {
            get => icon;
            set => icon = value;
        }

        public int BonusRewindTurn
        {
            get => rewindTurnBonus;
            set => rewindTurnBonus = value;
        }

        public int ReduceCreateTurn
        {
            get => reduceCreateTurnBonus;
            set => reduceCreateTurnBonus = value;
        }

        public int MaximumFoodInteractPerTurnBonus
        {
            get => maximumFoodInteractPerTurnBonus;
            set => maximumFoodInteractPerTurnBonus = value;
        }
    }
}
