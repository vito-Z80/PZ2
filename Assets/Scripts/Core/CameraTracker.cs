using UnityEngine;

namespace Core
{
    public class CameraTracker : MonoBehaviour
    {
        Camera m_camera;
        Transform m_target;

        void Start()
        {
            m_camera = GetComponent<Camera>();
        }

        void Update()
        {
            if (m_target == null) return;
            m_camera.transform.position = m_target.position + Vector3.back + Vector3.up;            
        }

        public void SetTarget(Transform target)
        {
            m_target = target;
        }
    }
}