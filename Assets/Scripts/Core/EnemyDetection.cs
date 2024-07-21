using System;
using UnityEngine;

namespace Core
{
    public class EnemyDetection : MonoBehaviour
    {


        public static event Action<Collider2D> OnDetect;
        public static event Action<Collider2D> OnExit;
        
        
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                OnDetect?.Invoke(other);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                OnExit?.Invoke(other);
            }
        }
    }
}