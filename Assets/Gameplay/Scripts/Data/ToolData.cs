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
        /// <summary>
        /// The bundle this tool belongs to. Empty string means Base Game (always unlocked).
        /// </summary>
        [SerializeField] protected string bundleId = "";

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

        /// <summary>
        /// The bundle ID that unlocks this tool. Empty = Base Game (always available).
        /// </summary>
        public string BundleId
        {
            get => bundleId;
            set => bundleId = value;
        }
    }
}
