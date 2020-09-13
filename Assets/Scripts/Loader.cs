//#define DEBUGLOG // ログの出力を有効にします。コメントアウトするとDebug呼び出しがコンパイルされず、ログの出力が抑制されます。
//#define TRAINER_TEST // トレーナーをエディタで実行できます。(通常はビルドした実行形式でトレーニングします。)
using UnityEngine;
using Unity.MLAgents.Policies;

namespace ReversiGame {

	/// <summary>ローダー</summary>
	public class Loader : MonoBehaviour {

		/// <summary>トレーナーの幅</summary>
		public int TrainerWidth = 7;
		/// <summary>トレーナーの高さ</summary>
		public int TrainerHeight = 7;

		/// <summary>プレイヤーまたはトレーナーを起動して自身を消す</summary>
		private void Awake () {
			// デフォルト値
			var player = true;
			var change = true;
			// コマンドライン引数
#if TRAINER_TEST && UNITY_EDITOR
			var args = new [] { "-trainer", "-width", "4", "-height", "4" }; // シミュレーション
#else
			var args = System.Environment.GetCommandLineArgs ();
#endif
			for (var i = 0; i < args.Length; i++) {

				switch (args [i].ToLower ()) {
					case "-player":
						player = true;
						break;
					case "-trainer":
						player = false;
						break;
					case "-width":
						if (++i < args.Length && int.TryParse (args [i], out var width)) {
							TrainerWidth = width;
						}
						break;
					case "-height":
						if (++i < args.Length && int.TryParse (args [i], out var height)) {
							TrainerHeight = height;
						}
						break;
					case "-change": // トレーニング時の手番の切り替え
						change = true;
						break;
					case "-fix": // トレーニング時の手番の固定
						change = false;
						break;
				}
			}
			// 起動
			var center = new Vector3 (Screen.width / 2f, Screen.height / 2f);
			if (player) {
				Game.Create (transform.parent, center, BehaviorType.HeuristicOnly, BehaviorType.InferenceOnly, change);
			} else {
				for (var i = 0; i < TrainerHeight; i++) {
					for (var j = 0; j < TrainerWidth; j++) {
						Game.Create (transform.parent, center + new Vector3 (Screen.width * j, Screen.height * i, 0f), forceChange: change);
					}
				}
			}
			Destroy (gameObject, 0.016f);
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
