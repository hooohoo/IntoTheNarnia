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
    private static int _questLevel;

    // �÷��̾� �׺�޽� ������Ʈ ������Ʈ
    private NavMeshAgent _playerNavmeshAgent;
    // ������Ʈ Area Mask
    private int _areaMask;

    // �������� Ȯ���ϴ� ����, ������ true ���д� false
    public bool _successOrNot;

    // _questLevel Getter
    public static int QuestLevel
    {
        get { return _questLevel; }
    }
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
    // string Ÿ������ ������
    public void OpenDoor(string objName)
    {
        // Area Mask�� ������ == �÷��̾ �ٴ� �� �ִ� �������� ����
        // GetAreaFromName("Everything") �� ��� �������� ����
        _areaMask += 1 << NavMesh.GetAreaFromName(objName);
        _playerNavmeshAgent.areaMask = _areaMask;
    }
}
