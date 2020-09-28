using System;
using System.Collections.Generic;

namespace ReversiLogic {

	/// <summary>スコア</summary>
	public class Score : IEquatable<Score> {
		public string Title;
		public int Black;
		public int White;
		public int Draw;
		public int Human { get => Black; set => Black = value; }
		public int Machine { get => White; set => White = value; }
		public Score () { Title = ""; Black = White = Draw = 0; }
		public Score (int black, int white, int draw = 0) { Title = ""; Black = black; White = white; Draw = draw; }
		public Score (string title, int black = 0, int white = 0, int draw = 0) { Title = title; Black = black; White = white; Draw = draw; }
		public override string ToString () => $"({Black}, {White}, {Draw})";
		public static readonly Score Zero = new Score ();
		public virtual bool Equals (Score score) => score != null && this.Black == score.Black && this.White == score.White && this.Draw == score.Draw;
		public override bool Equals (object obj) => Equals (obj as Score);
		public override int GetHashCode () {
			var hashCode = 1190436781;
			hashCode = hashCode * -1521134295 + Black.GetHashCode ();
			hashCode = hashCode * -1521134295 + White.GetHashCode ();
			hashCode = hashCode * -1521134295 + Draw.GetHashCode ();
			return hashCode;
		}
		public static bool operator == (Score a, Score b) => EqualityComparer<Score>.Default.Equals (a, b);
		public static bool operator != (Score a, Score b) => !(a == b);
	}

	/// <summary>ゲーム</summary>
	public class Reversi {

		/// <summary>ターン番号</summary>
		public int Step => board.Step;

		/// <summary>最後の指し手</summary>
		public Move LastMove => board.LastMove;

		/// <summary>黒の手番</summary>
		public bool IsBlackTurn { get; private set; }

		/// <summary>白の手番</summary>
		public bool IsWhiteTurn => !IsBlackTurn;

		/// <summary>終局</summary>
		public bool IsEnd => board.Score.Status == Movability.End;

		/// <summary>黒の優勢</summary>
		public bool BlackWin => board.Score.Black > board.Score.White;

		/// <summary>白の優勢</summary>
		public bool WhiteWin => board.Score.Black < board.Score.White;

		/// <summary>黒の差し手がある</summary>
		public bool BlackEnable => (board.Score.Status & Movability.BlackEnable) == Movability.BlackEnable;

		/// <summary>白の差し手がある</summary>
		public bool WhiteEnable => (board.Score.Status & Movability.WhiteEnable) == Movability.WhiteEnable;

		/// <summary>手番の差し手がある</summary>
		public bool TurnEnable => (IsBlackTurn && BlackEnable) || (IsWhiteTurn && WhiteEnable);

		/// <summary>盤</summary>
		private Board board;

		/// <summary>マスの取得</summary>
		public Square this [int i, int j] => board [i, j];

		/// <summary>マスの取得</summary>
		public Square this [int index] => board [index];

		/// <summary>マスの状態</summary>
		public SquareStatus? SquareStatus (int index) => board.GetSquareStatus (index);

		/// <summary>マスの状態</summary>
		public SquareStatus? SquareStatus (int i, int j) => board.GetSquareStatus (i, j);

		/// <summary>コンストラクタ</summary>
		public Reversi () { IsBlackTurn = true; board = new Board (); }

		/// <summary>再初期化</summary>
		public void Reset () { IsBlackTurn = true; board.Reset (); }

		/// <summary>スコアと局面</summary>
		public BoardScore Score => board.Score;

		/// <summary>手番の石が置けるか</summary>
		public bool Enable (int i, int j) => (i == 0 && j == -1 && !TurnEnable) || board.Enable (i, j, IsBlackTurn);

		/// <summary>手番の石が置けるか</summary>
		public bool Enable (int index) => (index == -1 && !TurnEnable) || board.Enable (index, IsBlackTurn);

		// <summary>石を置いてターンを切り替え -1でパス</summary>
		/// <returns>終局</returns>
		public void Move (int index) => Move (index / Board.Size, index % Board.Size);

