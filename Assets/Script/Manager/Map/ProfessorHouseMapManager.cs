using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// Professor_House �� ���� Manager
public class ProfessorHouseMapManager : MonoBehaviour
{
    // �� ���ο� �ִ� �� ����Ʈ
    private DoorController[] _doorList;

    // -- �Ʒ� �������� ObjectManager�� �����ϵ��� ����
    // �����ִ� �� ��(������Ʈ : "TheWardrobeRoomDoor")
    //private DoorController _theWardrobeRoomDoor;
    // ������ �� ��(������Ʈ : "TheSiblingsRoomDoor")
    //private DoorController _theSiblingsRoomDoor;

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
        // �����
        Debug.Log("���� ����Ʈ ���� : " + QuestManager.QuestLevel);
        // ����Ʈ
        switch(QuestManager.QuestLevel)
        {
            // �������ڸ��� ����Ʈ ���� +1
            case 0:
                // UI ��� ���� Ʃ�丮�� ����
                GameManager.Ui.AllUIOff();
                GameManager.Quest.QuestLevelUp();
                break;
            case 1:
                Quest_1();
                break;
            case 2:
                Quest_2();
                break;
            default:
                Debug.Log("���� �ʿ��� ������ ����Ʈ�� �����ϴ�.");
                break;
        }
    }

    // �ʱ�ȭ
    private void Init()
    {
        // Find �Լ� ������� �ʴ� ���� ������.. �� ���ο� �ִ� ��� door �������� ���� ���
        _doorList = FindObjectsOfType<DoorController>();
        
        // �ֿ� ������Ʈ�� ���� �����ϱ�
        CollectMainObject();

        // TheWardrobeRoom, TheSiblingsRoom �ᰡ�α�, ����Ʈ ������ �� �� ��
        GameManager.Obj._theWardrobeRoomDoor._openOrLocked = false;
        GameManager.Obj._theSiblingsRoomDoor._openOrLocked = false;

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
                GameManager.Obj._theWardrobeRoomDoor = one;
            }
            // ������ �� �� ����
            if(one.name.Equals(ObjectName.TheSiblingsRoomDoor.ToString()))
            {
                GameManager.Obj._theSiblingsRoomDoor = one;
            }
        }
    }// end CollectMainObject()

    // Professor_House ���� ������ ����Ʈ
    // 1. ������(TheSiblingsRoom) ã�ư���
    // 2. ���ٲ����ϸ鼭 �����ִ� ��(TheWardrobeRoom) ����
    public void Quest_1()
    {
        // #1
        // ������ ��� ����
        GameManager.Obj._theSiblingsRoomDoor._openOrLocked = true;
        // ����Ʈ ���� �ƴϸ� ���ͼ� ����
        if(!GameManager.Quest._successOrNot)
        {
            // ������(TheSiblingsRoom) �� ������ ����Ʈ ����
            if(GameManager.Obj._theSiblingsRoomDoor._openOrClose)
            {
                // �����޼��� UI�� �ٲ� ��
                Debug.Log("ù ��° ����Ʈ ����!");
                GameManager.Quest._successOrNot = true;
            }
        }
        else
        {
            // �����ϸ� ���� ����Ʈ�� ���� false�� �缳��
            GameManager.Quest._successOrNot = false;
            // ����Ʈ ���� +1
            GameManager.Quest.QuestLevelUp();
        }
    }

    private void Quest_2()
    {
        // #2
        // ���� �� ��� ����
        GameManager.Obj._theWardrobeRoomDoor._openOrLocked = true;
        // �������� �� Ż���ϵ���
        if(!GameManager.Quest._successOrNot)
        {
            // ���� ��(TheWardRobeRoom) �� ������ ����Ʈ ����
            if(GameManager.Obj._theWardrobeRoomDoor._openOrClose)
            {
                // �����޼��� UI�� �ٲ� ��
                Debug.Log("�� ��° ����Ʈ ����!");
                GameManager.Quest._successOrNot = true;
            }
        }
        else
        {
            // ���� ����Ʈ�� ���� false�� �缳��
            GameManager.Quest._successOrNot = false;
            // ����Ʈ ���� +1
            GameManager.Quest.QuestLevelUp();
        }
        
    }
}
