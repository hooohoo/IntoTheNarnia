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
    private DoorController _theWardrobeRoomDoor;
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

        // ����Ʈ ���⼭ �ϸ� �ȵ�... ��ġ ã�ƾ�, Save�� �����ϱ�
        //Quest_1();
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

        // TheWardrobeRoom, TheSiblingsRoom �ᰡ�α�, ����Ʈ ������ �� �� ��
        _theWardrobeRoomDoor._openOrLocked = false;
        _theSiblingsRoomDoor._openOrLocked = false;

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
                _theWardrobeRoomDoor = one;
            }
            // ������ �� �� ����
            if(one.name.Equals(ObjectName.TheSiblingsRoomDoor.ToString()))
            {
                _theSiblingsRoomDoor = one;
            }
        }
    }// end CollectMainObject()

    // Professor_House ���� ������ ����Ʈ
    // 1. ������(TheSiblingsRoom) ã�ư���
    // 2. ���ٲ����ϸ鼭 �����ִ� ��(TheWardrobeRoom) ����
    private void Quest_1()
    {
        // #1
        // ������ ��� ����
        _theSiblingsRoomDoor._openOrLocked = true;
        // �������� �� Ż���ϵ���
        while(!GameManager.Quest._successOrNot)
        {
            // ������(TheSiblingsRoom) �� ������ ����Ʈ ����
            if(_theSiblingsRoomDoor._openOrClose)
            {
                // �����޼��� UI�� �ٲ� ��
                Debug.Log("ù ��° ����Ʈ ����!");
                GameManager.Quest._successOrNot = true;
            }
        }
        // ���� ����Ʈ�� ���� false�� �缳��
        GameManager.Quest._successOrNot = false;
    }

    private void Quest_2()
    {
        // #2
        // ���� �� ��� ����
        _theWardrobeRoomDoor._openOrLocked = true;
        // �������� �� Ż���ϵ���
        while(!GameManager.Quest._successOrNot)
        {
            // ���� ��(TheWardRobeRoom) �� ������ ����Ʈ ����
            if(_theWardrobeRoomDoor._openOrClose)
            {
                // �����޼��� UI�� �ٲ� ��
                Debug.Log("ù ��° ����Ʈ ����!");
                GameManager.Quest._successOrNot = true;
            }
        }
        // ���� ����Ʈ�� ���� false�� �缳��
        GameManager.Quest._successOrNot = false;
    }
}