		// <summary>石を置いてターンを切り替え (0, -1)でパス</summary>
		public void Move (int i, int j) {
			if (i < 0 || i >= Board.Size || j < 0 || j >= Board.Size) { // ボード外
				if (i == 0 && j == -1) { // パス
					board.Move (i, j, IsBlackTurn);
				} else {
					throw new ArgumentOutOfRangeException ($"{(IsBlackTurn ? "black" : "white")} turn without a hand ({i}, {j})");
				}
			} else if (Enable (i, j)) {
				board.Move (i, j, IsBlackTurn);
			} else { // 置けない場所に置こうとした
				throw new InvalidOperationException ($"{(IsBlackTurn ? "black" : "white")} turn without a hand ({i}, {j}) {board.GetSquareStatus (i, j)}");
			}
			IsBlackTurn = !IsBlackTurn; // チェンジ
		}

		/// <summary>待った</summary>
		public void RetractMove () {
			board.RetractMove (IsBlackTurn);
		}

		/// <summary>文字列化</summary>
		public override string ToString () {
			return $"turn = {(IsBlackTurn ? "black" : "white")}\n{board}";
		}

	}

	/// <summary>打石の可能性</summary>
	[Flags] public enum Movability {
		End = 0,
		BlackEnable = 1,
		WhiteEnable = 2,
		BothEnable = BlackEnable | WhiteEnable,
	}

	/// <summary>ボードスコア</summary>
	public class BoardScore : IEquatable<BoardScore> {
		public Score Score;
		public Movability Status;
		public int Black { get => Score.Black; set => Score.Black = value; }
		public int White { get => Score.White; set => Score.White = value; }
		public BoardScore () { Score = new Score (); Status = 0; }
		public BoardScore (int black, int white, Movability status) { Score = new Score (black, white); this.Status = status; }
		public override string ToString () => $"({Black}, {White}, {Status})";
		public virtual bool Equals (BoardScore score) => score != null && EqualityComparer<Score>.Default.Equals (Score, score.Score) && Status == score.Status;
		public override bool Equals (object obj) => Equals (obj as Score);
		public override int GetHashCode () {
			var hashCode = 559342988;
			hashCode = hashCode * -1521134295 + EqualityComparer<Score>.Default.GetHashCode (Score);
			hashCode = hashCode * -1521134295 + Status.GetHashCode ();
			return hashCode;
		}
		public static bool operator == (BoardScore a, BoardScore b) => EqualityComparer<BoardScore>.Default.Equals (a, b);
		public static bool operator != (BoardScore a, BoardScore b) => !(a == b);
	}

	/// <summary>盤</summary>
	public class Board {

		/// <summary>サイズ</summary>
		public const int Size = 8;

		/// <summary>マスの行列</summary>
		private Square [,] matrix;

		/// <summary>差し手の記録</summary>
		private List<Move> trace;

		/// <summary>手数</summary>
		public int Step => trace?.Count ?? 0;

		/// <summary>最後の手</summary>
		public Move LastMove => (trace?.Count > 0) ? trace [trace.Count - 1] : null;

		/// <summary>更新未反映</summary>
		private bool dirty;

		/// <summary>マスへのアクセス</summary>
		public Square this [int i, int j] => (i >= 0 && i < Size && j >= 0 && j < Size) ? matrix [i, j] : null;

		/// <summary>マスへのアクセス</summary>
		public Square this [int index] => (index >= 0 && index < Size * Size) ? matrix [index / Size, index % Size] : null;

		/// <summary>石が置けるか</summary>
		public bool Enable (int index, bool black) {
			var flag = black ? SquareStatus.BlackEnable : SquareStatus.WhiteEnable;
			return (GetSquareStatus (index) & flag) == flag;
		}

		/// <summary>石が置けるか</summary>
		public bool Enable (int i, int j, bool black) {
			var flag = black ? SquareStatus.BlackEnable : SquareStatus.WhiteEnable;
			return (GetSquareStatus (i, j) & flag) == flag;
		}

		/// <summary>マスの状態を取得</summary>
		public SquareStatus? GetSquareStatus (int index) => GetSquareStatus (index / Size, index % Size);

