using System;
using System.Collections.Generic;
using Gameplay.Scripts.Enum;
using UnityEngine;

namespace Gameplay.Scripts.Data
{
    [Serializable]
    public class FoodNodeData
    {
        [SerializeField] protected string id;
        [SerializeField] protected bool enabled = true;
        [SerializeField] protected string name;
        [SerializeField] protected Sprite sprite;
        [SerializeField] protected List<FoodNodeData> parents = new List<FoodNodeData>();
        [SerializeField] protected List<FoodNodeData> children = new List<FoodNodeData>();
        [SerializeField] protected int turnsToCreate = 0;
        [SerializeField] protected int baseBuyInCost = -1;
        [SerializeField] protected int autoTransformIn = -1;
        [SerializeField] protected FoodNodeData autoTransform = null;
        [SerializeField] protected int autoDestroyIn = -1;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public Sprite Sprite
        {
            get => sprite;
            set => sprite = value;
        }

        public List<FoodNodeData> Parents
        {
            get => parents;
            set => parents = value;
        }

        public List<FoodNodeData> Children
        {
            get => children;
            set => children = value;
        }

        public int TurnsToCreate
        {
            get => turnsToCreate;
            set => turnsToCreate = value;
        }

        public int BaseBuyInCost
        {
            get => baseBuyInCost;
            set => baseBuyInCost = value;
        }

        public int AutoTransformIn
        {
            get => autoTransformIn;
            set => autoTransformIn = value;
        }

        public int AutoDestroyIn
        {
            get => autoDestroyIn;
            set => autoDestroyIn = value;
        }

        public FoodNodeData AutoTransform
        {
            get => autoTransform;
            set => autoTransform = value;
        }

        public string ID
        {
            get => id;
            set => id = value;
        }

        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }
    }
}