using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
///	対象のオブジェクトをセーフエリアに整合する
///	オブジェクトは、ルートキャンバスの直下に、
///	アンカー(0, 0)-(1, 1)、オフセット(0, 0)-(0, 0)で、
///	配置されている必要がある。
/// </summary>
public class SetCanvasBounds : MonoBehaviour {

	#region static
	public static Rect GetSafeArea {
		get {
			Rect safeArea = Screen.safeArea;
			#if UNITY_ANDROID || UNITY_EDITOR
			float limit = 16.1f / 9.0f;	// これより細長ければセーフエリアを作る
			if (safeArea.width / safeArea.height >= limit) { // landscape
				safeArea.x = 44f / 812f * Screen.width;
				safeArea.width = 724f / 812f * Screen.width;
				#if UNITY_IOS
				safeArea.y = 21f / 375f * Screen.height;
				safeArea.height = 354f / 375f * Screen.height;
				#endif
			} else if (safeArea.height / safeArea.width >= limit) { // portrate
				safeArea.y = 34f / 812f * Screen.height;
				safeArea.height = 734f / 812f * Screen.height;
			}
			#endif
			return new Rect (safeArea);
		}
	}
	#endregion

	[SerializeField] private RectTransform panel = default;
	[SerializeField] private bool HorizontalSafety = default;
	[SerializeField] private bool VerticalSafety = default;
	private Rect lastSafeArea = Rect.zero;

	private void Start () {
		if (panel == null) {
			panel = GetComponent<RectTransform> ();
		}
	}

	private void Update () {
		if (panel != null) {
			Rect area = GetSafeArea;
			if (area != lastSafeArea) {
				var screenSize = new Vector2 (Screen.width, Screen.height);
				panel.anchorMin = new Vector2 (HorizontalSafety ? area.position.x / screenSize.x : 0, VerticalSafety ? area.position.y / screenSize.y : 0);
				panel.anchorMax = new Vector2 (HorizontalSafety ? (area.position.x + area.size.x) / screenSize.x : 1, VerticalSafety ? (area.position.y + area.size.y) / screenSize.y : 1);
				lastSafeArea = area;
			}
		}
	}

}
