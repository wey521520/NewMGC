using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MyUIControler : MonoBehaviour
{
	public RectTransform[] Pages;
	private RectTransform UPage;
	private RectTransform DPage;
	private RectTransform LPage;
	private RectTransform RPage;
	private RectTransform FPage;
	private RectTransform BPage;

	private Vector2 dragoffset;
	private float inertlength;
	private float staytime;
	private float steptime = 3f;
	private float space = 0.9f;

	//	bool operate;
	// Use this for initialization
	void Start ()
	{
		InitUI ();
		inertlength = 0.1f * Mathf.Min (Screen.width, Screen.height);
	}

	void InitUI ()
	{
		FPage = Pages [0];
		UPage = Pages [1];
		RPage = Pages [2];
		BPage = Pages [3];
		DPage = Pages [4];
		LPage = Pages [5];
		SetPagePosition ();
	}

	void SetPagePosition ()
	{
		for (int i = 0; i < Pages.Length; i++) {
			Pages [i].anchorMax = new  Vector2 (1f - (1f - space) / 2f, 1f - (1f - space) / 2f);// Vector2.one * space;
			Pages [i].anchorMin = new  Vector2 ((1f - space) / 2f, (1f - space) / 2f);// Vector2.zero * space;
		}
		FPage.anchoredPosition = Vector2.zero * space;
		UPage.anchoredPosition = new Vector2 (0f, Screen.height) * space;
		RPage.anchoredPosition = new Vector2 (Screen.width, 0f) * space;
		BPage.anchoredPosition = new Vector2 (Screen.width * 2f, 0f) * space;
		DPage.anchoredPosition = new Vector2 (0f, -Screen.height) * space;
		LPage.anchoredPosition = new Vector2 (-Screen.width, 0f) * space;
	}

	void SlidePage (SlideDirection dir)
	{
		RectTransform curpage = null;
		switch (dir) {
		case SlideDirection.Up:
			curpage = FPage;
			FPage = DPage;
			DPage = BPage;
			BPage = UPage;
			UPage = curpage;
			break;
		case SlideDirection.Down:
			curpage = FPage;
			FPage = UPage;
			UPage = BPage;
			BPage = DPage;
			DPage = curpage;
			break;
		case SlideDirection.Left:
			curpage = FPage;
			FPage = RPage;
			RPage = BPage;
			BPage = LPage;
			LPage = curpage;
			break;
		case SlideDirection.Right:
			curpage = FPage;
			FPage = LPage;
			LPage = BPage;
			BPage = RPage;
			RPage = curpage;
			break;
		}
		SetPagePosition ();
	}
	
	// Update is called once per frame
	void Update ()
	{
//		print (Input.touchCount + " " + steptime);
		if (Input.touchCount > 0 || Input.GetMouseButton (0)) {
//			operate = true;
			staytime = 0;
			steptime = 6f;
		} else {
//			operate = false;
			staytime += Time.deltaTime;
			if (staytime > steptime) {
				int i = Random.Range (0, 1000);
				SlideDirection dir = (SlideDirection)(i % 4);
				SlidePage (dir);
				staytime = 0f;
				steptime = 3f;
			}
		}
	}

	bool changepages;

	public void OnDraging (BaseEventData data)
	{
		PointerEventData e = (PointerEventData)data;
		if (!changepages) {
			dragoffset += e.delta;
			if (Mathf.Abs (dragoffset.x) > inertlength || Mathf.Abs (dragoffset.y) > inertlength) {
				changepages = true;
				if (dragoffset.x > inertlength) {
					SlidePage (SlideDirection.Right);
				} else if (dragoffset.x < -inertlength) {
					SlidePage (SlideDirection.Left);
				} else if (dragoffset.y > inertlength) {
					SlidePage (SlideDirection.Up);
				} else if (dragoffset.y < -inertlength) {
					SlidePage (SlideDirection.Down);
				}
			}
		}
	}

	public void OnDragEnd ()
	{
		dragoffset = Vector2.zero;
		changepages = false;
		staytime = 0f;
	}

	public void OnClick (string val)
	{
		switch (val) {
		case "operate":
			Global.State = Global.GameState.Operate;
			break;
		case "learn":
			Global.State = Global.GameState.Learn;
			break;
		case "exercise":
			Global.State = Global.GameState.Exercise;
			break;
		case "color":
			Global.State = Global.GameState.Color;
			break;
		case "formular":
			Global.State = Global.GameState.Formular;
			break;
		case "setting":
			Global.State = Global.GameState.Setting;
			break;
		}
		if (val == "setting") {
			Application.LoadLevel ("OtherFucntions");
		} else {
			Application.LoadLevel ("GameScene");
		}
	}
}

public enum SlideDirection
{
	Up,
	Down,
	Left,
	Right
}