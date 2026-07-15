using System.Collections.Generic;
using Gameplay.Scripts.Data;
using UnityEngine;

namespace Gameplay.Scripts.Controller
{
    public class AssetController : Singleton<AssetController>
    {
        public FoodGraphAsset FoodGraphAsset;
        public ToolCollectionAsset ToolCollectionAsset;
        public CustomerSpriteCollectionAsset customerSpriteCollectionAsset;

        #region Food

        public List<FoodNodeData> GetFoodNodes()
        {
            return FoodGraphAsset != null
                ? FoodGraphAsset.Nodes
                : new List<FoodNodeData>();
        }

        public FoodNodeData GetFoodNode(int index)
        {
            List<FoodNodeData> nodes = GetFoodNodes();

            if (index < 0 || index >= nodes.Count)
                return null;

            return nodes[index];
        }

        public FoodNodeData GetRandomFoodNode()
        {
            List<FoodNodeData> nodes = GetFoodNodes();

            if (nodes.Count == 0)
                return null;

            return nodes[Random.Range(0, nodes.Count)];
        }

        public List<FoodNodeData> GetLeafNodes()
        {
            List<FoodNodeData> nodes = GetFoodNodes();
            return nodes.FindAll(f => f.Children == null || f.Children.Count == 0);
        }

        public List<FoodNodeData> GetRootNodes()
        {
            List<FoodNodeData> nodes = GetFoodNodes();
            return nodes.FindAll(f => f.Parents == null || f.Parents.Count == 0);
        }

        #endregion

        #region Tool

        public List<ToolData> GetTools()
        {
            return ToolCollectionAsset != null
                ? ToolCollectionAsset.Tools
                : new List<ToolData>();
        }

        public ToolData GetTool(int index)
        {
            List<ToolData> tools = GetTools();

            if (index < 0 || index >= tools.Count)
                return null;

            return tools[index];
        }

        public ToolData GetRandomTool()
        {
            List<ToolData> tools = GetTools();

            if (tools.Count == 0)
                return null;

            return tools[Random.Range(0, tools.Count)];
        }

        #endregion

        #region Customer Sprite

        public List<Sprite> GetAllCustomerSprites()
        {
            return customerSpriteCollectionAsset != null
                ? new List<Sprite>(customerSpriteCollectionAsset.Sprites)
                : new List<Sprite>();
        }

        public Sprite GetCustomerSprite(int index)
        {
            List<Sprite> sprites = GetAllCustomerSprites();

            if (index < 0 || index >= sprites.Count)
                return null;

            return sprites[index];
        }

        public Sprite GetRandomCustomerSprite()
        {
            List<Sprite> sprites = GetAllCustomerSprites();

            if (sprites.Count == 0)
                return null;

            return sprites[Random.Range(0, sprites.Count)];
        }

        #endregion
    }
}