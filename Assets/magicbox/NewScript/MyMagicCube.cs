using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Reflection;
using LitJson;

public class MyMagicCube : MonoBehaviour
{
	#region Creat A Cube

	// 单纯的魔方单块的模型（不具备操作特性，即捕捉点击滑动等）
	private GameObject CubePrefab;

	public GameObject[] cubestylesprefab;

	// 魔方的（9*6）54个单元面，用来捕捉鼠标或touch事件
	public GameObject FacePrefab;

	//这个需要跟摄像机一起改，以后大概是不方便改了
	private const float CubeSize = 1f;
	//魔方的阶数
	private const int CubeClass = 3;

	private SingleCube[] Cubes;
	private CubeFace[] cubefaces;

	#endregion

	#region Operate The Cube

	public State mystate = State.Operate;
	// 旋转
	private bool rotating;
	// 整体旋转（双指操作）
	private bool flipping;

	public bool Stop{ get { return !rotating && !flipping; } }

	private bool rotatejudged;
	private bool flipjudged;
	// 单步动画所需时间
	public  float singleanitime = 0.4f;
	// 联动系数（用于滑动所产生的偏移或角度，自动适配计算）
	private float rlength;

	List<SingleCube> operatelist = new List<SingleCube> ();

	//是否正在执行公式，正在执行公式时不可以对魔方进行操作。
	private bool formularing;

	// 以后魔方的操作分为两种模式，一种拖拽操作，另外一种公式操作，
	// 两种操作方式都需要记录用户的操作步骤，
	// 增加功能，回放我的操作，除非用户重新开始，否则用户的操作不会被用户主动清除。
	// 增加功能，精简公式，精简自己的操作，判断是否有重复无用的操作。
	// 用户编辑公式的时候是不需要记录的，因为编辑完成的公式就是记录。

	#endregion

	#region Edit Cube Color

	// 当前编辑所选中的颜色
	private MagicColor curColor = MagicColor.White;

	public MagicColor Color { get { return curColor; } }

	public Dictionary<SingleCube,CubeMark> EditColorMap = new Dictionary<SingleCube, CubeMark> ();
	public Dictionary<MagicColor,int> EditColorStore = new Dictionary<MagicColor, int> ();

	#endregion

	#region accomplish

	public Dictionary<CubeFaceStyle,MagicColor> accomplishstate = new Dictionary<CubeFaceStyle, MagicColor> ();

	private List<JsonData> FormulaLibraryData = new List<JsonData> ();
	//	{"facemap":[6,6,6,6,0,6,6,6,6,6,6,6,6,4,6,6,4,6,6,6,6,6,2,6,6,2,6,6,3,6,1,3,3,6,3,6,6,6,6,6,1,6,6,3,6,6,6,6,6,5,6,6,5,6],"formula":"R,U',B,U"}

	#endregion

	void Start ()
	{
		// rlength = 180f / Screen.width / 0.23f / 3.14f;
		rlength = 249f / Screen.width;// 上式计算后得

		CreatCube ();

		InitColorstore ();

		InitScene ();

//		FormulaLibrary ();

		// 初步整理的公式
//		@"{""name"":""顶部十字（小白花）"",""facemap"":[6,6,6,6,0,6,6,6,6,6,6,6,6,4,6,6,4,6,6,6,6,6,2,6,6,2,6,6,3,6,1,3,3,6,3,6,6,6,6,6,1,6,6,3,6,6,6,6,6,5,6,6,5,6],""formula"":""R,U',B,U""}"
//		@"{""name"":""底部角块"",""facemap"":[6,0,0,0,0,0,0,0,0,4,4,4,6,4,6,6,6,6,6,2,2,6,2,6,1,6,6,0,6,6,6,3,6,6,6,6,6,1,1,6,1,6,2,6,6,5,5,5,6,5,6,6,6,6],""formula"":""R,U,R',U',R,U,R',U',R,U,R',U'""}"
//		@"{""name"":""第二层棱块（左）"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,6,4,6,6,6,6,2,2,2,6,2,6,6,2,6,6,1,6,6,3,6,6,6,6,1,1,1,6,1,6,6,6,6,5,5,5,6,5,6,6,6,6],""formula"":""U,R,U',R',U',F',U,F"",""formula2"":""F,U,F,U,F,U',F',U',F'""}"
//		@"{""name"":""第二层棱块（右）"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,6,4,6,6,6,6,2,2,2,6,2,6,6,6,6,6,6,6,2,3,6,6,6,6,1,1,1,6,1,6,6,1,6,5,5,5,6,5,6,6,6,6],""formula"":""U',F',U,F,U,R,U',R'"",""formula2"":""R',U',R',U',R',U,R,U,R""}"
//		@"{""name"":""顶部单点"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,6,3,6,2,2,2,2,2,2,6,3,6,6,6,6,6,3,6,6,6,6,1,1,1,1,1,1,6,3,6,5,5,5,5,5,5,6,3,6],""formula"":""F,R,U,R',U',F'""}"
//		@"{""name"":""顶部拐角"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,6,6,6,2,2,2,2,2,2,6,3,6,6,6,6,6,3,3,6,3,6,1,1,1,1,1,1,6,3,6,5,5,5,5,5,5,6,6,6],""formula"":""F,R,U,R',U',F'""}"
//		@"{""name"":""顶部直线"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,6,6,6,2,2,2,2,2,2,6,3,6,6,6,6,3,3,3,6,6,6,1,1,1,1,1,1,6,6,6,5,5,5,5,5,5,6,3,6],""formula"":""F,R,U,R',U',F'""}"
//		@"{""name"":""顶部十字对面错位"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,6,1,6,2,2,2,2,2,2,6,2,6,6,3,6,3,3,3,6,3,6,1,1,1,1,1,1,6,4,6,5,5,5,5,5,5,6,5,6],""formula"":""R,U,R',U,R,U2,R'""}"
//		@"{""name"":""顶部十字相邻错位"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,6,2,6,2,2,2,2,2,2,6,4,6,6,3,6,3,3,3,6,3,6,1,1,1,1,1,1,6,1,6,5,5,5,5,5,5,6,5,6],""formula"":""R,U,R',U,R,U2,R',U""}"
//		@"{""name"":""顶部十字完成，整面对角顶面未完成"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,6,4,3,2,2,2,2,2,2,3,2,6,6,3,3,3,3,3,3,3,6,1,1,1,1,1,1,6,1,6,5,5,5,5,5,5,6,5,6],""formula"":""U,R,U',L',U,R',U',L""}"
//		@"{""name"":""顶部十字完成，整面相邻顶面未完成"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,6,4,6,2,2,2,2,2,2,3,2,3,6,3,6,3,3,3,3,3,3,1,1,1,1,1,1,6,1,6,5,5,5,5,5,5,6,5,6],""formula"":""U,R,U',L',U,R',U',L""}"
//		@"{""name"":""顶部十字完成，整面相邻顶面未完成2"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,6,4,6,2,2,2,2,2,2,3,2,6,6,3,3,3,3,3,6,3,3,1,1,1,1,1,1,6,1,6,5,5,5,5,5,5,3,5,6],""formula"":""U,R,U',L',U,R',U',L""}"
//		@"{""name"":""顶部十字完成，整面其他顶面未完成"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,6,4,6,2,2,2,2,2,2,3,2,3,6,3,6,3,3,3,6,3,6,1,1,1,1,1,1,6,1,6,5,5,5,5,5,5,3,5,3],""formula"":""U,R,U',L',U,R',U',L""}"
//		@"{""name"":""顶部十字完成，整面其他顶面未完成2"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,3,4,3,2,2,2,2,2,2,3,2,6,6,3,6,3,3,3,6,3,6,1,1,1,1,1,1,6,1,6,5,5,5,5,5,5,3,5,6],""formula"":""R,U,R',U,R,U2,R',U""}"
//		@"{""name"":""顶部小鱼左"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,6,4,3,2,2,2,2,2,2,6,2,3,3,3,6,3,3,3,6,3,6,1,1,1,1,1,1,6,1,6,5,5,5,5,5,5,3,5,6],""formula"":""U,R,U',L',U,R',U',L""}"
//		@"{""name"":""顶部小鱼右"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,3,4,6,2,2,2,2,2,2,6,2,6,3,3,6,3,3,3,6,3,6,1,1,1,1,1,1,6,1,3,5,5,5,5,5,5,6,5,3],""formula"":""U,R,U',L',U,R',U',L""}"


//		string str = 
//			@"{""name"":""顶部小鱼右"",""facemap"":[0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,3,4,6,2,2,2,2,2,2,6,2,6,3,3,6,3,3,3,6,3,6,1,1,1,1,1,1,6,1,3,5,5,5,5,5,5,6,5,3],""formula"":""U,R,U',L',U,R',U',L""}";
//		List<int> a = new List<int> ();
//		JsonData jsdata = JsonMapper.ToObject (str);
//		for (int i = 0; i < jsdata ["facemap"].Count; i++) {
//			a.Add ((int)jsdata ["facemap"] [i]);
//		}
//		userrecord = jsdata ["formula"].ToString ();
//		SetStoreState (a);
	}

	void InitScene ()
	{
		if (Global.State == Global.GameState.Operate || Global.State == Global.GameState.Formular) {
			SetFullColor ();
		} else if (Global.State == Global.GameState.Color) {
			SetEditColor ();
		} else if (Global.State == Global.GameState.Exercise || Global.State == Global.GameState.Learn) {
			//设置一个公式库唯一标志号，用来调用公式场景和所需公式。
//			SetFullColor ();
		}
	}

	void InitColorstore ()
	{
		EditColorStore.Add (MagicColor.White, 0);
		EditColorStore.Add (MagicColor.Red, 0);
		EditColorStore.Add (MagicColor.Yellow, 0);
		EditColorStore.Add (MagicColor.Green, 0);
		EditColorStore.Add (MagicColor.Blue, 0);
		EditColorStore.Add (MagicColor.Orange, 0);
		EditColorStore.Add (MagicColor.None, 0);
	}

