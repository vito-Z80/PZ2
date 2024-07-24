using Core;
using TMPro;
using UnityEngine;

namespace UI
{
    public class BulletsCountLabel : MonoBehaviour
    {
    
        TextMeshProUGUI m_text;

        void Start()
        {
            m_text = GetComponent<TextMeshProUGUI>();
        }

        void OnEnable()
        {
            GameManager.OnBulletsUpdate += SetText;
        }

        void OnDisable()
        {
            GameManager.OnBulletsUpdate -= SetText;
        }

        void SetText(int digit)
        {
            m_text.text = digit.ToString();
        }
    }
}