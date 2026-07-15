using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Scripts.Data
{
    [CreateAssetMenu(fileName = "NewCustomerSpriteCollection", menuName = "Customer Sprite Collection/Asset", order = 0)]
    public class CustomerSpriteCollectionAsset : ScriptableObject
    {
        [SerializeField]
        private List<Sprite> sprites = new();

        public IReadOnlyList<Sprite> Sprites => sprites;
    }
}