	#region creat

	// 修改适用于N阶魔方的初始化
	void CreatCube ()
	{
		CubePrefab = cubestylesprefab [(int)Global.cubestyle];
		if (CubePrefab == null || FacePrefab == null) {
			Debug.LogWarning ("找不到单体！cann't find the cube or face!");
			return;
		}
			
		int numInClass = CubeClass * CubeClass;
		int numInCube = numInClass * CubeClass;

		Cubes = new SingleCube[numInCube];

		// 先实例化魔方的块（这里只实例化实际的东西，初始化的时候再判断游戏的模式）
		for (int i = 0; i < Cubes.Length; i++) {
			
			GameObject obj = Instantiate (CubePrefab);
			obj.transform.SetParent (this.transform);
			obj.transform.localScale = Vector3.one * CubeSize * 0.99f * 3f / CubeClass;
			obj.transform.position = new Vector3 (-1.5f + 1.5f / CubeClass + 3f / CubeClass * ((i % numInClass) / CubeClass),
				-1.5f + 1.5f / CubeClass + 3f / CubeClass * (i / numInClass),
				-1.5f + 1.5f / CubeClass + 3f / CubeClass * (i % CubeClass)) * CubeSize;
			obj.name = "C_" + i.ToString ("D2") + "_" + obj.transform.position;

			SingleCube cube = obj.GetComponent <SingleCube> ();
			Cubes [i] = cube;

			cube.index = i;
			cube.Size = CubeSize;

			// 判断各个块的属性，为块进行分类，角块，面块和棱块，和内部方块
			if (i == 0 || i == CubeClass - 1 || i == numInClass - CubeClass || i == numInClass - 1 ||
			    i == numInCube - 1 || i == numInCube - CubeClass ||
			    i == numInCube - numInClass + CubeClass - 1 || i == numInCube - numInClass) {
				cube.cubestyle = CubeStyle.Corner;
			} else if (i > numInClass && i < numInCube - numInClass && i % numInClass > CubeClass && i % numInClass < numInClass - CubeClass &&
			           i % numInClass % CubeClass != 0 && i % numInClass % CubeClass != CubeClass - 1) {
				cube.cubestyle = CubeStyle.None;
			} else if ((i > CubeClass && i < numInClass - CubeClass && i % CubeClass != 0 && i % CubeClass != CubeClass - 1) ||
			           (i < numInCube - CubeClass && i > numInCube - numInClass + CubeClass && i % CubeClass != 0 && i % CubeClass != CubeClass - 1) ||
			           i > numInClass && i < numInCube - numInClass && (((i % numInClass < CubeClass || i % numInClass > numInClass - CubeClass) &&
			           i % CubeClass != 0 && i % CubeClass != CubeClass - 1) || (i % numInClass >= CubeClass && i % numInClass < numInClass - CubeClass &&
			           (i % CubeClass == 0 || i % CubeClass == CubeClass - 1)))) {
				cube.cubestyle = CubeStyle.Face;
			} else {
				cube.cubestyle = CubeStyle.Edge;
			}
		}

		cubefaces = new CubeFace[6 * numInClass];
		for (int i = 0; i < cubefaces.Length; i++) {
			GameObject objface = Instantiate (FacePrefab);
			cubefaces [i] = objface.GetComponent <CubeFace> ();

			int j = 0;
			int l = 0;
			if (i < numInClass) {
				j = i;
				l = j;
				objface.transform.position = new Vector3 (-1.5f + 1.5f / CubeClass + 3f / CubeClass * (j / CubeClass),
					-1.5f, -1.5f + 1.5f / CubeClass + 3f / CubeClass * (j % CubeClass)) * CubeSize;
				objface.transform.eulerAngles = new Vector3 (-90, 0, 0);
				cubefaces [i].facestyle = CubeFaceStyle.Down;
			} else if (i < numInClass * 2) {
				j = i % numInClass;
				l = (j / CubeClass) * numInClass + (j % CubeClass) * CubeClass + CubeClass - 1;
				objface.transform.position = new Vector3 (-1.5f + 1.5f / CubeClass + 3f / CubeClass * (j % CubeClass)
					, -1.5f + 1.5f / CubeClass + 3f / CubeClass * (j / CubeClass), 1.5f) * CubeSize;
				objface.transform.eulerAngles = new Vector3 (180, 0, 0);
				cubefaces [i].facestyle = CubeFaceStyle.Front;
			} else if (i < numInClass * 3) {
				j = i % numInClass;
				l = (j / CubeClass) * numInClass + j % CubeClass;
				objface.transform.position = new Vector3 (-1.5f, -1.5f + 1.5f / CubeClass + 3f / CubeClass * (j / CubeClass),
					-1.5f + 1.5f / CubeClass + 3f / CubeClass * (j % CubeClass)) * CubeSize;
				objface.transform.eulerAngles = new Vector3 (0, 90, 0);
				cubefaces [i].facestyle = CubeFaceStyle.Right;
			} else if (i < numInClass * 4) {
				j = i % numInClass;
				l = j + numInClass * (CubeClass - 1);
				objface.transform.position = new Vector3 (-1.5f + 1.5f / CubeClass + 3f / CubeClass * (j / CubeClass), 1.5f,
					-1.5f + 1.5f / CubeClass + 3f / CubeClass * (j % CubeClass)) * CubeSize;
				objface.transform.eulerAngles = new Vector3 (90, 0, 0);
				cubefaces [i].facestyle = CubeFaceStyle.Up;
			} else if (i < numInClass * 5) {
				j = i % numInClass;
				l = (j / CubeClass) * numInClass + (j % CubeClass) * CubeClass;
				objface.transform.position = new Vector3 (-1.5f + 1.5f / CubeClass + 3f / CubeClass * (j % CubeClass),
					-1.5f + 1.5f / CubeClass + 3f / CubeClass * (j / CubeClass), -1.5f) * CubeSize;
				objface.transform.eulerAngles = new Vector3 (0, 0, 0);
				cubefaces [i].facestyle = CubeFaceStyle.Left;
			} else if (i < numInClass * 6) {
				j = i % numInClass;
				l = (j / CubeClass) * numInClass + j % CubeClass + numInClass - CubeClass;
				objface.transform.position = new Vector3 (1.5f, -1.5f + 1.5f / CubeClass + 3f / CubeClass * (j / CubeClass),
					-1.5f + 1.5f / CubeClass + 3f / CubeClass * (j % CubeClass)) * CubeSize;
				objface.transform.eulerAngles = new Vector3 (0, -90, 0);
				cubefaces [i].facestyle = CubeFaceStyle.Back;
			}

			objface.transform.SetParent (Cubes [l].transform);

			objface.transform.localScale = Vector3.one * CubeSize * 0.9f /** 3f / CubeClass*/;
			objface.name = "Face" + i.ToString ("D2");
			cubefaces [i].cube = Cubes [l];
			cubefaces [i].operater = this;
		}
		RecordMagicCubeState ();
	}

	#endregion

	#region 记录魔方状态，还原魔方状态

	private List<Vector3> positionlist = new List<Vector3> ();
	private List<Vector3> rotationlist = new List<Vector3> ();
	private List<MagicColor> colorlist = new List<MagicColor> ();

	public void RecordMagicCubeState ()
	{
		positionlist.Clear ();
		rotationlist.Clear ();
		colorlist.Clear ();
		for (int i = 0; i < Cubes.Length; i++) {
			positionlist.Add (Cubes [i].transform.position);
			rotationlist.Add (Cubes [i].transform.eulerAngles);
		}
		for (int j = 0; j < cubefaces.Length; j++) {
			colorlist.Add (cubefaces [j].mycolor);
		}
	}
	// 当前的存储只能存储一个魔方状态，至于以后，可能还需要重新修改。
	public void SetStoreState ()
	{
		for (int i = 0; i < Cubes.Length; i++) {
			Cubes [i].transform.position = positionlist [i];
			Cubes [i].transform.eulerAngles = rotationlist [i];
		}
		// 因为之前都是改变块的位置来打乱魔方的，所以这个面的颜色是不变的，以后要是开始改变魔方的面的颜色的时候就需要下面的方法了
		for (int j = 0; j < cubefaces.Length; j++) {
			cubefaces [j].SetMapColor (colorlist [j]);
		}
	}
	// 块的位置都是初始时候的，面的颜色是变化的
	public void SetStoreState (List<int> facemap)
	{
		ToOriginalPos ();
		for (int j = 0; j < cubefaces.Length; j++) {
			if (j < facemap.Count) {
				cubefaces [j].SetMapColor ((MagicColor)facemap [j]);
			}
		}
	}

	#endregion

	#region SetColor

	// set original color
	IEnumerator SetFullColor (float t)
	{
		for (int i = 0; i < cubefaces.Length; i++) {
//			yield return new WaitForSeconds (t);
			cubefaces [i].UpdateFaceStyle ();
			cubefaces [i].SetDefaultColor ();
		}
		yield return null;
	}

	public void SetFullColor ()
	{
		for (int i = 0; i < cubefaces.Length; i++) {
			cubefaces [i].UpdateFaceStyle ();
			cubefaces [i].SetDefaultColor ();
		}
	}

	public void SetEditColor ()
	{
		EditColorStore.Clear ();
		EditColorStore.Add (MagicColor.White, 0);
		EditColorStore.Add (MagicColor.Red, 0);
		EditColorStore.Add (MagicColor.Yellow, 0);
		EditColorStore.Add (MagicColor.Green, 0);
		EditColorStore.Add (MagicColor.Blue, 0);
		EditColorStore.Add (MagicColor.Orange, 0);
		EditColorStore.Add (MagicColor.None, 0);

		curColor = MagicColor.None;
		for (int i = 0; i < cubefaces.Length; i++) {
			if (cubefaces [i].cube.cubestyle.Equals (CubeStyle.Face)) {
				cubefaces [i].UpdateFaceStyle ();
				cubefaces [i].SetDefaultColor ();
			} else {
				cubefaces [i].SetEditColor (MagicColor.None);
			}
		}
	}

