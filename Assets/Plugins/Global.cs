using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

using LitJson;
using System;

//using UnityEngine.iOS;
using System.Text.RegularExpressions;
using UnityEngine.iOS;

public class Global : MonoBehaviour
{

	public enum SysType
	{
		OS_MACOSX = 1,
		OS_LINUX,
		OS_WINDOWS,
		OS_ANDROID,
		OS_IOS,
		OS_UNKNOWN,
	}

	public enum GameState
	{
		Operate = 1,
		Learn,
		Exercise,
		Color,
		Formular,
		Setting,
	}

	private static GameState state = GameState.Operate;

	public static GameState State {
		set{ state = value; }
		get{ return state; }
	}

	public enum CubeStyle
	{
		NoStyle = 0,
		BigFillet,
		SmallFillet,
		BigChamfer,
		SmallChamfer
	}

	public static CubeStyle cubestyle = CubeStyle.BigFillet;



	private static SysType _sysType = SysType.OS_UNKNOWN;
	private static bool _isEditorMode = false;
	private static bool _isStandalone = false;

	public static SysType SystemType { get { return _sysType; } }

	public static bool EditorMode { get { return _isEditorMode; } }

	public static bool Standalone { get { return _isStandalone; } }

	public string appVersionFile;

	private static Dictionary<string,bool> scenemap = new Dictionary<string, bool> ();

	public class DisplayMetricsAndroid
	{
		
		// The logical density of the display
		public static float Density { get; protected set; }
		
		// The screen density expressed as dots-per-inch
		public static int DensityDPI { get; protected set; }
		
		// The absolute height of the display in pixels
		public static int HeightPixels { get; protected set; }
		
		// The absolute width of the display in pixels
		public static int WidthPixels { get; protected set; }
		
		// A scaling factor for fonts displayed on the display
		public static float ScaledDensity { get; protected set; }
		
		// The exact physical pixels per inch of the screen in the X dimension
		public static float XDPI { get; protected set; }
		
		// The exact physical pixels per inch of the screen in the Y dimension
		public static float YDPI { get; protected set; }

		#if UNITY_ANDROID
		static DisplayMetricsAndroid ()
		{
			// Early out if we're not on an Android device
			if (Application.platform != RuntimePlatform.Android) {
				return;
			}
			
			// The following is equivalent to this Java code:
			//
			// metricsInstance = new DisplayMetrics();
			// UnityPlayer.currentActivity.getWindowManager().getDefaultDisplay().getMetrics(metricsInstance);
			//
			// ... which is pretty much equivalent to the code on this page:
			// http://developer.android.com/reference/android/util/DisplayMetrics.html
			
			using (
				AndroidJavaClass unityPlayerClass = new AndroidJavaClass ("com.unity3d.player.UnityPlayer"),
				metricsClass = new AndroidJavaClass ("android.util.DisplayMetrics")) {
				using (
					AndroidJavaObject metricsInstance = new AndroidJavaObject ("android.util.DisplayMetrics"),
					activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject> ("currentActivity"),
					windowManagerInstance = activityInstance.Call<AndroidJavaObject> ("getWindowManager"),
					displayInstance = windowManagerInstance.Call<AndroidJavaObject> ("getDefaultDisplay")) {
					displayInstance.Call ("getMetrics", metricsInstance);
					Density = metricsInstance.Get<float> ("density");
					DensityDPI = metricsInstance.Get<int> ("densityDpi");
					HeightPixels = metricsInstance.Get<int> ("heightPixels");
					WidthPixels = metricsInstance.Get<int> ("widthPixels");
					ScaledDensity = metricsInstance.Get<float> ("scaledDensity");
					XDPI = metricsInstance.Get<float> ("xdpi");
					YDPI = metricsInstance.Get<float> ("ydpi");
				}
			}
		}













#else
		static DisplayMetricsAndroid ()
		{
			Density = 1f;
			DensityDPI = 160;
			ScaledDensity = 1f;
		}
		#endif
	}

	private static float _scale = 1f;

	void Awake ()
	{
		DontDestroyOnLoad (this);
		init ();

		//Compose db path
		switch (Global.SystemType) {
		case Global.SysType.OS_ANDROID:
//			filename = string.Format ("{0}/{1}", Application.persistentDataPath, filename);
			_scale = DisplayMetricsAndroid.Density.Equals (0f) ? 1f : DisplayMetricsAndroid.Density;
			break;
		case Global.SysType.OS_IOS:
//			filename = string.Format ("{0}/{1}", Application.persistentDataPath, filename);
			break;
		case Global.SysType.OS_MACOSX:
		case Global.SysType.OS_LINUX:
		case Global.SysType.OS_WINDOWS:
//			filename = string.Format ("{0}/{1}", Application.dataPath, filename);
			break;
		default:
			break;
		}

#if UNITY_ANDROID
		Utils.print (LogLevel.DEBUG, "{0} | {1} | {2} | {3} | {4} ", 
			DisplayMetricsAndroid.Density, DisplayMetricsAndroid.DensityDPI, DisplayMetricsAndroid.ScaledDensity,
			DisplayMetricsAndroid.HeightPixels, DisplayMetricsAndroid.WidthPixels);
#endif

	}

	public static T GetEnumFromString<T> (string val, T default_val)
	{
		T rc = default_val;
		try {
			rc = (T)Enum.Parse (typeof(T), val, true);
		} catch (ArgumentException e) {
			Utils.print (LogLevel.DEBUG, e.ToString ());
		}
		return rc;
	}

