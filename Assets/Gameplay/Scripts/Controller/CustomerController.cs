using System;
using System.Collections.Generic;
using Gameplay.Scripts.Data;
using Gameplay.Scripts.Event;
using UnityEngine;

namespace Gameplay.Scripts.Controller
{
    public class CustomerController : Singleton<CustomerController>
    {
        protected AllCustomerData allCustomerData;

        protected ObjectPool<Customer> customerPool;

        // Customer đang active trên scene
        protected readonly Dictionary<string, Customer> customers = new();

        // Danh sách khách theo thứ tự hàng chờ (index 0 = đứng đầu)
        protected readonly List<Customer> customerQueue = new();

        // Lookup CustomerData theo id
        protected readonly Dictionary<string, CustomerData> customerDataMap = new();

        // ==========================================================
        // QUEUE POSITION CONFIG
        // ==========================================================

        /// <summary>Vị trí đứng đầu hàng – assign qua Inspector.</summary>
        [SerializeField] protected Transform firstPosition;

        /// <summary>
        /// Offset giữa 2 khách liên tiếp trong hàng chéo.
        /// X/Y dịch ngang/dọc, Z tăng để layer đúng trong 2D SpriteRenderer.
        /// </summary>
        [SerializeField] protected Vector3 queueOffset = new Vector3(0.5f, -0.3f, 1f);



        public void SetCustomerData(AllCustomerData allCustomerData)
        {
            this.allCustomerData = allCustomerData;

            HashSet<FoodNodeData> foodNeedToServe = new();
            
            foreach (Customer customer in customers.Values)
            {
                customerPool.Release(customer);
            }
            
            customers.Clear();
            customerDataMap.Clear();
            customerQueue.Clear();

            if (allCustomerData != null)
            {
                foreach (CustomerData customer in allCustomerData.allCustomerData)
                {
                    if (customer.served || customer.requireFood == null)
                        continue;

                    foreach (FoodNodeData food in customer.requireFood)
                    {
                        foodNeedToServe.Add(food);
                    }
                }
            }

            // double loop to seperate logic
            if (allCustomerData != null)
            {
                int queueIndex = 0;
                foreach (CustomerData data in allCustomerData.allCustomerData)
                {
                    if (data.served)
                        continue;

                    if (data.spawnTurn != allCustomerData.createdAtTurn)
                        continue;

                    Customer customer = customerPool.Get(null);
                    customer.Setup(data);
                    customer.transform.position = GetQueuePosition(queueIndex);

                    customers[data.id] = customer;
                    customerDataMap[data.id] = data;
                    customerQueue.Add(customer);

                    queueIndex++;
                }
            }

            EventBus.Publish(new UpdateCustomerListEvent
            {
                foodNeedToServe = new List<FoodNodeData>(foodNeedToServe)
            });
        }

        public AllCustomerData GetAllCustomerData()
        {
            return allCustomerData;
        }

        public AllCustomerData UpdateTurn(int currentTurn)
        {
            allCustomerData = allCustomerData.Clone();
            bool needAdvance = false;

            foreach (CustomerData data in allCustomerData.allCustomerData)
            {
                // Customer quá hạn nhưng chưa được đánh dấu kết thúc
                if (data.spawnTurn < currentTurn && !data.served)
                {
                    data.served = true;

                    bool hasActive = customers.TryGetValue(data.id, out Customer activeCustomer);

                    // Kiểm tra xem khách đã được serve ít nhất 1 lần trong turn chưa
                    if (data.serveCount > 0)
                    {
                        data.successfullyServed = true;

                        if (hasActive)
                        {
                            activeCustomer.OnServe();
                        }
                    }
                    else
                    {
                        data.successfullyServed = false;

                        if (hasActive)
                        {
                            activeCustomer.OnServeFail();
                        }
                        else
                        {
                            Customer tempCustomer = customerPool.Get(null);
                            tempCustomer.Setup(data);
                            tempCustomer.OnServeFail();
                            customerPool.Release(tempCustomer);
                        }
                    }

                    // Release khách đã được served (dù failed hay successful) khỏi active queue/scene
                    if (hasActive)
                    {
                        customers.Remove(data.id);
                        customerDataMap.Remove(data.id);
                        customerQueue.Remove(activeCustomer);
                        customerPool.Release(activeCustomer);
                        needAdvance = true;
                    }
                }

                // Reset serve count cho turn mới
                data.serveCount = 0;
            }

            if (needAdvance)
            {
                AdvanceQueue();
            }

            allCustomerData.createdAtTurn = currentTurn;

            return allCustomerData;
        }

        public void Serve(CustomerData customerData)
        {
            if (customerData == null)
                return;

            if (!customers.TryGetValue(customerData.id, out Customer customer))
                return;

            if (!customerDataMap.TryGetValue(customerData.id, out CustomerData data))
                return;

            if (data.served)
                return;

            // Trong 1 turn khách hàng có thể được serve nhiều lần
            data.serveCount++;

            customer.OnServe();
        }

        /// <summary>
        /// Trả về tổng số lượng khách hàng có trong level hiện tại.
        /// </summary>
        public int GetTotalCustomerCount()
        {
            return allCustomerData != null && allCustomerData.allCustomerData != null
                ? allCustomerData.allCustomerData.Count
                : 0;
        }

        /// <summary>
        /// Trả về số lượng khách hàng đã phục vụ thành công trong level hiện tại.
        /// </summary>
        public int GetServedCustomerCount()
        {
            if (allCustomerData == null || allCustomerData.allCustomerData == null)
                return 0;

            int count = 0;
            foreach (var customer in allCustomerData.allCustomerData)
            {
                if (customer.successfullyServed || customer.serveCount > 0)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Tính vị trí thế giới của khách thứ <paramref name="index"/> trong hàng.
        /// </summary>
        protected Vector3 GetQueuePosition(int index)
        {
            Vector3 origin = firstPosition != null ? firstPosition.position : Vector3.zero;
            return origin + queueOffset * index;
        }

        /// <summary>
        /// Dịch chuyển tất cả khách còn lại về đúng vị trí hàng chờ của mình.
        /// Gọi sau khi 1 khách bị xóa khỏi <c>customerQueue</c>.
        /// </summary>
        protected void AdvanceQueue()
        {
            for (int i = 0; i < customerQueue.Count; i++)
            {
                customerQueue[i].MoveTo(GetQueuePosition(i));
            }
        }
    }
}