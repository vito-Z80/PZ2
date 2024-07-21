using TMPro;
using UnityEngine;

namespace Utils
{
    public class ShowFps : MonoBehaviour
    {



        [SerializeField] TextMeshProUGUI textMeshPro;

        void Start()
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
        }


        void Update()
        {
            textMeshPro.text = ((int)(1f / Time.unscaledDeltaTime)).ToString();
        }
    }
}