		/// <summary>マスの状態を取得</summary>
		public SquareStatus? GetSquareStatus (int i, int j) {
			if (dirty) { _ = Score; }
			return squareStatuses [i, j];
		}

		/// <summary>ボードスコアキャッシュ</summary>
		private BoardScore boardScore;

		/// <summary>盤面状態キャッシュ</summary>
		private SquareStatus [,] squareStatuses;

		/// <summary>スコアと局面</summary>
		public BoardScore Score {
			get {
				if (dirty) {
					boardScore.Black = boardScore.White = 0;
					boardScore.Status = Movability.End;
					for (var i = 0; i < Size; i++) {
						for (var j = 0; j < Size; j++) {
							var s = getSquareStatus (i, j);
							squareStatuses [i, j] = s ?? SquareStatus.Empty;
							if (s.IsBlack ()) {
								boardScore.Black++;
							} else if (s.IsWhite ()) {
								boardScore.White++;
							} else {
								if (s.BlackEnable ()) {
									boardScore.Status |= Movability.BlackEnable;
								}
								if (s.WhiteEnable ()) {
									boardScore.Status |= Movability.WhiteEnable;
								}
							}
						}
					}
					dirty = false;
				}
				return boardScore;
			}
		}

		/// <summary>指定方向に異なる石が現れるまでスキャンする サブ</summary>
		private SquareStatus? scanStoneSub (int i, int j, int di, int dj, SquareStatus? status) {
			var deltaStatus = this [i + di, j + dj]?.Status;
			return (status == deltaStatus) ? scanStoneSub (i + di, j + dj, di, dj, deltaStatus) : deltaStatus;
		}

		/// <summary>指定方向に異なる石が現れるまでスキャンする</summary>
		private SquareStatus scanStone (int i, int j, int di, int dj) {
			var status = this [i + di, j + dj]?.Status;
			if (status.IsNotEmpty ()) {
				var opposite = scanStoneSub (i + di, j + dj, di, dj, status);
				if (status.Different (opposite)) {
					return opposite.Enabler () ?? SquareStatus.Empty;
				}
			}
			return SquareStatus.Empty;
		}

		/// <summary>マスの状態を算定</summary>
		private SquareStatus? getSquareStatus (int i, int j) {
			var status = this [i, j].Status;
			if (status == SquareStatus.Empty) {
				status |= 
					scanStone (i, j, -1, -1) |
					scanStone (i, j, -1, 0) |
					scanStone (i, j, -1, 1) |
					scanStone (i, j, 0, -1) |
					scanStone (i, j, 0, 0) |
					scanStone (i, j, 0, 1) |
					scanStone (i, j, 1, -1) |
					scanStone (i, j, 1, 0) |
					scanStone (i, j, 1, 1);
			}
			return status;
		}

		/// <summary>指定方向へ置き石を反映 サブ</summary>
		private SquareStatus? scanReverseSub (int i, int j, int di, int dj, SquareStatus? status, bool black) {
			var deltaStatus = this [i + di, j + dj]?.Status;
			var opposite = (status == deltaStatus) ? scanReverseSub (i + di, j + dj, di, dj, status, black) : deltaStatus;
			if (opposite.Exist (black)) {
				this [i, j].Reverse ();
			}
			return opposite;
		}

		/// <summary>指定方向へ置き石を反映</summary>
		private void scanReverse (int i, int j, int di, int dj, bool black) {
			var status = this [i + di, j + dj]?.Status;
			if (status.Exist (!black)) {
				scanReverseSub (i + di, j + dj, di, dj, status, black);
			}
		}

		/// <summary>石を置いて反映する</summary>
		public void Move (int i, int j, bool black) {
			var pass = (i == 0 && j == -1);
			if ((i >= 0 && i < Size && j >= 0 && j < Size) || pass) {
				trace.Add (new Move ((i, j), black));
				if (!pass) {
					var square = this [i, j];
					if (square.Status == SquareStatus.Empty) {
						square.EnBlack (black, Step);
						scanReverse (i, j, -1, -1, black);
						scanReverse (i, j, -1, 0, black);
						scanReverse (i, j, -1, 1, black);
						scanReverse (i, j, 0, -1, black);
						scanReverse (i, j, 0, 0, black);
						scanReverse (i, j, 0, 1, black);
						scanReverse (i, j, 1, -1, black);
						scanReverse (i, j, 1, 0, black);
						scanReverse (i, j, 1, 1, black);
						dirty = true;
					}
				}
			}
		}

