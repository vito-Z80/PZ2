using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Transform healthLine;
        int m_maxHealth;


        public void Init(int maxHealth, int currentHealth)
        {
            healthLine.localScale = new Vector3(1.0f, healthLine.localScale.y, healthLine.localScale.y);
            m_maxHealth = maxHealth;
            UpdateHealth(currentHealth);
        }

        public void UpdateHealth(int newHealth)
        {
            if (!healthLine || healthLine.IsDestroyed()) return;
            var normalizedHealth = Mathf.Clamp((float)newHealth / (float)m_maxHealth, 0.0f, 1.0f);
            healthLine.localScale = new Vector3(normalizedHealth, healthLine.localScale.y, healthLine.localScale.z);
        }
    }
}