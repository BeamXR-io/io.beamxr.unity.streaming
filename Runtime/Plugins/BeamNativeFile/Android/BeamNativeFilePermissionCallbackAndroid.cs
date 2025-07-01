#if UNITY_EDITOR || UNITY_ANDROID
using UnityEngine;

namespace BeamXR.Streaming.Core.NativeFile
{
	public class BeamNativeFilePermissionCallbackAndroid : AndroidJavaProxy
	{
		private readonly BeamNativeFile.PermissionCallback callback;
		private readonly BeamNativeFileCallbackHelper callbackHelper;

		public BeamNativeFilePermissionCallbackAndroid(BeamNativeFile.PermissionCallback callback ) : base($"{BeamNativeFile.ANDROID_SIGNATURE}.NativeFilePermissionReceiver")
		{
			this.callback = callback;
			callbackHelper = BeamNativeFileCallbackHelper.Create( true );
		}

		[UnityEngine.Scripting.Preserve]
		public void OnPermissionResult( int result )
		{
			callbackHelper.CallOnMainThread( () => callback( (BeamNativeFile.Permission) result ) );
		}
	}
}
#endif