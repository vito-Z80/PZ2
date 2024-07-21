using Core;
using UnityEngine;

//  https://docs.google.com/document/d/10ioZjbwGuKJBPhAostG4Qw1v6gCJLc2CwKosXrfNA2c/edit

//  нарезка коллайдеров в спрайтэдиторе
//  https://forum.unity.com/threads/the-problem-of-colliders-on-tilemap.1288397/


public class Launcher : MonoBehaviour
{
    void OnEnable()
    {
        var a = GameManager.I;
        Destroy(gameObject);
    }
}