	public static string getSystemName ()
	{
		string rc = "Unknown";
		switch (_sysType) {
		case SysType.OS_MACOSX:
			rc = "Mac OSX";
			break;
		case SysType.OS_LINUX:
			rc = "Linux";
			break;
		case SysType.OS_WINDOWS:
			rc = "Windows";
			break;
		case SysType.OS_ANDROID:
			rc = "Android";
			break;
		case SysType.OS_IOS:
			rc = "iOS";
			break;
		default:
			break;
		}
		return rc;
	}

	private void init ()
	{
		// init os type
		#if UNITY_STANDALONE_OSX
		_sysType = SysType.OS_MACOSX;
		#elif UNITY_STANDALONE_LINUX
		_sysType= SysType.OS_LINUX;
		#elif UNITY_STANDALONE_WIN
		_sysType= SysType.OS_WINDOWS;
		#elif UNITY_ANDROID
		_sysType = SysType.OS_ANDROID;
		#elif UNITY_IPHONE
		_sysType = SysType.OS_IOS;
		#endif

		// init editor
		#if UNITY_EDITOR
		_isEditorMode = true;
		#endif

		#if UNITY_STANDALONE
		_isStandalone = true;
		#endif
	}

	public static long ComTime ()
	{
		TimeSpan dt = DateTime.UtcNow - new DateTime (1970, 1, 1);
		return (long)dt.TotalMilliseconds;
	}

	public static Sprite LoadSprite (string name)
	{
		Sprite mysprite = null;
		if (name.Equals ("网络头像")) {
		} else {
			string spritename = name;
			if ((int)Global.dp2px (1f) == 2) {
				spritename = spritename + "_x2";
				mysprite = Resources.Load<Sprite> (spritename);
			} else if ((int)Global.dp2px (1f) == 3) {
				spritename = spritename + "_x3";
				mysprite = Resources.Load<Sprite> (spritename);
			}
			if (mysprite == null) {
				mysprite = Resources.Load<Sprite> (name);
			}
		}
		return mysprite;
	}

	public static float dp2px (float dp)
	{
		float ppi = Screen.dpi > 0f ? Screen.dpi : 160f;
		#if UNITY_EDITOR
		ppi = 160 * Screen.height / 500;
		#endif


		if (_sysType == SysType.OS_IOS) {
			#if UNITY_IOS
			if (Device.generation.Equals (DeviceGeneration.iPhone4) || Device.generation.Equals (DeviceGeneration.iPhone4S)) {
				return 1.5f * dp;
			}
			#endif
			return Mathf.Max (1f, Mathf.CeilToInt ((ppi / 160f) - 0.5f)) * dp;
		} else if (_sysType == SysType.OS_ANDROID) {
			return _scale * dp;
		} else {
			return Mathf.Max (1f, (ppi / 160f)) * dp;
		}
	}

	public static bool IsNumberic (string message, out float result)
	{
		Regex rex_int_p = new Regex (@"^\d+$");
		Regex rex_int_n = new Regex (@"^-\d+$");
		Regex rex_float_p = new Regex (@"^\d+\.\d+$");
		Regex rex_float_n = new Regex (@"^-\d+\.\d+$");
		result = 0f;
		bool rc = false;
		if (rex_int_p.IsMatch (message) || rex_int_n.IsMatch (message) ||
		    rex_float_p.IsMatch (message) || rex_float_n.IsMatch (message)) {
			try {
				result = float.Parse (message);
				rc = true;
			} catch {
				Utils.print (LogLevel.WARNING, "invalid number format: {0}", message);
			}
		}
		return rc;
	}

	// 识别汉字
	//	static Regex reg = new Regex (@"[\u4e00-\u9fa5]");
	// 识别双字节字符
	static Regex reg = new Regex (@"[^\x00-\xff]");

	/// <summary>
	/// Global.CutString (string msg, int length)
	/// 传入参数，需要操作的消息，需要截取的长度，返回截取长度的消息+“…”，中文字符和标点将被处理为两个长度
	/// </summary>
	public static string CutString (string msg, int length)
	{
		if (reg.IsMatch (msg)) {
			int iNum = 0;
			for (int i = 0; i < msg.Length; i++) {
				if (reg.IsMatch (msg [i].ToString ())) {
					iNum++;
					if (iNum + i >= length) {
						length = Mathf.Min (length, i);
						break;
					}
				}
			}
		}
		if (msg.Length > length) {
			msg = msg.Remove (length) + "...";
		}
		return msg;
	}

	public static bool IsXmlExist (string scenename)
	{
		if (scenemap.ContainsKey (scenename)) {
			return scenemap [scenename];
		}
		return false;
	}

	public static int convert (JsonData d, int defaultv)
	{
		if (d.IsInt)
			return (int)d;
		else if (d.IsDouble) { 
			return (int)(double)d;
		} else {
			Utils.error ("can not convert: {0}", d.ToJson ());
			return defaultv;
		}
	}

	public static float convert (JsonData d, float defaultv)
	{
		if (d.IsInt)
			return (float)(int)d;
		else if (d.IsDouble) { 
			return (float)(double)d;
		} else {
			Utils.error ("can not convert: {0}", d.ToJson ());
			return defaultv;
		}
	}

	public static double convert (JsonData d, double defaultv)
	{
		if (d.IsInt)
			return (double)(int)d;
		else if (d.IsDouble) { 
			return (double)d;
		} else {
			Utils.error ("can not convert: {0}", d.ToJson ());
			return defaultv;
		}
	}

	public static void SaveXmlScene (string scenename, bool needsave)
	{
		if (scenemap.ContainsKey (scenename)) {
			scenemap [scenename] = needsave;
		} else {
			scenemap.Add (scenename, needsave);
		}
	}

}
