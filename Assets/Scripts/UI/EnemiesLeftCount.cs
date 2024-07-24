using Core;
using TMPro;
using UnityEngine;

namespace UI
{
    public class EnemiesLeftCount : MonoBehaviour
    {
        TextMeshProUGUI m_text;

        void Start()
        {
            m_text = GetComponent<TextMeshProUGUI>();
        }

        void OnEnable()
        {
            Spawner.OnEnemyDead += SetText;
        }

        void OnDisable()
        {
            Spawner.OnEnemyDead -= SetText;
        }

        void SetText(int digit)
        {
            m_text.text = digit.ToString();
        }
    }
}