using Data;
using UnityEngine;

namespace Characters
{
    public abstract class MainCharacter : MonoBehaviour
    {

        protected Rigidbody2D Rigidbody;
        [SerializeField] protected Vector2 direction;
        public abstract void Damage(int damage);
        public abstract void AddHealth(int health);


        public abstract void Fire();

        void Start()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        protected void Dead()
        {
            if (gameObject)
                Destroy(gameObject);
        }
    }
}