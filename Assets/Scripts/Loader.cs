//#define DEBUGLOG // ログの出力
//#define TRAINER // トレーナー
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReversiGame {

	/// <summary>シーンローダー</summary>
	public class Loader : MonoBehaviour {

		private void Awake () {
			var scene = SceneManager.GetActiveScene ();
			if (scene.name == "Loader") {
				var player = true;
#if TRAINER
				foreach (var arg in new [] { "-trainer", }) {
#else
				foreach (var arg in System.Environment.GetCommandLineArgs ()) {
#endif

					switch (arg.ToLower ()) {
						case "-trainer":
							player = false;
							break;
						case "-player":
							player = true;
							break;
					}
					if (player) {
						SceneManager.LoadScene ("Scenes/Reversi");
					} else {
						SceneManager.LoadScene ("Scenes/ReversiTrainer");
					}
					SceneManager.UnloadSceneAsync (scene);
				}
			}
		}

	}

	/// <summary>Debugのラッパー (DEBUGLOG未定義時にコードを無効化する)</summary>
#if !DEBUGLOG
	public class Debug {
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void Assert (bool condition, string message, Object context) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void Assert (bool condition, object message, Object context) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void Assert (bool condition, string message) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void Assert (bool condition, object message) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void Assert (bool condition, Object context) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void Assert (bool condition) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void Assert (bool condition, string format, params object [] args) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void AssertFormat (bool condition, string format, params object [] args) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void AssertFormat (bool condition, Object context, string format, params object [] args) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void Break () { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void ClearDeveloperConsole () { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void DebugBreak () { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void DrawLine (Vector3 start, Vector3 end, Color color, float duration, bool depthTest) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void DrawLine (Vector3 start, Vector3 end, Color color, float duration) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void DrawLine (Vector3 start, Vector3 end) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void DrawLine (Vector3 start, Vector3 end, Color color) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void DrawRay (Vector3 start, Vector3 dir, Color color, float duration) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void DrawRay (Vector3 start, Vector3 dir, Color color, float duration, bool depthTest) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void DrawRay (Vector3 start, Vector3 dir) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void DrawRay (Vector3 start, Vector3 dir, Color color) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void Log (object message) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void Log (object message, Object context) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogAssertion (object message, Object context) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogAssertion (object message) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogAssertionFormat (Object context, string format, params object [] args) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogAssertionFormat (string format, params object [] args) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogError (object message, Object context) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogError (object message) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogErrorFormat (string format, params object [] args) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogErrorFormat (Object context, string format, params object [] args) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogException (System.Exception exception, Object context) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogException (System.Exception exception) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogFormat (Object context, string format, params object [] args) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogFormat (string format, params object [] args) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogWarning (object message) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogWarning (object message, Object context) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogWarningFormat (string format, params object [] args) { }
		[System.Diagnostics.Conditional ("DEBUGLOG")] public static void LogWarningFormat (Object context, string format, params object [] args) { }
	}
#endif

}
