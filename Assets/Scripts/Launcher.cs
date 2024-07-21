using Core;
using UnityEngine;

//  https://docs.google.com/document/d/10ioZjbwGuKJBPhAostG4Qw1v6gCJLc2CwKosXrfNA2c/edit




public class Launcher : MonoBehaviour
{
    void OnEnable()
    {
        var a = GameManager.I;
        Destroy(gameObject);
    }
}