using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class KeyBoard : MonoBehaviour
{
	public GameObject keyprefab;
	private GameObject[] mykeys;
	private RectTransform tran;
	private float sizelength;

	void Start ()
	{
		tran = GetComponent <RectTransform> ();
		sizelength = Mathf.Min (tran.rect.width, tran.rect.height);
		Init ();
		print (tran.rect);
	}

	void Init ()
	{
		mykeys = new GameObject[12];
		for (int i = 0; i < mykeys.Length; i++) {
			mykeys [i] = Instantiate (keyprefab);
			RectTransform tr = mykeys [i].GetComponent<RectTransform> ();
			tr.SetParent (this.transform);
//			tr.sizeDelta = Vector2.one * sizelength / 4f;
			tr.anchorMax = Vector2.zero;
			tr.anchorMin = Vector2.zero;
			tr.offsetMax = Vector2.one * sizelength / 10f;
			tr.offsetMin = -Vector2.one * sizelength / 10f;
		}
		mykeys [0].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (sizelength * 1f / 3f, sizelength * 7f / 8f);
		mykeys [1].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (sizelength * 2f / 3f, sizelength * 7f / 8f);
		mykeys [2].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (sizelength * 1f / 8f, sizelength * 2f / 3f);
		mykeys [3].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (sizelength * 1f / 2f, sizelength * 2f / 3f);
		mykeys [4].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (sizelength * 7f / 8f, sizelength * 2f / 3f);
		mykeys [5].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (sizelength * 1f / 3f, sizelength * 1f / 2f);
		mykeys [6].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (sizelength * 2f / 3f, sizelength * 1f / 2f);
		mykeys [7].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (sizelength * 1f / 8f, sizelength * 1f / 3f);
		mykeys [8].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (sizelength * 1f / 2f, sizelength * 1f / 3f);
		mykeys [9].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (sizelength * 7f / 8f, sizelength * 1f / 3f);
		mykeys [10].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (sizelength * 1f / 3f, sizelength * 1f / 8f);
		mykeys [11].GetComponent<RectTransform> ().anchoredPosition = new Vector2 (sizelength * 2f / 3f, sizelength * 1f / 8f);

		mykeys [0].GetComponent<SingleKey> ().mystep = OperateStep.U0;
		mykeys [1].GetComponent<SingleKey> ().mystep = OperateStep.U;
		mykeys [2].GetComponent<SingleKey> ().mystep = OperateStep.L0;
		mykeys [3].GetComponent<SingleKey> ().mystep = OperateStep.B0;
		mykeys [4].GetComponent<SingleKey> ().mystep = OperateStep.R;
		mykeys [5].GetComponent<SingleKey> ().mystep = OperateStep.F;
		mykeys [6].GetComponent<SingleKey> ().mystep = OperateStep.B;
		mykeys [7].GetComponent<SingleKey> ().mystep = OperateStep.L;
		mykeys [8].GetComponent<SingleKey> ().mystep = OperateStep.F0;
		mykeys [9].GetComponent<SingleKey> ().mystep = OperateStep.R0;
		mykeys [10].GetComponent<SingleKey> ().mystep = OperateStep.D;
		mykeys [11].GetComponent<SingleKey> ().mystep = OperateStep.D0;
		print (sizelength);

		for (int i = 0; i < mykeys.Length; i++) {
			mykeys [i].name = mykeys [i].GetComponent<SingleKey> ().mystep.ToString ();
			mykeys [i].GetComponent<Image> ().sprite = Global.LoadSprite ("Key" + mykeys [i].name);
			mykeys [i].GetComponentInChildren   <Text> ().text = mykeys [i].name;
		}
	}
}
