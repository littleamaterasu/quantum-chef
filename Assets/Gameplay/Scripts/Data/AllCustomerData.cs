using System;
using System.Collections.Generic;

namespace Gameplay.Scripts.Data
{
    [Serializable]
    public class AllCustomerData
    {
        public int createdAtTurn = 0;
        public List<CustomerData> allCustomerData;

        public AllCustomerData Clone()
        {
            List<CustomerData> newCustomerData = new List<CustomerData>();

            foreach (CustomerData customerData in allCustomerData)
            {
                newCustomerData.Add(customerData.Clone());
            }

            return new AllCustomerData
            {
                allCustomerData = newCustomerData,
                createdAtTurn = this.createdAtTurn
            };
        }
    }
}