		/// <summary>石を置いて反映する</summary>
		public void Move (int index, bool black) => Move (index / Size, index % Size, black);

		/// <summary>石を置いて反映する</summary>
		public void Move (Move move) => Move (move.Position.i, move.Position.j, move.IsBlack);

		/// <summary>待った</summary>
		public void RetractMove (bool black) {
			if (trace.Count > 0 && trace [trace.Count - 1].IsBlack == black) {
				trace.RemoveAt (trace.Count - 1);
			} else if (trace.Count > 1 && trace [trace.Count - 2].IsBlack == black) {
				trace.RemoveRange (trace.Count - 2, 2);
			} else return;
			Reset (redo: true);
		}

		/// <summary>コンストラクタ</summary>
		public Board () {
			trace = new List<Move> { };
			matrix = new Square [Size, Size];
			for (var i = 0; i < Size; i++) {
				for (var j = 0; j < Size; j++) {
					matrix [i, j] = new Square ();
				}
			}
			squareStatuses = new SquareStatus [Size, Size];
			boardScore = new BoardScore ();
			Reset (false);
		}

		/// <summary>初期配置</summary>
		/// <param name="init">初期化</param>
		/// <param name="redo">初期化後に再現を行う</param>
		public void Reset (bool init = true, bool redo = false) {
			var _trace = trace;
			if (init || redo) {
				trace = new List<Move> { };
				for (var i = 0; i < Size; i++) {
					for (var j = 0; j < Size; j++) {
						matrix [i, j].IsEmpty = true;
					}
				}
			}
			var halfIndex = Size / 2 - 1;
			matrix [halfIndex, halfIndex].EnWhite ();
			matrix [halfIndex, halfIndex + 1].EnBlack ();
			matrix [halfIndex + 1, halfIndex].EnBlack ();
			matrix [halfIndex + 1, halfIndex + 1].EnWhite ();
			if (redo) { // 盤面の再現
				foreach (var move in _trace) {
					Move (move);
				}
			}
			dirty = true;
		}

		/// <summary>文字列化</summary>
		public override string ToString () {
			var score = Score;
			var board = new string [Size];
			for (var i = 0; i < Size; i++) {
				var row = new List<Square> { };
				for (var j = 0; j < Size; j++) {
					row.Add (matrix [i, j]);
				}
				board [i] = string.Join ("", row.ConvertAll (s => s.ToString ()));
			}
			return $"Score = {score.Black} : {score.White} ({score.Status})\n{string.Join ("\n", board)}";
		}

	}

	/// <summary>マスの状態</summary>
	[Flags] public enum SquareStatus {
		Empty = 0,
		BlackEnable = 1,
		WhiteEnable = 2,
		BothEnable = BlackEnable | WhiteEnable,
		NotEmpty = 8,
		ExistColor = 4,
		BlackExist = NotEmpty,
		WhiteExist = NotEmpty | ExistColor,
		MaxValue = 15,
	}

	/// <summary>マスの状態の拡張</summary>
	public static class SquareStatusExtentions {

		/// <summary>黒石が存在</summary>
		public static bool IsBlack (this SquareStatus? status) => status == SquareStatus.BlackExist;

		/// <summary>白石が存在</summary>
		public static bool IsWhite (this SquareStatus? status) => status == SquareStatus.WhiteExist;

		/// <summary>双方に色の異なる石が存在</summary>
		public static bool Different (this SquareStatus? a, SquareStatus? b) => a.IsNotEmpty () && b.IsNotEmpty () && ((a ^ b) & SquareStatus.ExistColor) == SquareStatus.ExistColor;

		/// <summary>石が存在</summary>
		public static bool Exist (this SquareStatus? status, bool black) => status == (black ? SquareStatus.BlackExist : SquareStatus.WhiteExist);

