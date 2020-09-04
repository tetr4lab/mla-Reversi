using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReversiLogic;

namespace ReversiGame {

	/// <summary>物理マス</summary>
	public class SquareObject : MonoBehaviour, IPointerClickHandler {

		#region Static

		/// <summary>プレハブのパス</summary>
		private const string prefabPath = "Prefabs/Square";
		/// <summary>プレハブ</summary>
		private static GameObject prefab = null;

		/// <summary>生成</summary>
		public static SquareObject Create (Transform parent, Game game, int index) {
			if (!prefab) { prefab = Resources.Load<GameObject> (prefabPath); }
			if (!prefab) { throw new MissingComponentException ($"resources not found '{prefabPath}'"); }
			var instance = Instantiate (prefab, parent)?.GetComponent<SquareObject> ();
			instance?.initialize (parent, game, index);
			return instance;
		}

		#endregion

		/// <summary>物理ゲーム</summary>
		private Game game;
		/// <summary>マスの番号</summary>
		private int index;
		/// <summary>石の表示アニメ</summary>
		private Animator animator;

		/// <summary>初期化</summary>
		private void initialize (Transform parent, Game game, int index) {
			transform.SetAsLastSibling ();
			this.game = game;
			this.index = index;
			animator = GetComponentInChildren<Animator> ();
		}

		/// <summary>表示更新要求</summary>
		public void RequestUpdate () {
			if (animator) {
				var status = game [index].Status;
				animator.SetBool ("NotEmpty", status.NotEmpty ());
				animator.SetBool ("Black", status.Black ());
			}
		}

		/// <summary>マスのクリック</summary>
		public void OnPointerClick (PointerEventData eventData) {
			if (game.HumanTurn && !game.TurnAgent) { game.Move (index); }
		}

	}

}
