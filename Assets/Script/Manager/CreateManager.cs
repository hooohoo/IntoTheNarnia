using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ĳ���� �� ������Ʈ�� ���� ������ ����� �Լ�(Instantiate() ���)
public class CreateManager
{
    // �÷��̾� ĳ����
    private PlayerController _player;

    // �÷��̾� ĳ���� ����
    public PlayerController CreatePlayer(Vector3 origin, string characterName)
    {
        // ������ ���̸� ���� ���� ���̿� ���� ĳ���� ���� �ڵ�
        origin.y += 100f;
        RaycastHit hit;
        if(Physics.Raycast(origin, -Vector3.up, out hit, Mathf.Infinity))
        {
            GameObject tempObj = GameManager.Resource.GetPlayer(characterName);
            // null üũ �ϱ�
            if(tempObj != null)
            {
                GameObject player = GameObject.Instantiate<GameObject>(tempObj, hit.point, Quaternion.identity);
                // �����տ� �����Ǿ��ִ� PlayerController ��ũ��Ʈ ��������
                _player = player.GetComponent<PlayerController>();
                // �̸� ����, �̸��� �� ������ �Ѿ��, ���� ����Ʈ�Ŵ����� �����Ҽ��� ����
                _player._characterName = characterName;
            }
        }
        return _player;
    }
}
