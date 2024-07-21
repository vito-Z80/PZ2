using UnityEngine;
using UnityEngine.Android;

namespace Utils
{
    public static class PlatformPermission
    {
        public static void  CheckPermissions()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }


            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) &&
                Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Debug.Log("Permission granted !!!!!!!!!!!!!!!!!!!!");
            }
            else
            {
                Debug.LogError("Не предоставлены необходимые разрешения -------------------------------------");
            }
        }
        
    }
}