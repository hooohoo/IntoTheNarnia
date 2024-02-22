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
                // ������, ��������� ��� �ִϸ��̼�
                if(_thisDoor != null)
                {
                    _thisDoor.OpenAndCloseDoor();
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
