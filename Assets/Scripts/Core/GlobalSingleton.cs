using System;
using UnityEngine;

namespace Core
{
    public class GlobalSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly Lazy<T> Lazy = new(() => new GameObject(typeof(T).Name).AddComponent<T>());
        public static T I => Lazy.Value;

        internal GlobalSingleton()
        {
        }
    }
}