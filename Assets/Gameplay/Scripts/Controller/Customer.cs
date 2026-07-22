using Gameplay.Scripts.Data;
using UnityEngine;

namespace Gameplay.Scripts.Controller
{
    public class Customer: MonoBehaviour
    {
        public Sprite image;

        public void Setup(CustomerData customerData)
        {
            
        }

        public void OnServeFail()
        {
            
        }

        public void OnServe()
        {
            
        }

        /// <summary>
        /// Di chuyển customer tới vị trí mới trong hàng chờ.
        /// Thay bằng coroutine / DOTween nếu muốn có animation.
        /// </summary>
        public virtual void MoveTo(Vector3 targetPosition)
        {
            transform.position = targetPosition;
        }
    }
}