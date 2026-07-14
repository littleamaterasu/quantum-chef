using System.Collections.Generic;

namespace Gameplay.Scripts.Data
{
    public static class ToolCollectionLoader
    {
        /// <summary>
        /// Loads and returns the list of ToolData from a ToolCollectionAsset.
        /// </summary>
        public static List<ToolData> Load(ToolCollectionAsset asset)
        {
            if (asset == null)
            {
                return new List<ToolData>();
            }

            return asset.Tools;
        }
    }
}
