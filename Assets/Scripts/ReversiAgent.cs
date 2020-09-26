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
		public enum Team {
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
		/// <summary>チームカラー</summary>
		public Team TeamColor { get; private set; }
		/// <summary>黒である</summary>
		public bool IsBlack => TeamColor == Team.Black;
		/// <summary>白である</summary>
		public bool IsWhite => TeamColor == Team.White;
		/// <summary>自分が優勢</summary>
		public bool IWinner => (TeamColor == Team.Black && reversi.BlackWin) || (TeamColor == Team.White && reversi.WhiteWin);
		/// <summary>自分が劣勢</summary>
		public bool ILoser => (TeamColor == Team.Black && reversi.WhiteWin) || (TeamColor == Team.White && reversi.BlackWin);

		/// <summary>VectorAction Branch 0 Size</summary>
		public bool Passable => Parameters.BrainParameters.VectorActionSize [0] > (Size * Size);

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

		/// <summary>トレーニング中の可能性が高い</summary>
		public bool IsTraning => Parameters.BehaviorType == BehaviorType.Default;

		/// <summary>チームカラーの入れ替え (チームIDは変更しない)</summary>
		public bool ChangeTeam () {
			if (reversi.Step == 0 || reversi.IsEnd) {
				TeamColor = (TeamColor == Team.Black) ? Team.White : Team.Black;
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
				TeamColor = (Team) TeamId;
			}
		}

		/// <summary>エピソードの開始</summary>
		public override void OnEpisodeBegin () {
			Debug.Log ($"OnEpisodeBegin ({TeamColor}): step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.Status}");
			if (reversi.Step > 0 && game.State == GameState.Play) { Debug.LogError ("Not Reseted"); }
		}

		/// <summary>環境の観測</summary>
		public override void CollectObservations (VectorSensor sensor) {
			Debug.Log ($"CollectObservations ({TeamColor}): step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.Status}");
			var statuses = new float [Size * Size];
			for (var i = 0; i < Size * Size; i++) {
				//statuses [i] = (float) reversi [i].Status; // 正規化なし
				statuses [i] = (float) reversi [i].Status / (float) SquareStatus.MaxValue; // 正規化あり
			}
			sensor.AddObservation (statuses);
		}

		/// <summary>行動のマスク</summary>
		public override void CollectDiscreteActionMasks (DiscreteActionMasker actionMasker) {
			Debug.Log ($"CollectDiscreteActionMasks ({TeamColor}): step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.Status}");
			var actionIndices = new List<int> { };
			for (var i = 0; i < Size * Size; i++) {
				var status = reversi.SquareStatus (i);
				if ((IsBlack && !status.BlackEnable ()) || (IsWhite && !status.WhiteEnable ())) { // 自分が置けない場所
					actionIndices.Add (i);
				}
			}
			if (Passable && reversi.TurnEnable) { // 打てるならパスできない
				actionIndices.Add (Size * Size);
			}
			actionMasker.SetMask (0, actionIndices);
		}

		// 例外
		public class AgentMismatchException : Exception { }
		public class TeamMismatchException : Exception { }

		/// <summary>行動と報酬の割り当て</summary>
		public override void OnActionReceived (float [] vectorAction) {
			Debug.Log ($"OnActionReceived ({TeamColor}) [{vectorAction [0]}]: step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.Status}");
			if (IsMachine) {
				var index = Mathf.FloorToInt (vectorAction [0]); // 整数化
				if (index == Size * Size) { index = -1; } // パス
				try {
					if (game.TurnAgent != this) throw new AgentMismatchException (); // エージェントの不一致
					if ((reversi.IsBlackTurn && TeamColor != Team.Black) || (reversi.IsWhiteTurn && TeamColor != Team.White)) throw new TeamMismatchException (); // 手番とチームの不整合
					if (!reversi.Enable (index)) { throw new ArgumentOutOfRangeException (); } // 置けない場所
					game.Move (index);
					Debug.Log ($"Moved ({TeamColor}) [{index}]: step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.Status}");
					AddReward ((index < 0) ? -0.0006f : -0.0003f); // 継続報酬
				} catch (AgentMismatchException) {
					EndEpisode ();
					Debug.LogError ($"Agent mismatch ({TeamColor}): Step={reversi.Step}, Turn={(reversi.IsBlackTurn ? "Black" : "White")}, Status={reversi.Score.Status}\n{reversi}");
				} catch (ArgumentOutOfRangeException) {
					EndEpisode ();
					Debug.LogWarning ($"DisableMove ({TeamColor}) [{index}]: step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.Status}\n{reversi}");
				} catch (TeamMismatchException) {
					Debug.LogWarning ($"Team mismatch ({TeamColor}): Step={reversi.Step}, Turn={(reversi.IsBlackTurn ? "Black" : "White")}, Status={reversi.Score.Status}\n{reversi}");
				} finally {
					game.TurnAgent = null; // 要求を抹消
				}
			} else {
				Debug.LogError ($"{TeamColor}Agent is not Human: step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.Status}");
			}
		}

		/// <summary>終局処理と最終的な報酬の割り当て</summary>
		public void OnEnd () {
			Debug.Log ($"AgentOnEnd ({TeamColor}, {(IWinner ? "winner" : ILoser ? "loser" : "draw")}): step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.Status}");
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
				Debug.LogError ($"{TeamColor}Agent is not Human: step={reversi.Step}, turn={(reversi.IsBlackTurn ? "Black" : "White")}, status={reversi.Score.Status}");
			}
		}

	}

}
