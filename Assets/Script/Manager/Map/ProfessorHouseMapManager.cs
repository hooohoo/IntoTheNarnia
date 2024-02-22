using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// Professor_House �� ���� Manager
public class ProfessorHouseMapManager : MonoBehaviour
{
    // �� ���ο� �ִ� �� ����Ʈ
    private DoorController[] _doorList;

    // �����ִ� �� ��(������Ʈ : "TheWardrobeRoomDoor")
    private DoorController _theWardRobeRoomDoor;
    // ������ �� ��(������Ʈ : "TheSiblingsRoomDoor")
    private DoorController _theSiblingsRoomDoor;

    // ��Ż
    // SiblingsRoom ���� �̵��ϴ� ��Ż
    public NextMapController _theSiblingsRoomPortal;
    // WardrobeRoom ���� �̵��ϴ� ��Ż
    public NextMapController _theWardrobeRoomPortal;

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
        // Find �Լ� ������� �ʴ� ���� ������.. �� ���ο� �ִ� ��� door �������� ���� ���
        _doorList = FindObjectsOfType<DoorController>();
        
        // �ֿ� ������Ʈ�� ���� �����ϱ�
        CollectMainObject();

        // TheWardrobeRoom, TheSiblingsRoom ����α�
        _theWardRobeRoomDoor._openOrLocked = true;
        _theSiblingsRoomDoor._openOrLocked = true;

        // ��Ż�� �� �����صα�
        _theSiblingsRoomPortal._linkedScene = SceneName.TheSiblingsRoom;
        _theWardrobeRoomPortal._linkedScene = SceneName.TheWardrobeRoom;
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
