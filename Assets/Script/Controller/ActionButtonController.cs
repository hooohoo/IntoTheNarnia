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

    void Start()
    {
        //
    }

    void Update()
    {
        //
    }

    // �±׿� ���� ��ȣ�ۿ� Ȥ�� �����ϴ� �Լ�
    private void DoActionOrDoAttack(TagName tag)
    {
        //
        switch(tag)
        {
            case TagName.Structure:
                break;
            case TagName.Monster:
                break;
            default:
                break;
        }
    }
}