	// 拾取颜色
	public void PickColor (MagicColor col)
	{
		curColor = col;
		print (curColor);
	}

	// 因为增加的颜色必然是当前的颜色，所以不需要参数了
	public void ColorAdd ()
	{
		//      print ("新添颜色：" + curColor);
		if (curColor != MagicColor.None)
			EditColorStore [curColor] += 1;
	}

	public void ColorDelete (MagicColor color)
	{
		if (color != MagicColor.None) {
			EditColorStore [color] -= 1;
		}
	}

	public void ColorMapAdd (SingleCube cube, CubeMark mark)
	{
		EditColorMap.Add (cube, mark);
		if (CheakEditColorFinished ()) {
			print ("颜色编辑完成！！！！！！！！");
		}

	}

	public void ColorMapDelete (SingleCube cube)
	{
		EditColorMap.Remove (cube);
	}

	bool CheakEditColorFinished ()
	{
		bool finished = true;
		foreach (MagicColor c in EditColorStore.Keys) {
			if (c != MagicColor.None) {
				if (EditColorStore [c] != 8) {
					finished = false;
				}
				break;
			}
		}
		if (finished) {
			// 1. 清空字典
			if (EditColorMap.Count < 20) {
				finished = false;
			}
			// 2. 清除数据
			foreach (SingleCube s in EditColorMap.Keys) {
				if (EditColorMap [s] == CubeMark.None) {
					finished = false;
					break;
				}
			}
		}
		return finished;
	}

	#endregion

	#region UI Operate Function

	//	private bool recordbrokenstate;
	// 自动打乱魔方，并记录打乱后的状态，清除用户操作记录
	public void AutoBroken ()
	{
		ForceRecover ();

		CreatBrokeFormula ();
		singleanitime = 0f;
		spacetime = 0f;
		print (brokenformular);
		StartCoroutine (DoBrokenAndRecord ());

		CleanUserOperate ();
	}

	IEnumerator DoBrokenAndRecord ()
	{
		yield return StartCoroutine (DoFormula (brokenformular));
		RecordMagicCubeState ();
	}

	// 以上次打乱的状态再来一次
	public void OnceAgain ()
	{
		StopAllCoroutines ();
		rotating = false;
		SetStoreState ();
	}
	// 强制还原
	public void ForceRecover ()
	{
		StopAllCoroutines ();
		rotating = false;
		ToOriginalPos ();
		ToOriginalColor ();
	}
	// 自动还原
	public void AutoRecover ()
	{
		OnceAgain ();
		singleanitime = 0.4f;
		spacetime = 0.2f;
		DoMyFormula (recoverformular);
	}
	// 运行记录的玩家的公式
	public void PlayUserRecord ()
	{
		OnceAgain ();
		singleanitime = 0.4f;
		spacetime = 0.2f;
		DoMyFormula (userrecord);
	}

	// 运行记录的玩家的公式
	public void PlayCombinedRecord ()
	{
		OnceAgain ();
		singleanitime = 0.4f;
		spacetime = 0.2f;
		DoMyFormula (combinedrecord);
	}

	// 运行玩家输入的公式
	public void PlayUserEditFormula ()
	{
		singleanitime = 0.4f;
		spacetime = 0.2f;
		DoMyFormula (formula);
	}

	#endregion


	// for test
	void OnGUI ()
	{
		if (Global.State.Equals (Global.GameState.Operate)) {
			if (GUI.Button (new Rect (150f, 0f, 100f, 40f), "自动打乱")) {
				AutoBroken ();		
			}
			if (GUI.Button (new Rect (300f, 0f, 100f, 40f), "查看用户操作记录")) {
				PlayUserRecord ();
			}
			if (GUI.Button (new Rect (150f, 50f, 100f, 40f), "再来一次")) {
				OnceAgain ();
			}
			if (GUI.Button (new Rect (300f, 50f, 100f, 40f), "清除用户操作记录")) {
				CleanUserOperate ();
			}
			if (GUI.Button (new Rect (150f, 100f, 100f, 40f), "强制还原")) {
				ForceRecover ();			
			}
			if (GUI.Button (new Rect (300f, 100f, 100f, 40f), "合并用户操作")) {
				CombineTheUserOperate ();			
			}
			if (GUI.Button (new Rect (150f, 150f, 100f, 40f), "自动还原")) {
				AutoRecover ();			
			}
			if (GUI.Button (new Rect (300f, 150f, 100f, 40f), "合并的操作记录")) {
				PlayCombinedRecord ();			
			}
			if (!string.IsNullOrEmpty (doingformula)) {
				GUI.Label (new Rect (0f, 200f, Screen.width, 40f), "正在执行的公式为" + doingformula);		
			}
			if (!string.IsNullOrEmpty (userrecord)) {		
				GUI.Label (new Rect (0f, 250f, Screen.width, 40f), "记录的玩家操作的公式：" + userrecord, GUIStyle.none);
			}
			if (!string.IsNullOrEmpty (combinedrecord)) {
				GUI.Label (new Rect (0f, 300f, Screen.width, 40f), "合并后玩家操作：" + combinedrecord, GUIStyle.none);
			}
		} else if (Global.State.Equals (Global.GameState.Formular)) {
			if (GUI.Button (new Rect (150f, 0f, 100f, 40f), "自动打乱")) {
				AutoBroken ();		
			}
			if (GUI.Button (new Rect (150f, 50f, 100f, 40f), "再来一次")) {
				OnceAgain ();			
			}
			if (GUI.Button (new Rect (150f, 150f, 100f, 40f), "自动还原")) {
				AutoRecover ();			
			}
			if (GUI.Button (new Rect (150f, 100f, 100f, 40f), "强制还原")) {
				ForceRecover ();			
			}
			if (GUI.Button (new Rect (300f, 50f, 100f, 40f), "清除录入的公式")) {
				formula = "";
			}
			if (GUI.Button (new Rect (300f, 0f, 100f, 40f), "运行用户操作记录")) {
				PlayUserEditFormula ();
			}
			if (!string.IsNullOrEmpty (formula)) {
				GUI.Label (new Rect (0f, 200f, Screen.width, 40f), "正在编辑的公式为" + formula);	
			}
			if (!string.IsNullOrEmpty (doingformula)) {
				GUI.Label (new Rect (0f, 250f, Screen.width, 40f), "正在执行的公式为" + doingformula);		
			}
		} else if (Global.State.Equals (Global.GameState.Learn)) {
			// 创建公式库，其内包括公式库场景，可以应用的公式，适用于什么情况，公式的名称
			if (GUI.Button (new Rect (150f, 0f, 100f, 40f), "输出页面状态")) {
				string lib = "";
				for (int i = 0; i < cubefaces.Length; i++) {
					lib += "," + (int)cubefaces [i].mycolor;
				}	
				print (lib);
			}
			if (GUI.Button (new Rect (300f, 0f, 100f, 40f), "查看公式操作")) {
				singleanitime = 0.4f;
				spacetime = 0.2f;
				DoMyFormula (userrecord);
			}
		}

		Vector2 v2 = Vector2.zero;
		// 二指或以上操作，并且没有判定为转动和运行公式
		if (Input.touchCount > 1 && !rotatejudged && !flipjudged && !flipping && !rotating) {
			v2 = Input.mousePosition;
			StartCoroutine (FlipMagicBox (v2));
		}
	}


	void Update ()
	{
		if (formularing || rotating) {
			return;
		}
		if (Global.State == Global.GameState.Operate || Global.State == Global.GameState.Color || Global.State == Global.GameState.Exercise) {
			// 按键操作
			if (Input.GetKeyDown (KeyCode.L)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					DoLP2 ();
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					DoLN ();
				} else {
					DoLP ();
				}
			}

			if (Input.GetKeyDown (KeyCode.R)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					DoRP2 ();
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					DoRN ();
				} else {
					DoRP ();
				}
			}

