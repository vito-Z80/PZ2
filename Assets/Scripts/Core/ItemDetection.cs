using Items;
using UnityEngine;

namespace Core
{
    public class ItemDetection : MonoBehaviour
    {
        void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Item"))
            {
                other.GetComponent<Item>().SetTargetToFly(transform);
            }
        }
    }
}