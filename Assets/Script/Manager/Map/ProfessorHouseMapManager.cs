using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// Professor_House �� ���� Manager
// �÷��̾� ���� ��ġ�� �����ϰ� ��ġ�� ��
public class ProfessorHouseMapManager : MonoBehaviour
{
    // �� ���ο� �ִ� �� ����Ʈ
    private DoorController[] _doorList;

    // �����ִ� �� ��(������Ʈ : "TheWardrobeRoomDoor")
    private DoorController _theWardRobeRoomDoor;
    // ������ �� ��(������Ʈ : "TheSiblingsRoomDoor")
    private DoorController _theSiblingsRoomDoor;

    // �÷��̾� ������ ��ġ == �� ������Ʈ ��ġ�� ��
    private Vector3 _startPos;

    void Start()
    {
        // �ʱ�ȭ
        Init();
    }

    void Update()
    {
        //
    }

    // �ʱ�ȭ
    private void Init()
    {
        // ������ġ ���
        _startPos = transform.position;
        
        // �÷��̾� ����
        //CreatePlayer();
        
        // Find �Լ� ������� �ʴ� ���� ������.. �� ���ο� �ִ� ��� door �������� ���� ���
        _doorList = FindObjectsOfType<DoorController>();
        
        // �ֿ� ������Ʈ�� ���� �����ϱ�
        CollectMainObject();
    }

    // �÷��̾� ���� �ڵ�
    private void CreatePlayer()
    {
        // ���� �ʿ��� ���ΰ��� Lucy
        GameManager.Obj._player = GameManager.Create.CreatePlayer(_startPos, CharacterName.Lucy.ToString());
    }

    // _doorList���� �ٽ� ������Ʈ ���� �����ϱ�
    private void CollectMainObject()
    {
        foreach(DoorController one in _doorList)
        {
            // ���� �� �� ����
            if(one.name.Equals(ObjectName.TheWardrobeRoomDoor.ToString()))
            {
                _theWardRobeRoomDoor = one;
            }
            // ������ �� �� ����
            if(one.name.Equals(ObjectName.TheSiblingsRoomDoor.ToString()))
            {
                _theSiblingsRoomDoor = one;
            }
        }
    }
}
