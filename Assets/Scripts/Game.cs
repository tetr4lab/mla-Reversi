#define DEBUGLOG
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReversiLogic;

namespace ReversiGame {

	/// <summary>物理ゲーム</summary>
	public class Game : MonoBehaviour {

		#region Field and Accessor
		/// <summary>ゲーム状態</summary>
		public GameState State {
			get => state;
			private set {
				Debug.Log ($"GameState {state} => {value}");
				state = value;
			}
		}
		private GameState state = GameState.NotReady;
		/// <summary>ステップ</summary>
		public int Step => Reversi.Step;
		/// <summary>最後の手</summary>
		public (int i, int j) LastMove => Reversi.LastMove;
		/// <summary>累積スコア</summary>
		public (int black, int white) ColorScore;
		/// <summary>累積スコア</summary>
		public (int human, int machine) RaceScore;
		/// <summary>スコア</summary>
		public (int black, int white, BoardStatus status) Score => Reversi.Score;
		/// <summary>終局</summary>
		public bool IsEnd => Reversi.IsEnd;
		/// <summary>黒の手番</summary>
		public bool IsBlackTurn => Reversi.IsBlackTurn;
		/// <summary>白の手番</summary>
		public bool IsWhiteTurn => Reversi.IsWhiteTurn;
		/// <summary>黒の差し手がある</summary>
		public bool BlackEnable => Reversi.BlackEnable;
		/// <summary>白の差し手がある</summary>
		public bool WhiteEnable => Reversi.WhiteEnable;
		/// <summary>人間対機械</summary>
		public bool HumanVsMachine => BlackHuman != WhiteHuman;
		/// <summary>人間がいる</summary>
		public bool SomeHuman => BlackHuman || WhiteHuman;
		/// <summary>人間だけ</summary>
		public bool HumanOnly => BlackHuman && WhiteHuman;
		/// <summary>人間の手番</summary>
		public bool HumanTurn => (IsBlackTurn && BlackHuman) || (IsWhiteTurn && WhiteHuman);
		/// <summary>機械がいる</summary>
		public bool SomeMachine => BlackMachine || WhiteMachine;
		/// <summary>機械だけ</summary>
		public bool MachineOnly => BlackMachine && WhiteMachine;
		/// <summary>機械の手番</summary>
		public bool MachineTurn => (IsBlackTurn && BlackMachine) || (IsWhiteTurn && WhiteMachine);
		/// <summary>人間対機械時の機械のエージェント</summary>
		public ReversiAgent MachineAgent => HumanVsMachine ? (BlackMachine ? blackAgent : whiteAgent) : null;
		/// <summary>人間対機械時の人間のスコア</summary>
		public int HumanScore => HumanVsMachine ? BlackHuman ? reversi.Score.black : reversi.Score.white : 0;
		/// <summary>人間対機械時の機械のスコア</summary>
		public int MachineScore => HumanVsMachine ? BlackMachine ? reversi.Score.black : reversi.Score.white : 0;
		/// <summary>人間対機械時の人間の優勢</summary>
		public bool HumanWin => HumanScore > MachineScore;
		/// <summary>人間対機械時の機械の優勢</summary>
		public bool MachineWin => HumanScore < MachineScore;
		/// <summary>マスの取得</summary>
		public ReversiLogic.Square this [int index] => Reversi [index];
		/// <summary>手番の石が置けるか</summary>
		public bool Enable (int index) => Reversi.Enable (index);
		/// <summary>決定要求中のエージェント</summary>
		public ReversiAgent TurnAgent = null;

		/// <summary>黒は人間</summary>
		public bool BlackHuman {
			get => blackAgent.IsHuman;
			set { blackAgent.IsHuman = value; }
		}
		/// <summary>白は人間</summary>
		public bool WhiteHuman {
			get => whiteAgent.IsHuman;
			set { whiteAgent.IsHuman = value; }
		}
		/// <summary>黒は機械</summary>
		public bool BlackMachine {
			get => blackAgent.IsMachine;
			set { blackAgent.IsMachine = value; }
		}
		/// <summary>白は機械</summary>
		public bool WhiteMachine {
			get => whiteAgent.IsMachine;
			set { whiteAgent.IsMachine = value; }
		}

		/// <summary>人間と機械の入れ替え</summary>
		public bool ChangeActors () => blackAgent.ChangeActor () && whiteAgent.ChangeActor ();

		/// <summary>チームの入れ替え</summary>
		public bool ChangeAgents () {
			if (blackAgent.ChangeTeam () && whiteAgent.ChangeTeam ()) {
				Debug.Log ($"Agents Changed");
				detectAgents ();
				return true;
			}
			return false;
		}

		/// <summary>論理ゲーム</summary>
		public Reversi Reversi {
			get {
				if (reversi == null) { init (); }
				return reversi;
			}
			private set { reversi = value; }
		}
		private Reversi reversi = null;

		/// <summary>物理盤面</summary>
		private BoardObject board = null;
		private ReversiAgent [] agents = null;
		/// <summary>黒のエージェント</summary>
		private ReversiAgent blackAgent = null;
		/// <summary>白のエージェント</summary>
		private ReversiAgent whiteAgent = null;

		#endregion

		/// <summary>エージェントの検出</summary>
		private void detectAgents () {
			if (agents == null) {
				agents = GetComponentsInChildren<ReversiAgent> ();
				foreach (var agent in agents) { agent.Init (); }
			}
			foreach (var agent in agents) {
				if (agent.IsBlack) {
					blackAgent = agent;
				} else if (agent.IsWhite) {
					whiteAgent = agent;
				}
			}
			Debug.Log ($"Agents Detected blackAgentId={blackAgent?.TeamId}, whiteAgentId={whiteAgent?.TeamId}");
		}

