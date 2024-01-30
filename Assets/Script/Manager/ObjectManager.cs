using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 오브젝트 관련 기능 모여있는 Manager 클래스
public class ObjectManager
{
    // 이름으로 검색해서 GameObject 반환하는 함수
    public GameObject GetObjectByName(List<GameObject> list, string objName)
    {
        foreach(GameObject one in list)
        {
            if(one.name.Equals(objName))
            {
                // 일치하면 return
                return one;
            }
        }
        // 아니면 null return
        return null;
    }
}
