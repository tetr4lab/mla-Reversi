using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Policies;
using ReversiLogic;

namespace ReversiGame {

	/// <summary>物理ゲーム</summary>
	public class Game : MonoBehaviour {

		#region Static

		/// <summary>プレハブのパス</summary>
		private const string prefabPath = "Prefabs/Game";
		/// <summary>プレハブ</summary>
		private static GameObject prefab = null;

		/// <summary>生成</summary>
		public static Game Create (Transform parent, Vector3 position, BehaviorType? blackAgentType = null, BehaviorType? whiteAgentType = null) {
			if (!prefab) { prefab = Resources.Load<GameObject> (prefabPath); }
			if (!prefab) { throw new MissingComponentException ($"resources not found '{prefabPath}'"); }
			var instance = Instantiate (prefab, position, Quaternion.identity, parent)?.GetComponent<Game> ();
			instance?.initialize (blackAgentType, whiteAgentType);
			return instance;
		}

		#endregion

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
		public int HumanScore => HumanVsMachine ? BlackHuman ? Reversi.Score.black : Reversi.Score.white : 0;
		/// <summary>人間対機械時の機械のスコア</summary>
		public int MachineScore => HumanVsMachine ? BlackMachine ? Reversi.Score.black : Reversi.Score.white : 0;
		/// <summary>人間対機械時の人間の優勢</summary>
		public bool HumanWin => HumanScore > MachineScore;
		/// <summary>人間対機械時の機械の優勢</summary>
		public bool MachineWin => HumanScore < MachineScore;
		/// <summary>マスの取得</summary>
		public Square this [int index] => Reversi [index];
		/// <summary>マスの状態</summary>
		public SquareStatus? SquareStatus (int index) => Reversi.Status (index);
		/// <summary>手番の石が置けるか</summary>
		public bool Enable (int index) => Reversi.Enable (index);
		/// <summary>決定要求中のエージェント</summary>
		[HideInInspector] public ReversiAgent TurnAgent = null;

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
		public Reversi Reversi { get; private set; }
		/// <summary>物理盤面</summary>
		private BoardObject board = null;
		private ReversiAgent [] agents = null;
		/// <summary>黒のエージェント</summary>
		private ReversiAgent blackAgent = null;
		/// <summary>白のエージェント</summary>
		private ReversiAgent whiteAgent = null;

		#endregion

		/// <summary>エージェントの検出</summary>
		private void detectAgents (BehaviorType? blackAgentType = null, BehaviorType? whiteAgentType = null) {
			if (agents == null) {
				agents = GetComponentsInChildren<ReversiAgent> ();
				foreach (var agent in agents) { agent.Init (); }
			}
			foreach (var agent in agents) {
				if (agent.IsBlack) {
					blackAgent = agent;
					if (blackAgentType != null) {
						blackAgent.BehaviorType = (BehaviorType) blackAgentType;
					}
				} else if (agent.IsWhite) {
					whiteAgent = agent;
					if (whiteAgentType != null) {
						whiteAgent.BehaviorType = (BehaviorType) whiteAgentType;
					}
				}
			}
			Debug.Log ($"Agents Detected blackAgentId={blackAgent?.TeamId}, whiteAgentId={whiteAgent?.TeamId}");
		}

		/// <summary>初期化</summary>
		private void initialize (BehaviorType? blackAgentType, BehaviorType? whiteAgentType) {
			transform.SetAsLastSibling ();
			if (State == GameState.NotReady) {
				detectAgents (blackAgentType, whiteAgentType);
				Reversi = new Reversi ();
				ColorScore = (0, 0);
				board = BoardObject.Create (transform, this);
				Resources.UnloadUnusedAssets ();
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
		}

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
					Reversi.Reset ();
					board.RequestUpdate ();
					Debug.Log ($"GameReseted black={(BlackHuman ? "human" : "machine")}, white={(WhiteHuman ? "human" : "machine")}, step={Reversi.Step}, turn={(Reversi.IsBlackTurn ? "Black" : "White")}, status={Reversi.Score.status}");
					State = GameState.Play;
					break;
				case GameState.End: // 終局
					if (SomeHuman) { // 対人で終局
						if (!Confirm.OnMode) {
							Confirm.Create ( // リセットダイアログ
								transform.parent,
								HumanVsMachine ? $"<size=64>{(HumanWin ? "You Win" : MachineWin ? "You Lose" : "Draw")}</size>" : $"<size=64>{(Reversi.BlackWin ? "Black Win" : Reversi.WhiteWin ? "White Win" : "Draw")}</size>",
								"Change", () => ChangeActors (),
								"Continue", null,
								() => { State = GameState.Reset; }
							);
						}
						State = GameState.Confirm;
					} else {
						State = GameState.Reset;
					}
					if (blackAgent.IsMachine) { blackAgent.OnEnd (); }
					if (whiteAgent.IsMachine) { whiteAgent.OnEnd (); }
					if (HumanWin) { RaceScore.human++; } else if (MachineWin) { RaceScore.machine++; }
					if (Reversi.BlackWin) { ColorScore.black++; } else if (Reversi.WhiteWin) { ColorScore.white++; }
					Debug.Log ($"GameReseted step={Reversi.Step}, turn={(Reversi.IsBlackTurn ? "Black" : "White")}, status={Reversi.Score.status}");
					board.RequestUpdate ();
					break;
				case GameState.Play: // プレイ進行中
					if (IsEnd) { // 終局を検出
						State = GameState.End;
					}
					if (BlackMachine && IsBlackTurn && BlackEnable) { // 黒機械の手番
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

}
