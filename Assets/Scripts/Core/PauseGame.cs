using UnityEngine;

namespace Core
{
    public class PauseGame : MonoBehaviour
    {
        
        void OnEnable()
        {
            Time.timeScale = 0.0f;
        }

        void OnDisable()
        {
            Time.timeScale = 1.0f;
        }
    }
}