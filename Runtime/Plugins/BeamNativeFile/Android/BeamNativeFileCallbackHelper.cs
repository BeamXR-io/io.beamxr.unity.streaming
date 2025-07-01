#if UNITY_EDITOR || UNITY_ANDROID
using System;
using UnityEngine;

namespace BeamXR.Streaming.Core.NativeFile
{
	public class BeamNativeFileCallbackHelper : MonoBehaviour
	{
		private bool autoDestroyWithCallback;
		private Action mainThreadAction = null;

		public static BeamNativeFileCallbackHelper Create( bool autoDestroyWithCallback )
		{
			BeamNativeFileCallbackHelper result = new GameObject( "BeamNativeFile" ).AddComponent<BeamNativeFileCallbackHelper>();
			result.autoDestroyWithCallback = autoDestroyWithCallback;
			DontDestroyOnLoad( result.gameObject );
			return result;
		}

		public void CallOnMainThread( Action function )
		{
			lock( this )
			{
				mainThreadAction += function;
			}
		}

		private void Update()
		{
			if( mainThreadAction != null )
			{
				try
				{
					Action temp;
					lock( this )
					{
						temp = mainThreadAction;
						mainThreadAction = null;
					}

					temp();
				}
				finally
				{
					if( autoDestroyWithCallback )
						Destroy( gameObject );
				}
			}
		}
	}
}
#endif