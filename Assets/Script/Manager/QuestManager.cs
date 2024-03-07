using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// ����Ʈ �Ŵ���
// ����Ʈ ���� ��ɵ� ��Ƴ��� �Ŵ���
public class QuestManager
{
    // ����Ʈ ����
    // �÷��̾� ���� ���Ͽ��� ��������
    private int _questLevel;

    // �÷��̾� �׺�޽� ������Ʈ ������Ʈ
    private NavMeshAgent _playerNavmeshAgent;
    // ������Ʈ Area Mask
    private int _areaMask;

    // �������� Ȯ���ϴ� ����, ������ true ���д� false
    public bool _successOrNot;

    // �ʱ�ȭ
    public void Init()
    {
        // �ӽ� �ڵ�, �Ŀ� ���� ���Ͽ��� �ҷ��� �� �־�� ��
        // ���� �ʱ�ȭ
        _questLevel = 0;

        // �׺�޽� ������Ʈ �־���
        _playerNavmeshAgent = GameManager.Obj._player.GetComponent<NavMeshAgent>();
        // ������Ʈ Area Mask �־���
        _areaMask = _playerNavmeshAgent.areaMask;
        // ���������� �� �ֱ�
        _successOrNot = false;
    }

    // ����Ʈ ���� �� �Լ�
    public void QuestLevelUp()
    {
        _questLevel++;
    }

    // ������ �ϴ� �� �����ͼ� �ش� ���� AreaMask�� �߰��ϴ� �Լ�
    // Define.ObjectName Ÿ������ ������
    public void OpenDoor(Define.ObjectName objName)
    {
        // Area Mask�� ������ == �÷��̾ �ٴ� �� �ִ� �������� ����
        // GetAreaFromName("Everything") �� ��� �������� ����
        _areaMask += 1 << NavMesh.GetAreaFromName(objName.ToString());
        _playerNavmeshAgent.areaMask = _areaMask;
    }
}
