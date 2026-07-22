using System.Collections.Generic;
using Gameplay.Scripts.Data;

namespace Gameplay.Scripts.Event
{
    public struct UpdateCustomerListEvent
    {
        public List<FoodNodeData> foodNeedToServe;
    }
}