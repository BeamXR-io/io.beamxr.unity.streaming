using System;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace BeamXR.Streaming.Core.NativeFile
{
	public static class BeamNativeFile
	{
		public const string ANDROID_SIGNATURE = "io.beamxr.nativefile";

		public struct ImageProperties
		{
			public readonly int width;
			public readonly int height;
			public readonly string mimeType;
			public readonly ImageOrientation orientation;

			public ImageProperties(int width, int height, string mimeType, ImageOrientation orientation)
			{
				this.width = width;
				this.height = height;
				this.mimeType = mimeType;
				this.orientation = orientation;
			}
		}

		public struct VideoProperties
		{
			public readonly int width;
			public readonly int height;
			public readonly long duration;
			public readonly float rotation;

			public VideoProperties(int width, int height, long duration, float rotation)
			{
				this.width = width;
				this.height = height;
				this.duration = duration;
				this.rotation = rotation;
			}
		}

		public enum PermissionType { Read = 0, Write = 1 };
		public enum Permission { Denied = 0, Granted = 1, ShouldAsk = 2 };

		[Flags]
		public enum MediaType { Image = 1, Video = 2, Audio = 4 };

		public enum ImageOrientation { Unknown = -1, Normal = 0, Rotate90 = 1, Rotate180 = 2, Rotate270 = 3, FlipHorizontal = 4, Transpose = 5, FlipVertical = 6, Transverse = 7 };

		public delegate void PermissionCallback(Permission permission);
		public delegate void MediaSaveCallback(bool success, string path);

        #region Platform Specific Elements
#if !UNITY_EDITOR && UNITY_ANDROID
		private static AndroidJavaClass m_ajc = null;
		private static AndroidJavaClass AJC
		{
			get
			{
				if (m_ajc == null)
					m_ajc = new AndroidJavaClass($"{ANDROID_SIGNATURE}.NativeFileManager");

				return m_ajc;
			}
		}

		private static AndroidJavaObject m_context = null;
		private static AndroidJavaObject Context
		{
			get
			{
				if (m_context == null)
				{
					using (AndroidJavaObject unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
					{
						m_context = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
					}
				}

				return m_context;
			}
		}
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
	private static string m_temporaryImagePath = null;
	private static string TemporaryImagePath
	{
		get
		{
			if( m_temporaryImagePath == null )
			{
				m_temporaryImagePath = Path.Combine( Application.temporaryCachePath, "tmpImg" );
				Directory.CreateDirectory( Application.temporaryCachePath );
			}

			return m_temporaryImagePath;
		}
	}

	private static string m_selectedMediaPath = null;
	private static string SelectedMediaPath
	{
		get
		{
			if( m_selectedMediaPath == null )
			{
				m_selectedMediaPath = Path.Combine( Application.temporaryCachePath, "pickedMedia" );
				Directory.CreateDirectory( Application.temporaryCachePath );
			}

			return m_selectedMediaPath;
		}
	}
#endif
        #endregion

        #region Runtime Permissions

        public static bool CheckPermission(PermissionType permissionType, MediaType mediaTypes)
		{
#if !UNITY_EDITOR && UNITY_ANDROID
		return AJC.CallStatic<int>( "CheckPermission", Context, permissionType == PermissionType.Read, (int) mediaTypes ) == 1;
#else
			return true;
#endif
		}

		public static void RequestPermissionAsync(PermissionCallback callback, PermissionType permissionType, MediaType mediaTypes)
		{
#if !UNITY_EDITOR && UNITY_ANDROID
		BeamNativeFilePermissionCallbackAndroid nativeCallback = new( callback );
		AJC.CallStatic( "RequestPermission", Context, nativeCallback, permissionType == PermissionType.Read, (int) mediaTypes );
#else
            callback(Permission.Granted);
#endif
		}

		public static Task<Permission> RequestPermissionAsync(PermissionType permissionType, MediaType mediaTypes)
		{
			TaskCompletionSource<Permission> tcs = new TaskCompletionSource<Permission>();
			RequestPermissionAsync((permission) => tcs.SetResult(permission), permissionType, mediaTypes);
			return tcs.Task;
		}
		#endregion

		#region Save Functions
		public static void SaveImageToGallery(byte[] mediaBytes, string album, string filename, MediaSaveCallback callback = null)
		{
			SaveToGallery(mediaBytes, album, filename, MediaType.Image, callback);
		}

		public static void SaveImageToGallery(string existingMediaPath, string album, string filename, MediaSaveCallback callback = null)
		{
			SaveToGallery(existingMediaPath, album, filename, MediaType.Image, callback);
		}

		public static void SaveImageToGallery(Texture2D image, string album, string filename, MediaSaveCallback callback = null)
		{
			if (image == null)
				throw new ArgumentException("Parameter 'image' is null!");

			if (filename.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
				SaveToGallery(GetTextureBytes(image, true), album, filename, MediaType.Image, callback);
			else if (filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				SaveToGallery(GetTextureBytes(image, false), album, filename, MediaType.Image, callback);
			else
				SaveToGallery(GetTextureBytes(image, false), album, filename + ".png", MediaType.Image, callback);
		}

		public static void SaveVideoToGallery(byte[] mediaBytes, string album, string filename, MediaSaveCallback callback = null)
		{
			SaveToGallery(mediaBytes, album, filename, MediaType.Video, callback);
		}

		public static void SaveVideoToGallery(string existingMediaPath, string album, string filename, MediaSaveCallback callback = null)
		{
			SaveToGallery(existingMediaPath, album, filename, MediaType.Video, callback);
		}
		#endregion

		#region Internal Functions
		private static void SaveToGallery(byte[] mediaBytes, string album, string filename, MediaType mediaType, MediaSaveCallback callback)
		{
			if (mediaBytes == null || mediaBytes.Length == 0)
				throw new ArgumentException("Parameter 'mediaBytes' is null or empty!");

			if (album == null || album.Length == 0)
				throw new ArgumentException("Parameter 'album' is null or empty!");

			if (filename == null || filename.Length == 0)
				throw new ArgumentException("Parameter 'filename' is null or empty!");

			if (string.IsNullOrEmpty(Path.GetExtension(filename)))
				Debug.LogWarning("'filename' doesn't have an extension, this might result in unexpected behaviour!");

			RequestPermissionAsync((permission) =>
			{
				if (permission != Permission.Granted)
				{
					callback?.Invoke(false, null);
					return;
				}

				string path = GetTemporarySavePath(filename);
				File.WriteAllBytes( path, mediaBytes );

				SaveToGalleryInternal(path, album, mediaType, callback);
			}, PermissionType.Write, mediaType);
		}

		private static void SaveToGallery(string existingMediaPath, string album, string filename, MediaType mediaType, MediaSaveCallback callback)
		{
			if (!File.Exists(existingMediaPath))
				throw new FileNotFoundException("File not found at " + existingMediaPath);

			if (album == null || album.Length == 0)
				throw new ArgumentException("Parameter 'album' is null or empty!");

			if (filename == null || filename.Length == 0)
				throw new ArgumentException("Parameter 'filename' is null or empty!");

			if (string.IsNullOrEmpty(Path.GetExtension(filename)))
			{
				string originalExtension = Path.GetExtension(existingMediaPath);
				if (string.IsNullOrEmpty(originalExtension))
					Debug.LogWarning("'filename' doesn't have an extension, this might result in unexpected behaviour!");
				else
					filename += originalExtension;
			}

			RequestPermissionAsync((permission) =>
			{
				if (permission != Permission.Granted)
				{
					callback?.Invoke(false, null);
					return;
				}

				string path = GetTemporarySavePath(filename);
				File.Copy( existingMediaPath, path, true );

				SaveToGalleryInternal(path, album, mediaType, callback);
			}, PermissionType.Write, mediaType);
		}

		private static void SaveToGalleryInternal(string path, string album, MediaType mediaType, MediaSaveCallback callback)
		{
#if !UNITY_EDITOR && UNITY_ANDROID
		string savePath = AJC.CallStatic<string>( "SaveMedia", Context, (int) mediaType, path, album );

		File.Delete( path );

		if( callback != null )
			callback( !string.IsNullOrEmpty( savePath ), savePath );
#else

			string basePath = Application.persistentDataPath;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			switch (mediaType)
			{
				case MediaType.Image:
					basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    break;
				case MediaType.Video:
					basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                    break;
				case MediaType.Audio:
					basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
					break;
			}
#endif
            string newPath = Path.Combine(basePath, album);
			Directory.CreateDirectory(newPath);
            newPath = Path.Combine(newPath, Path.GetFileName(path));
			File.Copy(path, newPath, true);
			File.Delete(path);

            if (callback != null)
				callback(true, null);
#endif
		}

		private static string GetTemporarySavePath(string filename)
		{
			string saveDir = Path.Combine(Application.persistentDataPath, "BeamRecordings");
			Directory.CreateDirectory(saveDir);
			return Path.Combine(saveDir, filename);
		}

		private static byte[] GetTextureBytes(Texture2D texture, bool isJpeg)
		{
			try
			{
				return isJpeg ? texture.EncodeToJPG(100) : texture.EncodeToPNG();
			}
			catch (UnityException)
			{
				return GetTextureBytesFromCopy(texture, isJpeg);
			}
			catch (ArgumentException)
			{
				return GetTextureBytesFromCopy(texture, isJpeg);
			}

#pragma warning disable 0162
			return null;
#pragma warning restore 0162
		}

		private static byte[] GetTextureBytesFromCopy(Texture2D texture, bool isJpeg)
		{
			// Texture is marked as non-readable, create a readable copy and save it instead
			Debug.LogWarning("Saving non-readable textures is slower than saving readable textures");

			Texture2D sourceTexReadable = null;
			RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height);
			RenderTexture activeRT = RenderTexture.active;

			try
			{
				Graphics.Blit(texture, rt);
				RenderTexture.active = rt;

				sourceTexReadable = new Texture2D(texture.width, texture.height, isJpeg ? TextureFormat.RGB24 : TextureFormat.RGBA32, false);
				sourceTexReadable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
				sourceTexReadable.Apply(false, false);
			}
			catch (Exception e)
			{
				Debug.LogException(e);

				Object.DestroyImmediate(sourceTexReadable);
				return null;
			}
			finally
			{
				RenderTexture.active = activeRT;
				RenderTexture.ReleaseTemporary(rt);
			}

			try
			{
				return isJpeg ? sourceTexReadable.EncodeToJPG(100) : sourceTexReadable.EncodeToPNG();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				return null;
			}
			finally
			{
				Object.DestroyImmediate(sourceTexReadable);
			}
		}

#if UNITY_ANDROID
		private static async Task<T> TryCallNativeAndroidFunctionOnSeparateThread<T>(Func<T> function)
		{
			T result = default(T);
			bool hasResult = false;

			await Task.Run(() =>
			{
				if (AndroidJNI.AttachCurrentThread() != 0)
					Debug.LogWarning("Couldn't attach JNI thread, calling native function on the main thread");
				else
				{
					try
					{
						result = function();
						hasResult = true;
					}
					finally
					{
						AndroidJNI.DetachCurrentThread();
					}
				}
			});

			return hasResult ? result : function();
		}
#endif
		#endregion
	}
}