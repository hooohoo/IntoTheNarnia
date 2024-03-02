using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// ����Ʈ �Ŵ���
// ����Ʈ ���� ��ɵ� ��Ƴ��� �Ŵ���
public class QuestManager
{
    // �÷��̾� �׺�޽� ������Ʈ ������Ʈ
    private NavMeshAgent _playerNavmeshAgent;
    // ������Ʈ Area Mask
    private int _areaMask;

    // �������� Ȯ���ϴ� ����, ������ true ���д� false
    public bool _successOrNot;

    // �ʱ�ȭ
    public void Init()
    {
        // �׺�޽� ������Ʈ �־���
        _playerNavmeshAgent = GameManager.Obj._player.GetComponent<NavMeshAgent>();
        // ������Ʈ Area Mask �־���
        _areaMask = _playerNavmeshAgent.areaMask;
        // ���������� �� �ֱ�
        _successOrNot = false;
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
