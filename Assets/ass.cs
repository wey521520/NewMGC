using UnityEngine;
using System.Collections;

public class ass : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		int CubeClass = 5;
		int numInClass = CubeClass * CubeClass;
		int numInCube = numInClass * CubeClass;

		// 先实例化魔方的块（这里只实例化实际的东西，初始化的时候再判断游戏的模式）
		for (int i = 0; i < numInCube; i++) {
			if (i == 0 || i == CubeClass - 1 || i == numInClass - CubeClass || i == numInClass - 1 ||
			    i == numInCube - 1 || i == numInCube - CubeClass ||
			    i == numInCube - numInClass + CubeClass - 1 || i == numInCube - numInClass) {
				print (i + "是顶点");
			} else if (i > numInClass && i < numInCube - numInClass && i % numInClass > CubeClass && i % numInClass < numInClass - CubeClass &&
			           i % numInClass % CubeClass != 0 && i % numInClass % CubeClass != CubeClass - 1) {
				print (i + "是内点");
			} else if ((i > CubeClass && i < numInClass - CubeClass && i % CubeClass != 0 && i % CubeClass != CubeClass - 1) ||
			           (i < numInCube - CubeClass && i > numInCube - numInClass + CubeClass && i % CubeClass != 0 && i % CubeClass != CubeClass - 1) ||
			           i > numInClass && i < numInCube - numInClass && (((i % numInClass < CubeClass || i % numInClass > numInClass - CubeClass) &&
			           i % CubeClass != 0 && i % CubeClass != CubeClass - 1) || (i % numInClass >= CubeClass && i % numInClass < numInClass - CubeClass &&
			           (i % CubeClass == 0 || i % CubeClass == CubeClass - 1)))) {
				print (i + "是面");
			} else {
				print (i + "是棱");
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
