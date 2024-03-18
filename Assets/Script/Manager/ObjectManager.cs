using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������Ʈ ���� ��� ���ִ� Manager Ŭ����
public class ObjectManager
{
    // �÷��̾�
    public PlayerController _player;

    // ����Ʈ�� �ʿ��� ������Ʈ ��
    // ProfessorHouse �� ���� ������Ʈ
    public DoorController _theWardrobeRoomDoor;
    public DoorController _theSiblingsRoomDoor;

    // ����Ʈ ������
    // ĳ���� ���
    public List<CharacterLineClass> _characterLineList;

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
