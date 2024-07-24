using Core;
using UnityEngine;

//  https://docs.google.com/document/d/10ioZjbwGuKJBPhAostG4Qw1v6gCJLc2CwKosXrfNA2c/edit

//  нарезка коллайдеров в спрайтэдиторе
//  https://forum.unity.com/threads/the-problem-of-colliders-on-tilemap.1288397/


public class Launcher : MonoBehaviour
{
    [Header("Одновременно врагов на карте.")] [SerializeField]
    int maxSpawnEnemies;

    [Header("Сколько уничтожить врагов до победы.")] [SerializeField]
    int killEnemiesForWin;

    [Header("Враги преследуют цель всегда или при обнаружении.")] [SerializeField]
    bool enemiesAlwaysPursueGoal;

    void OnEnable()
    {
        var gm = GameManager.I;
        gm.maxSpawnEnemies = maxSpawnEnemies;
        gm.killEnemiesForWin = killEnemiesForWin;
        gm.enemiesAlwaysPursueGoal = enemiesAlwaysPursueGoal;
        Destroy(gameObject);
    }
}