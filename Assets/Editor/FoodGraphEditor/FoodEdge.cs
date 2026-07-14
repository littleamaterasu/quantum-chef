using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.FoodGraphEditor
{
    public class FoodEdge : Edge
    {
        public FoodEdge()
        {
            // Custom styling for culinary orange edges
            style.color = new StyleColor(new Color(0.9f, 0.45f, 0.25f, 0.85f));
        }
    }
}