		/// <summary>初期化</summary>
		private void init () {
			if (reversi == null) {
				detectAgents ();
				Reversi = new Reversi ();
				ColorScore = (0, 0);
				board = BoardObject.Create (transform, this);
				if (HumanVsMachine) {
					Confirm.Create ( // 攻守選択ダイアログ
						transform.parent,
						$"<size=32>Select First / Second</size>",
						"Black", () => { BlackHuman = true; WhiteMachine = true; },
						"White", () => { BlackMachine = true; WhiteHuman = true; },
						() => { State = GameState.Play; }
					);
					State = GameState.Confirm;
				} else {
					State = GameState.Play;
				}
			}
			#region SimpleTest
			//Init ();
			//Debug.Log (reversi);
			//reversi.Move (2, 4);
			//Debug.Log (reversi);
			//reversi.Move (2, 5);
			//Debug.Log (reversi);
			//reversi.Move (3, 5);
			//Debug.Log (reversi);
			//reversi.Move (4, 5);
			//Debug.Log (reversi);
			//reversi.Move (3, 6);
			//Debug.Log (reversi);
			//reversi.Move (2, 3);
			//Debug.Log (reversi);
			//reversi.Move (1, 3);
			//Debug.Log (reversi);
			//reversi.Move (3, 7);
			//Debug.Log (reversi);
			//reversi.Move (2, 2);
			//Debug.Log (reversi);
			//reversi.Move (0, 2);
			//Debug.Log (reversi);
			//reversi.Move (0, 3);
			//Debug.Log (reversi);
			//reversi.Move (0, 4);
			//Debug.Log (reversi);
			//reversi.Move (5, 3);
			//Debug.Log (reversi);
			//reversi.Move (6, 3);
			//Debug.Log (reversi);
			//reversi.Move (2, 6);
			//Debug.Log (reversi);
			//reversi.Move (3, 1);
			//Debug.Log (reversi);
			//reversi.Move (4, 2);
			//Debug.Log (reversi);
			//reversi.Move (2, 7);
			//Debug.Log (reversi);
			//reversi.Move (1, 1);
			//Debug.Log (reversi);
			//reversi.Move (0, 0);
			//Debug.Log (reversi);
			//reversi.Move (6, 4);
			//Debug.Log (reversi);
			//reversi.Move (7, 5);
			//Debug.Log (reversi);
			//Board.OnUpdated ();
			#endregion
		}

		/// <summary>オブジェクト初期化</summary>
		private void Awake () => init ();

		/// <summary>石を置く</summary>
		public void Move (int index) {
			if (Enable (index)) {
				Reversi.Move (index);
				board.RequestUpdate ();
			}
		}

		/// <summary>駆動</summary>
		private void Update () {
			if (TurnAgent) { return; } // 要求受付待機中
			switch (State) {
				case GameState.Reset: // リセット要求がある
					reversi.Reset ();
					board.RequestUpdate ();
					Debug.Log ($"GameReseted black={(BlackHuman ? "human" : "machine")}, white={(WhiteHuman ? "human" : "machine")}, step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.status}");
					State = GameState.Play;
					break;
				case GameState.End: // 終局
					if (SomeHuman) { // 対人で終局
						if (!Confirm.OnMode) {
							Confirm.Create ( // リセットダイアログ
								transform.parent,
								HumanVsMachine ? $"<size=64>{(HumanWin ? "You Win" : MachineWin ? "You Lose" : "Draw")}</size>" : $"<size=64>{(reversi.BlackWin ? "Black Win" : reversi.WhiteWin ? "White Win" : "Draw")}</size>",
								"Change", () => ChangeAgents (),
								"Continue", null,
								() => { State = GameState.Reset; }
							);
						}
						State = GameState.Confirm;
					} else {
						if (MachineOnly) { ChangeAgents (); }
						State = GameState.Reset;
					}
					if (blackAgent.IsMachine) { blackAgent.OnEnd (); }
					if (whiteAgent.IsMachine) { whiteAgent.OnEnd (); }
					if (HumanWin) { RaceScore.human++; } else if (MachineWin) { RaceScore.machine++; }
					if (reversi.BlackWin) { ColorScore.black++; } else if (reversi.WhiteWin) { ColorScore.white++; }
					Debug.Log ($"GameReseted step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.status}");
					board.RequestUpdate ();
					break;
				case GameState.Play: // プレイ進行中
					if (IsEnd) { // 終局を検出
						State = GameState.End;
					} if (BlackMachine && IsBlackTurn && BlackEnable) { // 黒機械の手番
						Debug.Log ($"BlackAgent.RequestDecision step={Step}, turn={(IsBlackTurn ? "Black" : "White")}, status={Score.status}");
						TurnAgent = blackAgent;
						TurnAgent.RequestDecision ();
					} else if (WhiteMachine && IsWhiteTurn && WhiteEnable) { // 白機械の手番
						Debug.Log ($"WhiteAgent.RequestDecision step={Step}, turn={(IsBlackTurn ? "Black" : "White")}, status={Score.status}");
						TurnAgent = whiteAgent;
						TurnAgent.RequestDecision ();
					}
					break;
			}
		}

	}

	/// <summary>ゲーム状態</summary>
	public enum GameState {
		NotReady = 0, // 初期化待機中
		Play, // プレイ進行中
		End, // 終局処理要求
		Confirm, // 確認待機中
		Reset, // 初期化要求
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
