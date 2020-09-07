using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ReversiGame {

	/// <summary>確認ダイアログ</summary>
	public class Confirm : MonoBehaviour {

		#region Static

		/// <summary>排他制御用</summary>
		public static bool OnMode => instances?.Count > 0;

		/// <summary>プレハブのパス</summary>
		private const string prefabPath = "Prefabs/Confirm";
		/// <summary>プレハブ</summary>
		private static GameObject prefab = null;
		/// <summary>インスタンスのリスト</summary>
		private static List<Confirm> instances = null;

		/// <summary>生成</summary>
		/// <param name="parent">親</param>
		/// <param name="okLabel">ボタンラベル</param>
		/// <param name="okCall">ボタンが押されたときの処理</param>
		/// <returns></returns>
		public static Confirm Create (Transform parent, string message = "", string okLabel = "", UnityAction okCall = null, string cancelLabel = "", UnityAction cancelCall = null, UnityAction postCall = null) {
			if (!prefab) { prefab = Resources.Load<GameObject> (prefabPath); }
			if (!prefab) { throw new MissingComponentException ($"resources not found '{prefabPath}'"); }
			if (instances == null) { instances = new List<Confirm> { }; }
			var instance = Instantiate (prefab, parent)?.GetComponent<Confirm> ();
			instances.Add (instance);
			instance?.initialize (parent, message, okLabel, okCall, cancelLabel, cancelCall, postCall);
			return instance;
		}

		#endregion

		/// <summary>終了中</summary>
		private bool termination = false;

		/// <summary>初期化</summary>
		private void initialize (Transform parent, string message, string okLabel, UnityAction okCall, string cancelLabel, UnityAction cancelCall, UnityAction postCall) {
			transform.SetAsLastSibling ();
			var text = GetComponentInChildren<Text> ();
			var buttons = GetComponentsInChildren<Button> ();
			text.text = message ?? "";
			if (buttons.Length > 0) {
				if (okLabel == null) {
					buttons [0].gameObject.SetActive (false);
				} else {
					var Llabel0 = buttons [0].GetComponentInChildren<Text> ();
					if (Llabel0 && okLabel != "") { Llabel0.text = okLabel; }
					buttons [0].onClick.AddListener (() => {
						if (!termination) {
							termination = true;
							okCall?.Invoke ();
							postCall?.Invoke ();
							Destroy (gameObject, 0.016f);
						}
					});
				}
			}
			if (buttons.Length > 1) {
				if (cancelLabel == null) {
					buttons [1].gameObject.SetActive (false);
				} else {
					var Llabel0 = buttons [1].GetComponentInChildren<Text> ();
					if (Llabel0 && cancelLabel != "") { Llabel0.text = cancelLabel; }
					buttons [1].onClick.AddListener (() => {
						if (!termination) {
							termination = true;
							cancelCall?.Invoke ();
							postCall?.Invoke ();
							Destroy (gameObject, 0.016f);
						}
					});
				}
			}
		}

		/// <summary>破棄</summary>
		private void OnDestroy () {
			instances.Remove (this);
		}

	}

}