		/// <summary>石が不存</summary>
		public static bool IsEmpty (this SquareStatus? status) => (status & SquareStatus.NotEmpty) != SquareStatus.NotEmpty;

		/// <summary>石が存在</summary>
		public static bool IsNotEmpty (this SquareStatus? status) => (status & SquareStatus.NotEmpty) == SquareStatus.NotEmpty;

		/// <summary>黒石が設置可</summary>
		public static bool BlackEnable (this SquareStatus? status) => (status & SquareStatus.BlackEnable) == SquareStatus.BlackEnable;

		/// <summary>白石が設置可</summary>
		public static bool WhiteEnable (this SquareStatus? status) => (status & SquareStatus.WhiteEnable) == SquareStatus.WhiteEnable;

		/// <summary>どちらでも設置可</summary>
		public static bool BothEnable (this SquareStatus? status) => BlackEnable (status) && WhiteEnable (status);

		/// <summary>何も置けない</summary>
		public static bool NotEnable (this SquareStatus? status) => !BlackEnable (status) && !WhiteEnable (status);

		/// <summary>石の所在を可能性として返す</summary>
		public static SquareStatus? Enabler (this SquareStatus? status) {
			switch (status) {
				case SquareStatus.BlackExist:
					return SquareStatus.BlackEnable;
				case SquareStatus.WhiteExist:
					return SquareStatus.WhiteEnable;
			}
			return status;
		}

	}

	/// <summary>マス</summary>
	public class Square {

		/// <summary>基礎状態</summary>
		public SquareStatus? Status { get; private set; }

		/// <summary>石が置かれたステップ</summary>
		public int Step { get; private set; }

		/// <summary>石が不在</summary>
		public bool IsEmpty {
			get => (Status & SquareStatus.NotEmpty) != SquareStatus.NotEmpty;
			set {
				Status = value ? SquareStatus.Empty : SquareStatus.NotEmpty;
				Step = -1;
			}
		}

		/// <summary>石が存在</summary>
		public bool IsNotEmpty {
			get => (Status & SquareStatus.NotEmpty) == SquareStatus.NotEmpty;
			set {
				Status = value ? SquareStatus.NotEmpty : SquareStatus.Empty;
				Step = -1;
			}
		}

		/// <summary>黒石の所在</summary>
		public bool IsBlack => Status == SquareStatus.BlackExist;

		/// <summary>黒石の設置</summary>
		public void EnBlack (bool black = true, int step = -1) {
			if (IsEmpty) { Step = step; }
			Status = black ? SquareStatus.BlackExist : SquareStatus.WhiteExist;
		}

		/// <summary>白石の所在</summary>
		public bool IsWhite => Status == SquareStatus.WhiteExist;

		/// <summary>白石の設置</summary>
		public void EnWhite (bool white = true, int step = -1) => EnBlack (!white, step);

		/// <summary>石を裏返す</summary>
		public void Reverse () {
			switch (Status) {
				case SquareStatus.BlackExist:
					Status = SquareStatus.WhiteExist;
					break;
				case SquareStatus.WhiteExist:
					Status = SquareStatus.BlackExist;
					break;
			}
		}

		/// <summary>コンストラクタ</summary>
		public Square () {
			Status = SquareStatus.Empty;
			Step = -1;
		}

		/// <summary>文字列化</summary>
		public override string ToString () {
			return IsBlack ? "●" : IsWhite ? "○" : "・";
		}

	}

	/// <summary>手</summary>
	public class Move {

		/// <summary>手番</summary>
		public bool IsBlack { get; private set; }

		/// <summary>位置</summary>
		public (int i, int j) Position { get; private set; }

		/// <summary>位置</summary>
		public int Index {
			get => Position.i * Board.Size + Position.j;
			private set => Position = (value / Board.Size, value % Board.Size);
		}

		/// <summary>コンストラクタ</summary>
		public Move () { Position = (-1, -1); }

		/// <summary>コンストラクタ</summary>
		public Move ((int i, int j) position, bool isBlack) {
			Position = position;
			IsBlack = isBlack;
		}

		/// <summary>コンストラクタ</summary>
		public Move (int index, bool isBlack) {
			Index = index;
			IsBlack = isBlack;
		}

	}

}
