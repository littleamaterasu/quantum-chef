using System.Collections.Generic;

namespace Gameplay.Scripts.Data
{
    public static class FoodGraphLoader
    {
        /// <summary>
        /// Loads and reconstructs a list of connected FoodNodeData elements from a FoodGraphAsset.
        /// </summary>
        public static List<FoodNodeData> Load(FoodGraphAsset asset)
        {
            if (asset == null)
            {
                return new List<FoodNodeData>();
            }

            // Reconstruct the graph's reference connections to be safe and return the connected nodes list.
            asset.ReconstructGraph();
            return asset.Nodes;
        }
    }
}
