using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������Ʈ ���� ��� ���ִ� Manager Ŭ����
public class ObjectManager
{
    // �̸����� �˻��ؼ� GameObject ��ȯ�ϴ� �Լ�
    public GameObject GetObjectByName(List<GameObject> list, string objName)
    {
        foreach(GameObject one in list)
        {
            if(one.name.Equals(objName))
            {
                // ��ġ�ϸ� return
                return one;
            }
        }
        // �ƴϸ� null return
        return null;
    }
}
