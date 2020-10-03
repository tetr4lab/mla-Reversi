using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReversiLogic;

namespace ReversiGame {

	/// <summary>物理盤面</summary>
	public class BoardObject : MonoBehaviour {

		#region Static

		/// <summary>表示更新可</summary>
		public static bool AllowUpdate = true;
		public static bool AllowDisplayHint = false;

		/// <summary>プレハブのパス</summary>
		private const string prefabPath = "Prefabs/Board";
		private const string labelPrefabPath = "Prefabs/Label";
		/// <summary>盤面のマス数</summary>
		private const int Size = ReversiLogic.Board.Size;
		/// <summary>プレハブ</summary>
		private static GameObject prefab = null;
		private static GameObject labelPrefab = null;
		/// <summary>マス名</summary>
		private static string squareName (Move move) => $"{(char) ('a' + move.Position.j)}{1 + move.Position.i}";

		/// <summary>生成</summary>
		public static BoardObject Create (Transform parent, Game game) {
			if (!prefab) { prefab = Resources.Load<GameObject> (prefabPath); }
			if (!prefab) { throw new MissingComponentException ($"resources not found '{prefabPath}'"); }
			if (!labelPrefab) { labelPrefab = Resources.Load<GameObject> (labelPrefabPath); }
			if (!labelPrefab) { throw new MissingComponentException ($"resources not found '{labelPrefabPath}'"); }
			var instance = Instantiate (prefab, parent)?.GetComponent<BoardObject> ();
			instance?.initialize (parent, game);
			return instance;
		}

		#endregion

		[SerializeField, Tooltip ("表示更新許可切替")] private Toggle updateToggle = default;
		[SerializeField, Tooltip ("助言表示許可切替")] private Toggle hintToggle = default;
		[SerializeField, Tooltip ("総合得点表示体")] private Text totalScoreText = default;
		[SerializeField, Tooltip ("総合得点総表示体")] private GameObject allScore = default;
		[SerializeField, Tooltip ("得点表示体")] private Text scoreText = default;
		[SerializeField, Tooltip ("手番表示体")] private Text turnText = default;
		[SerializeField, Tooltip ("最終手表示体")] private Text lastMoveText = default;
		[SerializeField, Tooltip ("手数表示体")] private Text stepText = default;
		[SerializeField, Tooltip ("棄権押釦")] private Button passButton = default;
		[SerializeField, Tooltip ("待押釦")] private Button retractButton = default;
		[SerializeField, Tooltip ("行名札容器")] private Transform rowLabels = default;
		[SerializeField, Tooltip ("列名札容器")] private Transform colLabels = default;

		/// <summary>物理ゲーム</summary>
		private Game game;
		/// <summary>物理マス</summary>
		private List<SquareObject> squares;
		/// <summary>マスの取得</summary>
		public SquareObject this [int index] => squares [index];

		/// <summary>総スコア表示体</summary>
		private Text [] allScores;
		/// <summary>盤面グリッド</summary>
		private GridLayoutGroup grid;

		/// <summary>初期化</summary>
		private void initialize (Transform parent, Game game) {
			transform.SetAsLastSibling ();
			this.game = game;
			grid = GetComponentInChildren<GridLayoutGroup> ();
			var rect = transform as RectTransform;
			grid.cellSize = (rect.sizeDelta - grid.spacing) / (grid.constraintCount = Size) - grid.spacing; // 盤面の外形とマスの数から、折り返しとセルサイズを算出
			for (var j = 0; j < Size; j++) {
				Instantiate (labelPrefab, rowLabels).GetComponent<Text> ().text = (1 + j).ToString ();
				Instantiate (labelPrefab, colLabels).GetComponent<Text> ().text = ((char) ('a' + j)).ToString ();
			}
			squares = new List<SquareObject> { };
			for (var i = 0; i < Size * Size; i++) {
				var square = SquareObject.Create (grid.transform, game, i);
				if (square) {
					squares.Add (square);
				}
			}
			updateToggle.isOn = AllowUpdate = !game.MachineOnly;
			updateToggle.gameObject.SetActive (game.IsMaster && game.MachineOnly);
			retractButton.gameObject.SetActive (false);
			hintToggle.isOn = AllowDisplayHint;
			allScore.SetActive (false);
			allScores = allScore.GetComponentsInChildren<Text> ();
			Debug.Log ("BoardObject intialized");
			RequestUpdate ();
		}

		/// <summary>表示更新要求</summary>
		public void RequestUpdate () {
			if (AllowUpdate) {
				Debug.Log ($"Scores Race={game.RaceScore}, Color={game.ColorScore}, Team={game.TeamScore}, Human={game.HumanScore}, Machine={game.MachineScore}, Black={game.Score.Black}, White={game.Score.White}");
				var totalScore = (game.ForceChange && game.MachineOnly) ? game.TeamScore : game.HumanVsMachine ? game.RaceScore : game.ColorScore;
				totalScoreText.text = $"<size=18>{totalScore.Title}</size>\n{totalScore.Black} : {totalScore.White} : {totalScore.Draw}"; // 累積スコア
				totalScoreText.gameObject.SetActive (totalScore != Score.Zero);
				if (allScores != null) {
					allScores [0].text = $"<size=18>{game.TeamScore.Title}</size>\n{game.TeamScore.Black} : {game.TeamScore.White} : {game.TeamScore.Draw}";
					allScores [1].text = $"<size=18>{game.RaceScore.Title}</size>\n{game.RaceScore.Black} : {game.RaceScore.White} : {game.RaceScore.Draw}";
					allScores [2].text = $"<size=18>{game.ColorScore.Title}</size>\n{game.ColorScore.Black} : {game.ColorScore.White} : {game.ColorScore.Draw}";
				}
				var score = game.Score;
				scoreText.text = game.HumanVsMachine ? $"{game.HumanScore} : {game.MachineScore}" : $"{score.Black} : {score.White}"; // スコア
				turnText.text = (score.Status == Movability.End) ? "End" : game.IsBlackTurn ? "Black" : "White"; // ターン
				lastMoveText.text = (game.Step == 0) ? "" : (game.LastMove.Index < 0) ? "Pass" : squareName (game.LastMove); // 最後の手
				stepText.text = $"Move {game.Step}"; // ステップ
				foreach (var square in squares) { square.RequestUpdate (); } // マス
				passButton.gameObject.SetActive (game.HumanTurn && !game.IsEnd && !game.TurnEnable); // 人間が打てないときだけパスボタンを表示
				retractButton.gameObject.SetActive (game.HumanTurn && !game.IsEnd && game.Step > 1); // 人間が戻せるときだけ待ったボタンを表示
			}
		}

		/// <summary>表示更新許可の切り替え</summary>
		public void OnChangeUpdateToggle () => AllowUpdate = updateToggle.isOn;

		/// <summary>助言表示許可の切り替え</summary>
		public void OnChangeStepToggle () {
			AllowDisplayHint = hintToggle.isOn;
			RequestUpdate ();
		}

		/// <summary>総合得点総表示の切り替え</summary>
		public void OnChangeAllScore () {
			allScore.SetActive (!allScore.activeSelf);
		}

		/// <summary>パスボタンが押された</summary>
		public void OnPressPassButton () {
			if (game.HumanTurn && !game.TurnAgent) { game.Move (-1); }
		}

		/// <summary>待ったボタンが押された</summary>
		public void OnPressRetractButton () {
			if (game.HumanTurn && !game.TurnAgent) { game.RetractMove (); }
		}

	}

}
