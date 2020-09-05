using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReversiLogic;

namespace ReversiGame {

	/// <summary>物理盤面</summary>
	public class BoardObject : MonoBehaviour {

		#region Static

		/// <summary>表示更新可</summary>
		[HideInInspector] public static bool AllowUpdate = true;

		/// <summary>プレハブのパス</summary>
		private const string prefabPath = "Prefabs/Board";
		/// <summary>盤面のマス数</summary>
		private const int Size = ReversiLogic.Board.Size;
		/// <summary>プレハブ</summary>
		private static GameObject prefab = null;
		/// <summary>縦のマス名</summary>
		private static readonly string [] rowName = { "1", "2", "3", "4", "5", "6", "7", "8" };
		/// <summary>横のマス名</summary>
		private static readonly string [] colName = { "a", "b", "c", "d", "e", "f", "g", "h" };

		/// <summary>生成</summary>
		public static BoardObject Create (Transform parent, Game game) {
			if (!prefab) { prefab = Resources.Load<GameObject> (prefabPath); }
			if (!prefab) { throw new MissingComponentException ($"resources not found '{prefabPath}'"); }
			var instance = Instantiate (prefab, parent)?.GetComponent<BoardObject> ();
			instance?.initialize (parent, game);
			return instance;
		}

		#endregion

		[SerializeField, Tooltip ("表示更新許可切替")] private Toggle allowUpdateToggle = default;
		[SerializeField, Tooltip ("総合得点表示体")] private Text totalScoreText = default;
		[SerializeField, Tooltip ("得点表示体")] private Text scoreText = default;
		[SerializeField, Tooltip ("手番表示体")] private Text turnText = default;
		[SerializeField, Tooltip ("最終手表示体")] private Text lastMoveText = default;
		[SerializeField, Tooltip ("手数表示体")] private Text stepText = default;

		/// <summary>物理ゲーム</summary>
		private Game game;
		/// <summary>物理マス</summary>
		private List<SquareObject> squares;
		/// <summary>マスの取得</summary>
		public SquareObject this [int index] => squares [index];

		/// <summary>初期化</summary>
		private void initialize (Transform parent, Game game) {
			transform.SetAsLastSibling ();
			this.game = game;
			allowUpdateToggle.isOn = AllowUpdate = !game.MachineOnly;
			allowUpdateToggle.gameObject.SetActive (game.MachineOnly);
			squares = new List<SquareObject> { };
			for (var i = 0; i < Size * Size; i++) {
				var square = SquareObject.Create (transform, game, i);
				if (square) {
					squares.Add (square);
				}
			}
			RequestUpdate ();
		}

		/// <summary>表示更新要求</summary>
		public void RequestUpdate () {
			if (AllowUpdate) {
				Debug.Log ($"Scores Race=({game.RaceScore}), Color=({game.ColorScore}), Human={game.HumanScore}, Machine={game.MachineScore}, Black={game.Score.black}, White={game.Score.white}");
				(var x, var y) = game.HumanVsMachine ? game.RaceScore : game.ColorScore;
				totalScoreText.text = $"{x} : {y}";
				var score = game.Score;
				scoreText.text = game.HumanVsMachine ? $"{game.HumanScore} : {game.MachineScore}" : $"{score.black} : {score.white}";
				turnText.text = (score.status == BoardStatus.End) ? "End" : game.IsBlackTurn ? "Black" : "White";
				lastMoveText.text = (game.LastMove.i < 0) ? "Pass" : ((game.Step == 0) ? "" : $"{colName [game.LastMove.j]}{rowName [game.LastMove.i]}");
				stepText.text = $"Move {game.Step}";
				foreach (var square in squares) {
					square.RequestUpdate ();
				}
			}
		}

		/// <summary>表示更新許可の切り替え</summary>
		public void OnChangeToggle () => AllowUpdate = allowUpdateToggle.isOn;

	}

}
