using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// �׼� ��ư(��ȣ�ۿ� ��ư) ��Ʈ�ѷ�
// �±׷� �����Ͽ� Structure �̸� ��ȣ�ۿ�, Monster �̸� �������� ��ȯ�ϴ� ��Ʈ�ѷ�
public class ActionButtonController : MonoBehaviour
{
    // ������Ʈ�� ���������� �������� �����ϱ� ���� ����
    public TagName _verifyTag;
    // ���� �÷��̾ ������ ��
    public DoorController _thisDoor;

    void Start()
    {
        //
    }

    void Update()
    {
        //
    }

    // �±׿� ���� ��ȣ�ۿ� Ȥ�� �����ϴ� �Լ�
    public void DoActionOrDoAttack()
    {
        //
        switch(_verifyTag)
        {
            // �������� ���, ����� ��
            case TagName.Structure:
                // null üũ
                if(_thisDoor != null)
                {
                    // ������, ��������� ��� �ִϸ��̼�
                    _thisDoor.OpenAndCloseDoor();
                    // ����ִ� ���� �ƴ� ���� ������ �� �ֵ���
                    if(_thisDoor._openOrLocked)
                    {
                        // navmesh ���� walkable�� ����
                        GameManager.Quest.OpenDoor(_thisDoor.name);
                    }
                }
                // �������� �±� ����ֱ�
                _verifyTag = TagName.None;
                break;
            // ������ ���, ����
            case TagName.Monster:
                // todo
                // �������� �±� ����ֱ�
                _verifyTag = TagName.None;
                break;
            // _verifyTag�� ������ �ƹ��͵� ���� ����
            default:
                break;
        }
    }
}
