using Gameplay.Scripts.Data;

namespace Gameplay.Scripts.Controller
{
    public class CustomerController: Singleton<CustomerController>
    {
        protected CustomerData customerData;

        public void SetCustomerData(CustomerData customerData)
        {
            this.customerData = customerData;
        }
        
        public CustomerData UpdateTurn(int currentTurn)
        {
            customerData = customerData.Clone();
            return customerData;
        }
    }
}