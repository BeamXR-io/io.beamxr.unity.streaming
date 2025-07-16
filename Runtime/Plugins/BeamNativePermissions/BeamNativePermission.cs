using System;

namespace BeamXR.Streaming.Core.Permissions
{
    public static class BeamNativePermission
    {
        public enum Permission
        {
            Microphone = 0,
        }

        public static void RequestPermission(Permission permission, Action<bool> result)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            result?.Invoke(true);
#elif UNITY_ANDROID
            CheckAndroidPermission(permission, result);
#else
            result?.Invoke(true);
#endif 
        }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static void CheckAndroidPermission(Permission permission, Action<bool> result)
    {
        var callbacks = new UnityEngine.Android.PermissionCallbacks();
        callbacks.PermissionDenied += (name) => { result?.Invoke(false); };
        callbacks.PermissionGranted += (name) => { result?.Invoke(true); };
        callbacks.PermissionDeniedAndDontAskAgain += (name) => { result?.Invoke(false); };

        switch (permission)
        {
            case Permission.Microphone:
                if (UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
                {
                    result?.Invoke(true);
                }
                else
                {
                    
                    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone, callbacks);
                }
                break;
        }
    }
#endif
    }

}