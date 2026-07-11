using System;
using UnityEngine;

namespace Gameplay.Scripts.Data
{
    [Serializable]
    public class ToolData
    {
        protected int id;
        [SerializeField] protected string name;
        [SerializeField] protected Sprite icon;
        [SerializeField] protected int bonusRewindTurn = 0;
        [SerializeField] protected int reduceCreateTurn = 0;
    }
}