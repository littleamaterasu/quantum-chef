using System;

namespace Editor.FoodGraphEditor
{
    public static class FoodGraphUtility
    {
        /// <summary>
        /// Generates a unique string ID for nodes in the graph.
        /// </summary>
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
