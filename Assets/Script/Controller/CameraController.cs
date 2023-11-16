using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // �÷��̾� ������Ʈ
    public GameObject _Player;
    // ī�޶� ���� ��ġ ���� ����
    Vector3 _CameraPos;
    // �÷��̾��� ���� ��ġ
    Vector3 _OldPos;

    void Start()
    {
        // �ӽ÷� �� �ڵ�
        _CameraPos = transform.position;
        _OldPos = _Player.transform.position;
    }

    void Update()
    {
        FollowingPlayer();
    }

    // �÷��̾� ���󰡴� �Լ�
    void FollowingPlayer()
    {
        Vector3 delta = _Player.transform.position - _OldPos;
        transform.position += delta;
        _OldPos = _Player.transform.position;
    }
}
