using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using ReversiLogic;

namespace ReversiGame {

	/// <summary>リバーシ・エージェント</summary>
	public class ReversiAgent : Agent {

		/// <summary>チーム識別子</summary>
		public enum TeamColor {
			Black = 1,
			White = 0,
		}

		/// <summary>盤面のマス数</summary>
		private const int Size = ReversiLogic.Board.Size;
		/// <summary>物理ゲーム</summary>
		private Game game = null;
		/// <summary>論理ゲーム</summary>
		private Reversi reversi => game.Reversi;
		/// <summary>挙動パラメータ</summary>
		private BehaviorParameters Parameters => _parameters ?? (_parameters = gameObject.GetComponent<BehaviorParameters> ());
		private BehaviorParameters _parameters;
		/// <summary>チームID</summary>
		public int TeamId => Parameters.TeamId;
		/// <summary>チーム</summary>
		public TeamColor Team { get; private set; }
		/// <summary>黒である</summary>
		public bool IsBlack => Team == TeamColor.Black;
		/// <summary>白である</summary>
		public bool IsWhite => Team == TeamColor.White;
		/// <summary>自分が優勢</summary>
		public bool IWinner => (Team == TeamColor.Black && reversi.BlackWin) || (Team == TeamColor.White && reversi.WhiteWin);
		/// <summary>自分が劣勢</summary>
		public bool ILoser => (Team == TeamColor.Black && reversi.WhiteWin) || (Team == TeamColor.White && reversi.BlackWin);

		/// <summary>挙動タイプ</summary>
		public BehaviorType BehaviorType {
			get => Parameters.BehaviorType;
			set => Parameters.BehaviorType = value;
		}

		/// <summary>人間が操作</summary>
		public bool IsHuman {
			get => (Parameters.BehaviorType == BehaviorType.HeuristicOnly);
			set {
				if (reversi.Step == 0 || reversi.IsEnd) {
					Parameters.BehaviorType = value ? BehaviorType.HeuristicOnly : BehaviorType.InferenceOnly;
				}
			}
		}

		/// <summary>機械が操作 (推論時のみ)</summary>
		public bool IsMachine {
			get => (Parameters.BehaviorType != BehaviorType.HeuristicOnly);
			set {
				if (reversi.Step == 0 || reversi.IsEnd) {
					Parameters.BehaviorType = value ? BehaviorType.InferenceOnly : BehaviorType.HeuristicOnly;
				}
			}
		}

		/// <summary>チームカラーの入れ替え (チームIDは変更しない)</summary>
		public bool ChangeTeam () {
			if (reversi.Step == 0 || reversi.IsEnd) {
				Team = (Team == TeamColor.Black) ? TeamColor.White : TeamColor.Black;
				return true;
			}
			return false;
		}

		/// <summary>人間と機械の入れ替え</summary>
		public bool ChangeActor () {
			if ((reversi.Step == 0 || reversi.IsEnd) && Parameters.BehaviorType != BehaviorType.Default) {
				Parameters.BehaviorType = (Parameters.BehaviorType == BehaviorType.HeuristicOnly) ? BehaviorType.InferenceOnly : BehaviorType.HeuristicOnly;
				return true;
			}
			return false;
		}

		/// <summary>オブジェクト初期化</summary>
		private void Awake () => Init ();

		/// <summary>初期化</summary>
		public void Init () {
			if (!game) { // 一度だけ
				game = GetComponentInParent<Game> ();
				Team = (TeamColor) TeamId;
			}
		}

		/// <summary>エピソードの開始</summary>
		public override void OnEpisodeBegin () {
			Debug.Log ($"OnEpisodeBegin ({Team}): step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.status}");
			if (reversi.Step > 0 && game.State == GameState.Play) { Debug.LogError ("Not Reseted"); }
		}

		/// <summary>環境の観測</summary>
		public override void CollectObservations (VectorSensor sensor) {
			Debug.Log ($"CollectObservations ({Team}): step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.status}");
			for (var i = 0; i < Size * Size; i++) {
				sensor.AddObservation ((float) reversi [i].Status / (float) SquareStatus.MaxValue); // 正規化
			}
		}

		/// <summary>行動のマスク</summary>
		public override void CollectDiscreteActionMasks (DiscreteActionMasker actionMasker) {
			Debug.Log ($"CollectDiscreteActionMasks ({Team}): step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.status}");
			var actionIndices = new List<int> { };
			for (var i = 0; i < Size * Size; i++) {
				var status = reversi.Status (i);
				if ((IsBlack && !status.BlackEnable ()) || (IsWhite && !status.WhiteEnable ())) { // 自分が置けない場所
					actionIndices.Add (i);
				}
			}
			actionMasker.SetMask (0, actionIndices);
		}

		// 例外
		public class AgentMismatchException : Exception { }
		public class TeamMismatchException : Exception { }

		/// <summary>行動と報酬の割り当て</summary>
		public override void OnActionReceived (float [] vectorAction) {
			Debug.Log ($"OnActionReceived ({Team}) [{vectorAction [0]}]: step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.status}");
			if (IsMachine) {
				try {
					if (game.TurnAgent != this) throw new AgentMismatchException (); // エージェントの不一致
					if ((reversi.IsBlackTurn && Team != TeamColor.Black) || (reversi.IsWhiteTurn && Team != TeamColor.White)) throw new TeamMismatchException (); // 手番とチームの不整合
					var index = Mathf.FloorToInt (vectorAction [0]); // 整数化
					if (!reversi.Enable (index)) { throw new ArgumentOutOfRangeException (); } // 置けない場所
					game.Move (index);
					Debug.Log ($"Moved ({Team}) [{index}]: step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.status}");
					AddReward (-0.0003f); // 継続報酬
				} catch (AgentMismatchException) {
					EndEpisode ();
					Debug.LogError ($"Agent mismatch ({Team}): Step={reversi.Step}, Turn={(reversi.IsBlackTurn ? "Black" : "White")}, Status={reversi.Score.status}\n{reversi}");
				} catch (ArgumentOutOfRangeException) {
					EndEpisode ();
					Debug.LogWarning ($"DisableMove ({Team}) [{vectorAction [0]}]: step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.status}\n{reversi}");
				} catch (TeamMismatchException) {
					Debug.LogWarning ($"Team mismatch ({Team}): Step={reversi.Step}, Turn={(reversi.IsBlackTurn ? "Black" : "White")}, Status={reversi.Score.status}\n{reversi}");
				} finally {
					game.TurnAgent = null; // 要求を抹消
				}
			} else {
				Debug.LogError ($"{Team}Agent is not Human: step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.status}");
			}
		}

		/// <summary>終局処理と最終的な報酬の割り当て</summary>
		public void OnEnd () {
			Debug.Log ($"AgentOnEnd ({Team}, {(IWinner ? "winner" : ILoser ? "loser" : "draw")}): step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.status}");
			if (IsMachine) {
				if (IWinner) {
					SetReward (1.0f); // 勝利報酬
				} else if (ILoser) {
					SetReward (-1.0f); // 敗北報酬
				} else {
					SetReward (0f); // 引き分け報酬
				}
				EndEpisode ();
			} else {
				Debug.LogError ($"{Team}Agent is not Human: step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.status}");
			}
		}

	}

}