			if (Input.GetKeyDown (KeyCode.U)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					DoUP2 ();
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					DoUN ();
				} else {
					DoUP ();
				}
			}

			if (Input.GetKeyDown (KeyCode.D)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					DoDP2 ();
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					DoDN ();
				} else {
					DoDP ();
				}
			}

			if (Input.GetKeyDown (KeyCode.F)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					DoFP2 ();
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					DoFN ();
				} else {
					DoFP ();
				}
			}

			if (Input.GetKeyDown (KeyCode.B)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					DoBP2 ();
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					DoBN ();
				} else {
					DoBP ();
				}
			}

			if (Input.GetKeyDown (KeyCode.X)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					DoXP2 ();
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					DoXN ();
				} else {
					DoXP ();
				}
			}

			if (Input.GetKeyDown (KeyCode.Y)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					DoYP2 ();
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					DoYN ();
				} else {
					DoYP ();
				}
			}

			if (Input.GetKeyDown (KeyCode.Z)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					DoZP2 ();
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					DoZN ();
				} else {
					DoZP ();
				}
			}
		} else if (Global.State == Global.GameState.Formular) {

			// 按键操作
			if (Input.GetKeyDown (KeyCode.L)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					FormularAdd (OperateStep.L2);
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					FormularAdd (OperateStep.L0);
				} else {
					FormularAdd (OperateStep.L);
				}
			}

			if (Input.GetKeyDown (KeyCode.R)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					FormularAdd (OperateStep.R2);
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					FormularAdd (OperateStep.R0);
				} else {
					FormularAdd (OperateStep.R);
				}
			}

			if (Input.GetKeyDown (KeyCode.U)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					FormularAdd (OperateStep.U2);
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					FormularAdd (OperateStep.U0);
				} else {
					FormularAdd (OperateStep.U);
				}
			}

			if (Input.GetKeyDown (KeyCode.D)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					FormularAdd (OperateStep.D2);
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					FormularAdd (OperateStep.D0);
				} else {
					FormularAdd (OperateStep.D);
				}
			}

			if (Input.GetKeyDown (KeyCode.F)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					FormularAdd (OperateStep.F2);
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					FormularAdd (OperateStep.F0);
				} else {
					FormularAdd (OperateStep.F);
				}
			}

			if (Input.GetKeyDown (KeyCode.B)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					FormularAdd (OperateStep.B2);
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					FormularAdd (OperateStep.B0);
				} else {
					FormularAdd (OperateStep.B);
				}
			}

			if (Input.GetKeyDown (KeyCode.X)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					FormularAdd (OperateStep.X2);
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					FormularAdd (OperateStep.X0);
				} else {
					FormularAdd (OperateStep.X);
				}
			}

			if (Input.GetKeyDown (KeyCode.Y)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					FormularAdd (OperateStep.Y2);
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					FormularAdd (OperateStep.Y0);
				} else {
					FormularAdd (OperateStep.Y);
				}
			}

			if (Input.GetKeyDown (KeyCode.Z)) {
				if (Input.GetKey (KeyCode.LeftAlt)) {
					FormularAdd (OperateStep.Z2);
				} else if (Input.GetKey (KeyCode.LeftShift)) {
					FormularAdd (OperateStep.Z0);
				} else {
					FormularAdd (OperateStep.Z);
				}
			}
		}
	}

	#region Control

	//	public void SwitchState (State state)
	//	{
	//		this.mystate = state;
	//		switch (mystate) {
	//		case State.Operate:
	//			StartCoroutine (SetFullColor ());
	//			break;
	//		case State.EditColor:
	//			EditColorMap.Clear ();
	//			SetEditColor ();
	//			foreach (SingleCube sc in singlecubes) {
	//				sc.CleanCubeColor ();
	//			}
	//			break;
	//		case State.EditFormula:
	//			StartCoroutine (SetFullColor ());
	//			break;
	//		case State.OperateAndFormula:
	//			break;
	//		}
	//	}

	#endregion

	#region AutoOperate or PressButtonAndAnimation 公式编辑或者单步操作

	public void DoSingleStep (OperateStep step)
	{
		if (rotating) {
			return;
		}
		switch (step) {
		case OperateStep.L:
			DoLP ();
			break;
		case OperateStep.L0:
			DoLN ();
			break;
		case OperateStep.L2:
			DoLP2 ();
			break;
		case OperateStep.R:
			DoRP ();
			break;
		case OperateStep.R0:
			DoRN ();
			break;
		case OperateStep.R2:
			DoRP2 ();
			break;
		case OperateStep.U:
			DoUP ();
			break;
		case OperateStep.U0:
			DoUN ();
			break;
		case OperateStep.U2:
			DoUP2 ();
			break;
		case OperateStep.D:
			DoDP ();
			break;
		case OperateStep.D0:
			DoDN ();
			break;
		case OperateStep.D2:
			DoDP2 ();
			break;
		case OperateStep.F:
			DoFP ();
			break;
		case OperateStep.F0:
			DoFN ();
			break;
		case OperateStep.F2:
			DoFP2 ();
			break;
		case OperateStep.B:
			DoBP ();
			break;
		case OperateStep.B0:
			DoBN ();
			break;
		case OperateStep.B2:
			DoBP2 ();
			break;
		case OperateStep.X:
			DoXP ();
			break;
		case OperateStep.X0:
			DoXN ();
			break;
		case OperateStep.X2:
			DoXP2 ();
			break;
		case OperateStep.Y:
			DoYP ();
			break;
		case OperateStep.Y0:
			DoYN ();
			break;
		case OperateStep.Y2:
			DoYP2 ();
			break;
		case OperateStep.Z:
			DoZP ();
			break;
		case OperateStep.Z0:
			DoZN ();
			break;
		case OperateStep.Z2:
			DoZP2 ();
			break;
		}
//		print ("~~~~~~~~" + step);
		// 每走一步进行一次判断。
		CheckAcomplished ();
	}

	#region SlngleStep 标准单步操作

	// 使只有公式操作的时候才会发生编辑公式功能，其他时候就是单纯的记录公式

	#region L L2 L'

	//L
	public void DoLP ()
	{
		RecordUserOperate ("L");
		GetOperateSuit (OperateSuit.Left);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.forward, 90f, singleanitime));
	}

	//L2
	public void DoLP2 ()
	{
		RecordUserOperate ("L2");
		GetOperateSuit (OperateSuit.Left);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.forward, 180f, singleanitime * 2f));
	}

	//L‘
	public void DoLN ()
	{
		RecordUserOperate ("L'");
		GetOperateSuit (OperateSuit.Left);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.forward, -90f, singleanitime));
	}

	#endregion

	#region R R2 R'

	//R
	public void DoRP ()
	{
		RecordUserOperate ("R");
		GetOperateSuit (OperateSuit.Right);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.back, 90f, singleanitime));
	}

	//R2
	public void DoRP2 ()
	{
		RecordUserOperate ("R2");
		GetOperateSuit (OperateSuit.Right);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.back, 180f, singleanitime * 2f));
	}

	//R'
	public void DoRN ()
	{
		RecordUserOperate ("R'");
		GetOperateSuit (OperateSuit.Right);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.back, -90f, singleanitime));
	}

	#endregion

	#region U U2 U'

	//U
	public void DoUP ()
	{
		RecordUserOperate ("U");
		GetOperateSuit (OperateSuit.Up);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.up, 90f, singleanitime));
	}

	//U
	public void DoUP2 ()
	{
		RecordUserOperate ("U2");
		GetOperateSuit (OperateSuit.Up);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.up, 180f, singleanitime * 2f));
	}

	//U'
	public void DoUN ()
	{
		RecordUserOperate ("U'");
		GetOperateSuit (OperateSuit.Up);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.up, -90f, singleanitime));
	}

	#endregion


	#region D D2 D'

	//D
	public void DoDP ()
	{
		RecordUserOperate ("D");
		GetOperateSuit (OperateSuit.Down);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.down, 90f, singleanitime));
	}

	//D2
	public void DoDP2 ()
	{
		RecordUserOperate ("D2");
		GetOperateSuit (OperateSuit.Down);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.down, 180f, singleanitime * 2f));
	}

	//D'
	public void DoDN ()
	{
		RecordUserOperate ("D'");
		GetOperateSuit (OperateSuit.Down);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.down, -90f, singleanitime));
	}

	#endregion

	#region F F2 F'


	//F
	public void DoFP ()
	{
		RecordUserOperate ("F");
		GetOperateSuit (OperateSuit.Front);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.left, 90f, singleanitime));
	}

	//F
	public void DoFP2 ()
	{
		RecordUserOperate ("F2");
		GetOperateSuit (OperateSuit.Front);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.left, 180f, singleanitime * 2f));
	}

	//F'
	public void DoFN ()
	{
		RecordUserOperate ("F'");
		GetOperateSuit (OperateSuit.Front);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.left, -90f, singleanitime));
	}

	#endregion

	#region B B2 B'


	//B
	public void DoBP ()
	{
		RecordUserOperate ("B");
		GetOperateSuit (OperateSuit.Back);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.right, 90f, singleanitime));
	}

	//B
	public void DoBP2 ()
	{
		RecordUserOperate ("B2");
		GetOperateSuit (OperateSuit.Back);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.right, 180f, singleanitime * 2f));
	}

	//B'
	public void DoBN ()
	{
		RecordUserOperate ("B'");
		GetOperateSuit (OperateSuit.Back);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.right, -90f, singleanitime));
	}

	#endregion

	#endregion

	#region EntiretyStep 整体操作（旋转魔方）

	#region X X2 X'

	//X
	public void DoXP ()
	{
		RecordUserOperate ("X");
		GetOperateSuit (OperateSuit.Entriety);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.back, 90f, singleanitime));
	}

	//X2
	public void DoXP2 ()
	{
		RecordUserOperate ("X2");
		GetOperateSuit (OperateSuit.Entriety);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.back, 180f, singleanitime * 2f));
	}

	//X'
	public void DoXN ()
	{
		RecordUserOperate ("X'");
		GetOperateSuit (OperateSuit.Entriety);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.back, -90f, singleanitime));
	}

	#endregion

	#region Y Y2 Y'


	//Y
	void DoYP ()
	{
		RecordUserOperate ("Y");
		GetOperateSuit (OperateSuit.Entriety);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.up, 90f, singleanitime));
	}

	//Y2
	void DoYP2 ()
	{
		RecordUserOperate ("Y2");
		GetOperateSuit (OperateSuit.Entriety);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.up, 180f, singleanitime * 2f));
	}

	//Y'
	void DoYN ()
	{
		RecordUserOperate ("Y'");
		GetOperateSuit (OperateSuit.Entriety);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.up, -90f, singleanitime));
	}

	#endregion

	#region Z Z2 Z'


	//Z
	void DoZP ()
	{
		RecordUserOperate ("Z");
		GetOperateSuit (OperateSuit.Entriety);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.left, 90f, singleanitime));
	}

	//Z2
	void DoZP2 ()
	{
		RecordUserOperate ("Z2");
		GetOperateSuit (OperateSuit.Entriety);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.left, 180f, singleanitime * 2f));
	}

	//Z'
	void DoZN ()
	{
		RecordUserOperate ("Z'");
		GetOperateSuit (OperateSuit.Entriety);
		StartCoroutine (RotateAnimation (operatelist, Vector3.zero, Vector3.left, -90f, singleanitime));
	}

	#endregion

	#endregion

	#endregion

	#region Manual Operate 单指操作，滑动魔方一层

	public void ManualOperate (OperatePiece piece, Vector3 pos, GameObject singleboxobj)
	{
		//		curobj = singleboxobj;
		if (formularing || rotating || rotatejudged || flipjudged || flipping) {
			return;
		}
		StartCoroutine (DoJudge (piece, pos));
	}

	// Judge the actual operate step
	IEnumerator DoJudge (OperatePiece piece, Vector3 pos)
	{
		// 判断是否操作魔方
		Vector2 startpos = Vector2.zero;
		Vector2 curpos = Vector2.zero;

		if (Input.touchCount > 0) {
			startpos = Input.GetTouch (0).position;
			curpos = Input.GetTouch (0).position;
			while (!rotatejudged && Input.touchCount.Equals (1)) {
				curpos = Input.GetTouch (0).position;
				if (Vector2.Distance (curpos, startpos) >= 10f) {
					rotatejudged = true;
				}
				yield return null;
			}
		}

		if (rotatejudged) {
			rotating = true;
			
			// 记录转动的角度
			float rollangle = 0f;
			Vector2 lastmousepos = Vector2.zero;
			if (Input.touchCount > 0) {
				lastmousepos = Input.GetTouch (0).position;
			}
			Vector2 offset = Vector2.zero;
			float stepangle = 0f;

			Vector3 direction = Vector3.up;
			int suit = 0;

			string CS = "";

			switch (piece) {
			case OperatePiece.Left:
				if (Mathf.Abs (curpos.y - startpos.y) >= Mathf.Abs (curpos.x - startpos.x) * 1.4f) {
					//Slide up and down (alike x axis)
					if (pos.z < -0.5f) {
						GetOperateSuit (OperateSuit.Right);
						CS += "R";
					} else if (pos.z < 0.5f) {
						GetOperateSuit (OperateSuit.MiddleZ);
						CS += "MZ";
					} else {
						GetOperateSuit (OperateSuit.Left);
						CS += "L";
					}
					direction = Vector3.back;
					suit = 1;
				} else {
					//Slide left and right (alike y axis)
					if (pos.y < -0.5f) {
						GetOperateSuit (OperateSuit.Down);
						CS += "D";
					} else if (pos.y < 0.5f) {
						GetOperateSuit (OperateSuit.MiddleY);
						CS += "MY";
					} else {
						GetOperateSuit (OperateSuit.Up);
						CS += "U";
					}
					direction = Vector3.down;
					suit = 2;
				}
				break;
			case OperatePiece.Right:
				if (Mathf.Abs (curpos.y - startpos.y) >= Mathf.Abs (curpos.x - startpos.x) * 1.4f) {
					//Slide up and down (alike z axis)
					if (pos.x < -0.5f) {
						GetOperateSuit (OperateSuit.Front);
						CS += "F";
					} else if (pos.x < 0.5f) {
						GetOperateSuit (OperateSuit.MiddleX);
						CS += "MX";
					} else {
						GetOperateSuit (OperateSuit.Back);
						CS += "B";
					}
					direction = Vector3.right;
					suit = 1;
				} else {
					//Slide left and right (alike y axis)
					if (pos.y < -0.5f) {
						GetOperateSuit (OperateSuit.Down);
						CS += "D";
					} else if (pos.y < 0.5f) {
						GetOperateSuit (OperateSuit.MiddleY);
						CS += "MY";
					} else {
						GetOperateSuit (OperateSuit.Up);
						CS += "U";
					}
					direction = Vector3.down;
					suit = 2;
				}
				break;
			case OperatePiece.Top:
				if ((curpos.y - startpos.y) * (curpos.x - startpos.x) >= 0f) {
					//Slide alike x axis
					if (pos.z < -0.5f) {
						GetOperateSuit (OperateSuit.Right);
						CS += "R";
					} else if (pos.z < 0.5f) {
						GetOperateSuit (OperateSuit.MiddleZ);
						CS += "MZ";
					} else {
						GetOperateSuit (OperateSuit.Left);
						CS += "L";
					}
					direction = Vector3.back;
					suit = 2;
				} else {
					//Slide alike z axis
					if (pos.x < -0.5f) {
						GetOperateSuit (OperateSuit.Front);
						CS += "F";
					} else if (pos.x < 0.5f) {
						GetOperateSuit (OperateSuit.MiddleX);
						CS += "MX";
					} else {
						GetOperateSuit (OperateSuit.Back);
						CS += "B";
					}
					direction = Vector3.left;
					suit = 3;
				}
				break;
			}

			while (Input.touchCount > 0 && Input.GetTouch (0).phase != TouchPhase.Ended) {
				offset = new Vector2 (Input.GetTouch (0).position.x - lastmousepos.x, Input.GetTouch (0).position.y - lastmousepos.y);
				// 根据位置和滑动的方向来对魔方对应的层进行操作
				if (suit.Equals (1)) {
					stepangle = offset.y * rlength;
				} else if (suit.Equals (2)) {
					stepangle = (offset.x * 0.86f + offset.y * 0.25f) * rlength;
				} else if (suit.Equals (3)) {
					stepangle = (offset.x * 0.86f - offset.y * 0.25f) * rlength;
				}
			
				rollangle += stepangle;
				foreach (SingleCube b in operatelist) {
					b.transform.RotateAround (Vector3.zero, direction, stepangle);
				}
				lastmousepos = Input.GetTouch (0).position;
				yield return null;
			}

			float targetangle = 0f;
			int manstep = 0;
			if (rollangle > 0) {
				manstep = (int)((rollangle + 45f) / 90f);
			} else if (rollangle < 0) {
				manstep = (int)((rollangle - 45f) / 90f);
			}
			targetangle = manstep * 90f;

			manstep %= 4;
			manstep = manstep < 0 ? manstep + 4 : manstep;
			if (!manstep.Equals (0)) {
				// 由于朝向问题，所以对立面的公式是不同的。
				if (CS.Equals ("U") || CS.Equals ("L") || CS.Equals ("F")) {
					if (manstep == 2) {
						CS += "2";
					} else if (manstep == 1) {
						CS += "'";
					}
				} else if (CS.Equals ("D") || CS.Equals ("R") || CS.Equals ("B")) {
					if (manstep == 2) {
						CS += "2";
					} else if (manstep == 3) {
						CS += "'";
					}
				}
				RecordUserOperate (CS);
			}

			float offsetangle = targetangle - rollangle;

			float speed = 90f / singleanitime;
			if (offsetangle < 0) {
				speed *= -1f;
			}

			bool finished = false;

			float curoffsetangle = 0f;
			float lastoffsetangle = 0f;
			float truthoffsetangle = 0f;

			while (!finished) {
				float o = speed * Time.deltaTime;
				curoffsetangle += o;
				if (Mathf.Abs (curoffsetangle) > Mathf.Abs (offsetangle)) {
					curoffsetangle = offsetangle;
					finished = true;
				}
				truthoffsetangle = curoffsetangle - lastoffsetangle;
				foreach (SingleCube b in operatelist) {
					b.transform.RotateAround (Vector3.zero, direction, truthoffsetangle);
				}
				lastoffsetangle = curoffsetangle;
				yield return null;
			}


			foreach (SingleCube b in operatelist) {
				b.AdjustPos ();
			}

			foreach (CubeFace f in cubefaces) {
				f.UpdateFaceStyle ();
			}

			CheckAcomplished ();

			rotating = false;
		}
		rotatejudged = false;
	}

	#endregion

	#region 整体旋转魔方 （多指操作）

	IEnumerator FlipMagicBox (Vector2 originalpos)
	{
		int dir = 0;
		string CS = "";
		Vector3 direction = Vector3.zero;
		Vector2 multiplepos = Input.mousePosition;
		while (Input.touchCount > 1 && !flipjudged) {
			multiplepos = Input.mousePosition;
			if (Vector2.Distance (multiplepos, originalpos) >= 10f) {
				flipjudged = true;
				if (Mathf.Abs (multiplepos.x - originalpos.x) >= Mathf.Abs (multiplepos.y - originalpos.y)) {
					dir = 1;
					CS += "Y";
					direction = Vector3.down;
				} else {
					dir = 2;
					if (multiplepos.x >= Screen.width / 2) {
						CS += "Z";
						direction = Vector3.right;
					} else {
						CS += "X";
						direction = Vector3.back;
					}
				}
			}
			yield return null;
		}
		if (flipjudged) {
			flipping = true;
			GetOperateSuit (OperateSuit.Entriety);

			// 记录转动的角度
			float rollangle = 0f;

			Vector2 lastpos = multiplepos;

			float offset = 0f;
			while (Input.touchCount > 1) {
				multiplepos = Input.mousePosition;
				if (dir.Equals (1)) {
					// 左右转动Y轴
					offset = multiplepos.x - lastpos.x;
				} else if (dir.Equals (2)) {
					// 上下转动XZ轴
					offset = multiplepos.y - lastpos.y;
				}
				offset *= 2 * rlength;
				foreach (SingleCube b in operatelist) {
					b.transform.RotateAround (Vector3.zero, direction, offset);
				}
				rollangle += offset;
				//print ("flipping" + lastpos + multiplepos);
				lastpos = multiplepos;
				yield return null;
			}

			float targetangle = 0f;
			int manstep = 0;
			if (rollangle > 0) {
				manstep = (int)((rollangle + 45f) / 90f);
			} else if (rollangle < 0) {
				manstep = (int)((rollangle - 45f) / 90f);
			}
			targetangle = manstep * 90f;

			manstep %= 4;
			manstep = manstep < 0 ? manstep + 4 : manstep;
			if (!manstep.Equals (0)) {
				// 由于朝向问题，所以对立面的公式是不同的。
				if (CS.Equals ("Y") || CS.Equals ("Z")) {
					if (manstep == 2) {
						CS += "2";
					} else if (manstep == 1) {
						CS += "'";
					}
				} else if (CS.Equals ("X")) {
					if (manstep == 2) {
						CS += "2";
					} else if (manstep == 3) {
						CS += "'";
					}
				}
				RecordUserOperate (CS);
			}

			float offsetangle = targetangle - rollangle;

			float speed = 90f / singleanitime;
			if (offsetangle < 0) {
				speed *= -1f;
			}

			bool finished = false;

			float curoffsetangle = 0f;
			float lastoffsetangle = 0f;
			float truthoffsetangle = 0f;

			while (!finished) {
				float o = speed * Time.deltaTime;
				curoffsetangle += o;
				if (Mathf.Abs (curoffsetangle) > Mathf.Abs (offsetangle)) {
					curoffsetangle = offsetangle;
					finished = true;
				}
				truthoffsetangle = curoffsetangle - lastoffsetangle;
				foreach (SingleCube b in operatelist) {
					b.transform.RotateAround (Vector3.zero, direction, truthoffsetangle);
				}
				lastoffsetangle = curoffsetangle;
				yield return null;
			}

//			foreach (SingleCube b in operatelist) {
//				b.AdjustPos ();
//			}

			foreach (CubeFace f in cubefaces) {
				f.UpdateFaceStyle ();
			}

			flipping = false;
		}
		flipjudged = false;
	}

	#endregion


	#region DoRotate Animate 选择操作的魔方块组，然后进行旋转操作

	// 这里是旋转魔方的动画，这个是操作指定的块的方法，所以应该可以适用于其他阶魔方
	IEnumerator RotateAnimation (List<SingleCube> olist, Vector3 point, Vector3 axis, float angle, float length)
	{
		rotating = true;
		float t = 0f;
		float o = 0f;
		float lt = 0f;
		if (length > 0f) {
			float s = angle / length;
			while (t < length) {
				t += Time.deltaTime;
				if (t > length) {
					t = length;
				}
				o = (t - lt) * s;
				foreach (SingleCube b in olist) {
					b.transform.RotateAround (point, axis, o);
				}
				lt = t;
				yield return null;
			}
		} else {
			foreach (SingleCube b in olist) {
				b.transform.RotateAround (point, axis, angle);
			}
			yield return null;
		}
//		// 或许可以不需要
//		foreach (SingleCube b in olist) {
//			b.AdjustPos ();
//		}

		foreach (CubeFace f in cubefaces) {
			f.UpdateFaceStyle ();
		}

		CheckAcomplished ();
		rotating = false;
	}

	// Find operate suit. 找到当前操作的块儿组，这个需要修改来适应多阶魔方，
	// 为魔方添加各个位置的参数的话，可能比较方便的统计每个操作组里面都有哪些，需要以后修改尝试
	void GetOperateSuit (OperateSuit str)
	{
		switch (str) {
		case OperateSuit.Left:
			operatelist.Clear ();
			foreach (SingleCube b in Cubes) {
				if (b.transform.position.z > 0.5f) {
					operatelist.Add (b);
				}
			}
			break;
		case OperateSuit.Right:
			operatelist.Clear ();
			foreach (SingleCube b in Cubes) {
				if (b.transform.position.z < -0.5f) {
					operatelist.Add (b);
				}
			}
			break;
		case OperateSuit.Up:
			operatelist.Clear ();
			foreach (SingleCube b in Cubes) {
				if (b.transform.position.y > 0.5f) {
					operatelist.Add (b);
				}
			}
			break;
		case OperateSuit.Down:
			operatelist.Clear ();
			foreach (SingleCube b in Cubes) {
				if (b.transform.position.y < -0.5f) {
					operatelist.Add (b);
				}
			}
			break;
		case OperateSuit.Front:
			operatelist.Clear ();
			foreach (SingleCube b in Cubes) {
				if (b.transform.position.x < -0.5f) {
					operatelist.Add (b);
				}
			}
			break;
		case OperateSuit.Back:
			operatelist.Clear ();
			foreach (SingleCube b in Cubes) {
				if (b.transform.position.x > 0.5f) {
					operatelist.Add (b);
				}
			}
			break;
		case OperateSuit.Entriety:
			operatelist.Clear ();
			foreach (SingleCube b in Cubes) {
				operatelist.Add (b);
			}
			break;
		case OperateSuit.MiddleX:
			operatelist.Clear ();
			foreach (SingleCube b in Cubes) {
				if (b.transform.position.x >= -0.5f && b.transform.position.x <= 0.5f) {
					operatelist.Add (b);
				}
			}
			break;
		case OperateSuit.MiddleY:
			operatelist.Clear ();
			foreach (SingleCube b in Cubes) {
				if (b.transform.position.y >= -0.5f && b.transform.position.y <= 0.5f) {
					operatelist.Add (b);
				}
			}
			break;
		case OperateSuit.MiddleZ:
			operatelist.Clear ();
			foreach (SingleCube b in Cubes) {
				if (b.transform.position.z >= -0.5f && b.transform.position.z <= 0.5f) {
					operatelist.Add (b);
				}
			}
			break;
		}
	}

	#endregion

	#region Edit Formular

	public string formula = "";

	//the formula is doing; 正在执行的公式
	public string doingformula = "";
	// 两步之间的间隔时间
	public float spacetime = 1f;

	OperateStep GetString2Step (string val)
	{
		OperateStep step = OperateStep.L;// 初始化L没任何意义
		foreach (OperateStep s in MyMapPrefab.Step2StringMap.Keys) {
			if (MyMapPrefab.Step2StringMap [s].Equals (val)) {
				step = s;
				break;
			}
		}
		return step;
	}

	//  这个做的是针对操作过程中的记录一步一步加上去的。用起来有些不方便
	public void FormularAdd (OperateStep onestep)
	{
		if (MyMapPrefab.Step2StringMap.ContainsKey (onestep)) {
			if (string.IsNullOrEmpty (formula)) {
				formula += MyMapPrefab.Step2StringMap [onestep];
			} else {

				char[] charSeparator = { ',' };
				string[] l = formula.Split (charSeparator, System.StringSplitOptions.RemoveEmptyEntries);

				// 获得上一步存储的步骤序号
				int ps = (int)GetString2Step (l [l.Length - 1]);
				int cs = (int)onestep;

				// TODO: 检查新步骤是否与上一步骤有冲突，进行公式合并。
				if ((cs / 3) == (ps / 3)) {
					FormularDelete ();
					if ((cs % 3) == (ps % 3)) {
						if (cs % 3 == 0) { // 相加等于2
							FormularAdd ((OperateStep)(cs + 1));
						} else if (cs % 3 == 2) {
							FormularAdd ((OperateStep)(cs - 1));
						}
					} else {
						if (cs % 3 == 1) {
							FormularAdd ((OperateStep)(cs * 2 - ps));
						} else if (ps % 3 == 1) {
							FormularAdd ((OperateStep)(ps * 2 - cs));
						}
					}
				} else {
					formula += "," + MyMapPrefab.Step2StringMap [onestep];
				}
			}
		}

	}

	public void FormularDelete ()
	{
		if (!string.IsNullOrEmpty (formula)) {
			if (formula.Contains (",")) {
				int i = formula.LastIndexOf (',');
				formula = formula.Remove (i);
			} else {
				formula = "";
			}
		}
	}

	// 执行公式
	public void DoMyFormula (string fml)
	{
		StartCoroutine (DoFormula (fml));
	}

	IEnumerator DoFormula (string fml)
	{
		if (!string.IsNullOrEmpty (fml)) {
			print ("zhixing gongshi :" + fml);
			formularing = true;
			doingformula = fml;
			List<string> mysteps = new List<string> ();

			string context = fml;
			char[] charSeparator = { ',' };
			string[] l = context.Split (charSeparator, System.StringSplitOptions.RemoveEmptyEntries);
			mysteps.AddRange (l);

			while (!string.IsNullOrEmpty (fml) && mysteps.Count > 0) {
				if (!rotating) {
					// 等待1秒，如果是新手，应该多加等待
					yield return new WaitForSeconds (spacetime);
					// 找到map的第一个公式
					OperateStep curstep = GetString2Step (mysteps [0]);
					DoSingleStep (curstep);
				
					// 去掉公式的第一个操作字符串，生成剩下的公式字符串（适用于用户自己编写的公式）（盲拧）
					if (mysteps.Count > 1) {
						fml = fml.Remove (0, mysteps [0].Length + 1);
					} else {
						fml = fml.Remove (0, mysteps [0].Length);
					}
					mysteps.Remove (mysteps [0]);
				}
				doingformula = fml;
				yield return null;
			}
			formularing = false;
		}
		if (PlayerPrefs.HasKey ("SpaceTime")) {
			spacetime = PlayerPrefs.GetFloat ("SpaceTime");
		}
		if (PlayerPrefs.HasKey ("AnimateTime")) {
			singleanitime = PlayerPrefs.GetFloat ("AnimateTime");
		}
//		if (recordbrokenstate) {
//			RecordMagicCubeState ();
//		}
		print ("formula has done");
		yield return null;
	}


	// creat broken formular and the recover formular 适用于自动打乱
	private string brokenformular = "";
	private string recoverformular = "";

	public void CreatBrokeFormula ()
	{
		int lf = Random.Range (6, 21);// length of formula
//		int lf = Random.Range (1, 21);// length of formula
		int si = -1;// 记录当前公式的序号
		string fb = ""; // 生成的打乱公式
		string fr = ""; // 生成的还原公式

		for (int i = 0; i < lf;) {
			int cs = Random.Range (0, 18);
			// 防止出现前后左右上下互不影响的操作
			if (cs / 6 != si / 6) {
//			if ((int)(cs / 3) != (int)(si / 3)) {
				si = cs;

				OperateStep onestep = (OperateStep)cs;
				if (string.IsNullOrEmpty (fb)) {
					fb += MyMapPrefab.Step2StringMap [onestep];
				} else {
					fb += "," + MyMapPrefab.Step2StringMap [onestep];
				}

				int rs = (int)(cs / 3) * 3 + (2 - cs % 3);
				OperateStep restep = (OperateStep)rs;

				if (string.IsNullOrEmpty (fr)) {
					fr = MyMapPrefab.Step2StringMap [restep];
				} else {
					fr = MyMapPrefab.Step2StringMap [restep] + "," + fr;
				}

				i++;
			}
		}
		brokenformular = fb;
		recoverformular = fr;
	}

	// 记录用户的所有操作步骤。
	private string userrecord;
	private string combinedrecord;

	void RecordUserOperate (string ustep)
	{
		if (string.IsNullOrEmpty (userrecord)) {
			userrecord = ustep;
		} else {
			userrecord = userrecord + "," + ustep;
		}
	}

	// 在每次重新开始，或者编辑颜色，切换关卡的时候
	void CleanUserOperate ()
	{
		userrecord = "";
		combinedrecord = "";
	}

	void CombineTheUserOperate ()
	{
		if (string.IsNullOrEmpty (userrecord)) {
			return;
		}

		char[] charSeparator = { ',' };
		string[] l = userrecord.Split (charSeparator, System.StringSplitOptions.RemoveEmptyEntries);
		List<string> temp = new List<string> ();
		for (int i = 0; i < l.Length; i++) {
			temp.Add (l [i]);
		}
		// 找到公式一致的先合并，然后在重新判断，但是第一次就合并了之后怎么办？重新开始，还是继续合并？
		if (temp.Count > 1) {
			bool combined = false;
			while (!combined) {
				combined = true;
				for (int i = temp.Count - 1; i > 0; i--) {
					int cs = (int)GetString2Step (temp [i]);
					int ps = (int)GetString2Step (temp [i - 1]);
					if ((cs / 3) == (ps / 3)) {
//						print (i + "去除" + temp [i] + "和" + temp [i - 1]);
						temp.RemoveAt (i);
						temp.RemoveAt (i - 1);
						if ((cs % 3) == (ps % 3)) {
							if (cs % 3 == 0) { // 相加等于2
								temp.Insert (i - 1, MyMapPrefab.Step2StringMap [(OperateStep)(cs + 1)]);
//								print ("1  将其合并为" + temp [i - 1]);
							} else if (cs % 3 == 2) {
								temp.Insert (i - 1, MyMapPrefab.Step2StringMap [(OperateStep)(cs - 1)]);
//								print ("2  将其合并为" + temp [i - 1]);
							}
						} else {
							if (cs % 3 == 1) {
								temp.Insert (i - 1, MyMapPrefab.Step2StringMap [(OperateStep)(cs * 2 - ps)]);
//								print ("3  将其合并为" + temp [i - 1]);
							} else if (ps % 3 == 1) {
								temp.Insert (i - 1, MyMapPrefab.Step2StringMap [(OperateStep)(ps * 2 - cs)]);
//								print ("4  将其合并为" + temp [i - 1]);
							}
						}
						combined = false;
						break;
					}
				}
			}
		}
		combinedrecord = "";
		for (int i = 0; i < temp.Count; i++) {
			combinedrecord += temp [i] + ",";
		}
		if (!string.IsNullOrEmpty (combinedrecord)) {
			combinedrecord = combinedrecord.Remove (combinedrecord.Length - 1);
		}
	}

	#endregion

	#region 判断魔方是否还原成功

	// 检查是否完成还原
	// 以相同面中颜色是否相同为依据来判断是否完成魔方的还原
	void CheckAcomplished ()
	{
		bool finished = true;
		accomplishstate.Clear ();
		for (int i = 0; i < cubefaces.Length; i++) {
			if (cubefaces [i].mycolor != MagicColor.None) {
				if (accomplishstate.ContainsKey (cubefaces [i].facestyle)) {
					if (accomplishstate [cubefaces [i].facestyle] != cubefaces [i].mycolor) {
						finished = false;
						break;
					}
				} else {
					accomplishstate.Add (cubefaces [i].facestyle, cubefaces [i].mycolor);
				}
			}
		}
		if (finished) {
			print ("finished!!!!!!!!");
			// TODO：完成之后是重新开始还是再来一次等等，如果是任务关卡又该怎么样……

		}
	}

	#endregion

	#region 强制还原

	// 将魔方的所有块都放在初始化的状态，以便后续应用存储的面片数据来初始化魔方状态
	void ToOriginalPos ()
	{
		for (int i = 0; i < Cubes.Length; i++) {
			Cubes [i].transform.position = new 
				Vector3 ((i % 9) / 3 - 1, i / 9 - 1, i % 3 - 1) * CubeSize;
			Cubes [i].transform.eulerAngles = Vector3.zero;
		}
	}

	// 将魔方的所有块都放在初始化的状态，以便后续应用存储的面片数据来初始化魔方状态
	void ToOriginalColor ()
	{
		SetFullColor ();
	}

	#endregion

	// 先生成完成时的状态，然后以程序随机打乱，第一版本，现在感觉不太合适。
	void CreatLevel (int l)
	{
		SetEditColor ();
		switch (l) {
		case 1:
			//level1 第一层的棱块
			// 取最上面一层为操作层，需要显示的块的颜色为最上面一层的十字和对应的邻面
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y > 1.25 * CubeSize &&
				    (Mathf.Abs (cf.transform.position.x) < 0.25 * CubeSize || Mathf.Abs (cf.transform.position.z) < 0.25 * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if ((cf.transform.position.y > 0.75 * CubeSize && cf.transform.position.y < 1.75 * CubeSize) &&
				           (Mathf.Abs (cf.transform.position.x) < 0.25 * CubeSize || Mathf.Abs (cf.transform.position.z) < 0.25 * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			break;
		case 2:
			//level2 第一层
			// 取最上面一层为操作层，需要显示的块的颜色为最上面一层和对应的邻面
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y > 0.5 * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			break;
		case 3:
			//level3 第二层
			// 取最上面两层为操作层，需要显示的块的颜色为最上面两层的所有面
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y > -0.5 * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			break;
		case 4:
			//level4 下两层和顶面的十字
			// 需要显示的块的颜色为下面两层的所有面和顶面的十字
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5 * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 1.25 * CubeSize &&
				           (Mathf.Abs (cf.transform.position.x) < 0.25 * CubeSize || Mathf.Abs (cf.transform.position.z) < 0.25 * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			break;
		case 5:
			//level5 下两层和顶面
			// 需要显示的块的颜色为下面两层的所有面和顶面
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5 * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 1.25 * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			break;
		case 6:
			//level6 下两层和顶面以及四个角块
			// 需要显示的块的颜色为下面两层的所有面和顶面以及四个角块对应的面
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5 * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 1.25 * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if ((cf.transform.position.y > 0.5 * CubeSize && cf.transform.position.y < 1.25 * CubeSize) &&
				           !(Mathf.Abs (cf.transform.position.x) < 0.25 * CubeSize || Mathf.Abs (cf.transform.position.z) < 0.25 * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			break;
		case 7:
			//level7 全部解决
			SetFullColor ();
			break;
		}
		CreatBrokeFormula ();
		formula = brokenformular;
		DoMyFormula (formula);
	}

	void SingleRecoverFomularExercise ()
	{
		
	}



	// 每个公式都有自己的作用场景，需要事先设定好对应的公式和场景
	// 需要添加或修改成对应面颜色的模式，这样就可以实现部分的随机效果和各种记录初始状态然后进行多次尝试的情况，比较适于盲拧
	void FormulaLibrary ()
	{
		ToOriginalPos ();
		SetEditColor ();
		string formulaname = "TTopface7";
		switch (formulaname) {
		#region states
		case "FEdge1":
			// R
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y > 0.75 * CubeSize &&
				    (Mathf.Abs (cf.transform.position.x) < 0.25 * CubeSize || Mathf.Abs (cf.transform.position.z) < 0.25 * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [30].SetEditColor (MagicColor.None);
			cubefaces [43].SetEditColor (MagicColor.None);
			cubefaces [21].SetEditColor (MagicColor.Yellow);
			cubefaces [39].SetEditColor (MagicColor.Red);
			break;
		case "FEdge2":
			// R2
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y > 0.75 * CubeSize &&
				    (Mathf.Abs (cf.transform.position.x) < 0.25 * CubeSize || Mathf.Abs (cf.transform.position.z) < 0.25 * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [30].SetEditColor (MagicColor.None);
			cubefaces [43].SetEditColor (MagicColor.None);
			cubefaces [3].SetEditColor (MagicColor.Yellow);
			cubefaces [37].SetEditColor (MagicColor.Red);
			break;
		case "FEdge3":
			// R0
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y > 0.75 * CubeSize &&
				    (Mathf.Abs (cf.transform.position.x) < 0.25 * CubeSize || Mathf.Abs (cf.transform.position.z) < 0.25 * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [30].SetEditColor (MagicColor.None);
			cubefaces [43].SetEditColor (MagicColor.None);
			cubefaces [48].SetEditColor (MagicColor.Yellow);
			cubefaces [41].SetEditColor (MagicColor.Red);
			break;
		case "FEdge4":
			// R,U0,B,U
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y > 0.75 * CubeSize &&
				    (Mathf.Abs (cf.transform.position.x) < 0.25 * CubeSize || Mathf.Abs (cf.transform.position.z) < 0.25 * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [30].SetEditColor (MagicColor.Red);
			cubefaces [43].SetEditColor (MagicColor.Yellow);
			break;
		case "FCorner1":
			// R,U,R0,U0,R,U,R0,U0,R,U,R0,U0
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < -0.75 * CubeSize 
					/*&&
				    (Mathf.Abs (cf.transform.position.x) < 0.25 * singlecubesize || 
						Mathf.Abs (cf.transform.position.z) < 0.25 * singlecubesize)*/) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [0].SetEditColor (MagicColor.None);
			cubefaces [18].SetEditColor (MagicColor.None);
			cubefaces [36].SetEditColor (MagicColor.None);
			cubefaces [24].SetEditColor (MagicColor.Red);
			cubefaces [27].SetEditColor (MagicColor.White);
			cubefaces [42].SetEditColor (MagicColor.Blue);
			break;
//		case "FCorner2":
//			// R,U0,R0,F0,U2,F
//			foreach (CubeFace cf in cubefaces) {
//				if (cf.transform.position.y < -0.75f * singlecubesize &&
//				    (Mathf.Abs (cf.transform.position.x) < 0.25f * singlecubesize ||
//				    Mathf.Abs (cf.transform.position.z) < 0.25f * singlecubesize)) {
//					cf.UpdateFaceStyle ();
//					cf.SetDefaultColor ();
//				}
//			}
//			cubefaces [24].SetEditColor (MagicColor.Red);
//			cubefaces [27].SetEditColor (MagicColor.White);
//			cubefaces [42].SetEditColor (MagicColor.Blue);
//			break;
		case "SEdge1":
			// U,R,U0,R0,U0,F0,U,F
			// F,U,F,U,F,U0,F0,U0,F0
			// 第二层棱块公式
			// 首先需要初始化魔方状态，然后运行公式，魔方的位置不能动。
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < -0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [25].UpdateFaceStyle ();
			cubefaces [25].SetDefaultColor ();
			cubefaces [28].SetEditColor (MagicColor.Red);
			break;
		case "SEdge2":
			// U0,F0,U,F,U,R,U0,R0
			// R0,U0,R0,U0,R0,U,R,U,R
			// 第二层棱块公式
			// 首先需要初始化魔方状态，然后运行公式，魔方的位置不能动。
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < -0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [43].UpdateFaceStyle ();
			cubefaces [43].SetDefaultColor ();
			cubefaces [30].SetEditColor (MagicColor.Blue);
			break;
		case "TCross1":
			// F,R,U,R0,U0,F0
			// 第三层顶面中心点状态
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 0.5f * CubeSize &&
				           cf.transform.position.y < 1.25f * CubeSize &&
				           (Mathf.Abs (cf.transform.position.x) < 0.25f * CubeSize ||
				           Mathf.Abs (cf.transform.position.z) < 0.25f * CubeSize)) {
					cf.SetEditColor (MagicColor.Yellow);
				}
			}
			break;
		case "TCross2":
			// F,R,U,R0,U0,F0
			// 第三层顶面中折点状态
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [34].SetEditColor (MagicColor.Yellow);
			cubefaces [32].SetEditColor (MagicColor.Yellow);
			cubefaces [25].SetEditColor (MagicColor.Yellow);
			cubefaces [43].SetEditColor (MagicColor.Yellow);
			break;
		case "TCross3":
			// F,R,U,R0,U0,F0
			// 第三层顶面直线状态
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [30].SetEditColor (MagicColor.Yellow);
			cubefaces [32].SetEditColor (MagicColor.Yellow);
			cubefaces [25].SetEditColor (MagicColor.Yellow);
			cubefaces [52].SetEditColor (MagicColor.Yellow);
			break;
		case "TEdge1":
			// R,U,R0,U,R,U2,R0
			// 第三层，十字对面错位
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 0.5f * CubeSize &&
				           (Mathf.Abs (cf.transform.position.x) < 0.25f * CubeSize ||
				           Mathf.Abs (cf.transform.position.z) < 0.25f * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [43].SetEditColor (MagicColor.Orange);
			cubefaces [16].SetEditColor (MagicColor.Red);
			break;
		case "TEdge2":
			// R,U,R0,U,R,U2,R0,U
			// 第三层，十字相邻错位
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 0.5f * CubeSize &&
				           (Mathf.Abs (cf.transform.position.x) < 0.25f * CubeSize ||
				           Mathf.Abs (cf.transform.position.z) < 0.25f * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [25].SetEditColor (MagicColor.Orange);
			cubefaces [16].SetEditColor (MagicColor.Blue);
			break;
		case "TTopface1":
			// U,R,U0,L0,U,R0,U0,L
			// 第三层，十字完成，需要一步到十字状态（就是四角上面都没有顶面颜色）
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 0.5f * CubeSize &&
				           (Mathf.Abs (cf.transform.position.x) < 0.25f * CubeSize ||
				           Mathf.Abs (cf.transform.position.z) < 0.25f * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [24].SetEditColor (MagicColor.Yellow);
			cubefaces [29].SetEditColor (MagicColor.Yellow);
			cubefaces [33].SetEditColor (MagicColor.Yellow);
			cubefaces [17].SetEditColor (MagicColor.Yellow);
			break;
		case "TTopface2":
			// U,R,U0,L0,U,R0,U0,L
			// 第三层，十字完成，需要一步到十字状态（就是四角上面都没有顶面颜色）
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 0.5f * CubeSize &&
				           (Mathf.Abs (cf.transform.position.x) < 0.25f * CubeSize ||
				           Mathf.Abs (cf.transform.position.z) < 0.25f * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [24].SetEditColor (MagicColor.Yellow);
			cubefaces [26].SetEditColor (MagicColor.Yellow);
			cubefaces [33].SetEditColor (MagicColor.Yellow);
			cubefaces [35].SetEditColor (MagicColor.Yellow);
			break;
		case "TTopface3":
			// U,R,U0,L0,U,R0,U0,L
			// 第三层，十字完成，需要一步到十字状态（就是四角上面都没有顶面颜色）
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 0.5f * CubeSize &&
				           (Mathf.Abs (cf.transform.position.x) < 0.25f * CubeSize ||
				           Mathf.Abs (cf.transform.position.z) < 0.25f * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [24].SetEditColor (MagicColor.Yellow);
			cubefaces [51].SetEditColor (MagicColor.Yellow);
			cubefaces [35].SetEditColor (MagicColor.Yellow);
			cubefaces [29].SetEditColor (MagicColor.Yellow);
			break;
		case "TTopface4":
			// U,R,U0,L0,U,R0,U0,L
			// 第三层，十字完成，顶面四角都没有顶面颜色，需要一步到小鱼状态（就是四角上面只有一个是顶面颜色）
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 0.5f * CubeSize &&
				           (Mathf.Abs (cf.transform.position.x) < 0.25f * CubeSize ||
				           Mathf.Abs (cf.transform.position.z) < 0.25f * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [26].SetEditColor (MagicColor.Yellow);
			cubefaces [24].SetEditColor (MagicColor.Yellow);
			cubefaces [53].SetEditColor (MagicColor.Yellow);
			cubefaces [51].SetEditColor (MagicColor.Yellow);
			break;
		case "TTopface5":
			// R,U,R0,U,R,U2,R0,U
			// 第三层，十字完成，顶面四角都没有顶面颜色，需要一步到小鱼状态（就是四角上面只有一个是顶面颜色）
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 0.5f * CubeSize &&
				           (Mathf.Abs (cf.transform.position.x) < 0.25f * CubeSize ||
				           Mathf.Abs (cf.transform.position.z) < 0.25f * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [24].SetEditColor (MagicColor.Yellow);
			cubefaces [51].SetEditColor (MagicColor.Yellow);
			cubefaces [15].SetEditColor (MagicColor.Yellow);
			cubefaces [17].SetEditColor (MagicColor.Yellow);
			break;
		case "TTopface6":
			// U,R,U0,L0,U,R0,U0,L
			// 第三层，十字完成，就是四角上面只有一个是顶面颜色，按照公式转换成另外一个小鱼状态，再按照公式就可以完成顶面
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 0.5f * CubeSize &&
				           (Mathf.Abs (cf.transform.position.x) < 0.25f * CubeSize ||
				           Mathf.Abs (cf.transform.position.z) < 0.25f * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [26].SetEditColor (MagicColor.Yellow);
			cubefaces [17].SetEditColor (MagicColor.Yellow);
			cubefaces [27].SetEditColor (MagicColor.Yellow);
			cubefaces [51].SetEditColor (MagicColor.Yellow);
			break;
		case "TTopface7":
			// U,R,U0,L0,U,R0,U0,L
			// 第三层，十字完成，小鱼公式，可以使用一个公式转成另外一个小鱼状态，
			// 也可以使用一个公式，来完成上面顶面颜色层
			foreach (CubeFace cf in cubefaces) {
				if (cf.transform.position.y < 0.5f * CubeSize) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				} else if (cf.transform.position.y > 0.5f * CubeSize &&
				           (Mathf.Abs (cf.transform.position.x) < 0.25f * CubeSize ||
				           Mathf.Abs (cf.transform.position.z) < 0.25f * CubeSize)) {
					cf.UpdateFaceStyle ();
					cf.SetDefaultColor ();
				}
			}
			cubefaces [44].SetEditColor (MagicColor.Yellow);
			cubefaces [15].SetEditColor (MagicColor.Yellow);
			cubefaces [27].SetEditColor (MagicColor.Yellow);
			cubefaces [53].SetEditColor (MagicColor.Yellow);
			break;
			#endregion

		case "TCorner":
			// R0,D0,R,D
			// 第三层，顶面颜色层已经完成，只剩下四个角位置可能不对，
//			foreach (CubeFace cf in cubefaces) {
//				if (cf.transform.position.y < 0.5f * singlecubesize) {
//					cf.UpdateFaceStyle ();
//					cf.SetDefaultColor ();
//				} else if (cf.transform.position.y > 0.5f * singlecubesize &&
//				           (Mathf.Abs (cf.transform.position.x) < 0.25f * singlecubesize ||
//				           Mathf.Abs (cf.transform.position.z) < 0.25f * singlecubesize)) {
//					cf.UpdateFaceStyle ();
//					cf.SetDefaultColor ();
//				}
//			}
//			cubefaces [44].SetEditColor (MagicColor.Yellow);
//			cubefaces [15].SetEditColor (MagicColor.Yellow);
//			cubefaces [27].SetEditColor (MagicColor.Yellow);
//			cubefaces [53].SetEditColor (MagicColor.Yellow);
			break;
		default:
			break;
		}





	}
}
