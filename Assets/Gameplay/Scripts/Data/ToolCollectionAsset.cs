using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Scripts.Data
{
    [CreateAssetMenu(fileName = "NewToolCollection", menuName = "Tool Collection/Asset", order = 0)]
    public class ToolCollectionAsset : ScriptableObject
    {
        [SerializeField]
        private List<ToolData> tools = new List<ToolData>();

        public List<ToolData> Tools => tools;
    }
}
