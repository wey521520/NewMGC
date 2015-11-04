using UnityEngine;
using UnityEngine.UI;

public class SingleKey : MonoBehaviour
{
	public OperateStep mystep;
	private MyMagicCube operater;

	void Start ()
	{
		operater = FindObjectOfType <MyMagicCube> ();
		GetComponent <Button> ().onClick.AddListener (OnClick);
	}

	public void OnClick ()
	{
		switch (operater.mystate) {
		case State.Operate:
			operater.DoSingleStep (mystep);
			break;
		case State.EditFormula:
			operater.FormularAdd (mystep);
			break;
		case State.OperateAndFormula:
			operater.DoSingleStep (mystep);
			operater.FormularAdd (mystep);
			break;
		case State.EditColor:
			operater.DoSingleStep (mystep);
			break;
		}
	}
}
