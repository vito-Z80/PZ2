using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Transform healthLine;

        int m_maxHealth;

        int m_health;

        // Start is called before the first frame update
        void Start()
        {
            healthLine.localScale = new Vector3(1.0f, healthLine.localScale.y, 1);
        }


        public void SetMaxHealth(int maxHealth)
        {
            m_maxHealth = maxHealth;
            m_health = maxHealth;
        }

        public void UpdateHealth(int damage)
        {
            if (!healthLine || healthLine.IsDestroyed()) return;
            m_health += damage;
            var normalizedHealth = Mathf.Clamp((float)m_health / (float)m_maxHealth, 0.0f, 1.0f);
            healthLine.localScale = new Vector3(normalizedHealth, healthLine.localScale.y, healthLine.localScale.z);
        }
    }
}