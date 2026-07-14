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
        [SerializeField] protected int bonusRewindTurn = 0;
        [SerializeField] protected int reduceCreateTurn = 0;

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
            get => bonusRewindTurn;
            set => bonusRewindTurn = value;
        }

        public int ReduceCreateTurn
        {
            get => reduceCreateTurn;
            set => reduceCreateTurn = value;
        }
    }
}