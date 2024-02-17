using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// �̴ϸ� ��Ʈ�ѷ�
public class MiniMapController : MonoBehaviour
{
    // �� ���� ũ���� Plane ������Ʈ ũ��
    public Vector3 _worldSize;
    // �̴ϸ� ScorllRect
    public ScrollRect _scrollRect;
    // �÷��̾� ������(���� �����)
    public GameObject _playerIcon;
    // �� ���� ũ���� Plane ������Ʈ
    public GameObject _plane;
    // �÷��̾� ������Ʈ
    public GameObject _playerObject;

    void Start()
    {
        // �������� ���� �� �߾����� ǥ��, but �ǹ� ����
        _scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
        // Plane�� ������ ������
        _worldSize = _plane.transform.GetComponent<MeshCollider>().bounds.size;
        // �÷��̾� ������Ʈ �� �־���
        _playerObject = GameManager.Obj._player.gameObject;
    }

    void Update()
    {
        // �� ��ġ �ǽð� ����
        TryCalculate();
        // �÷��̾� ������ ȸ��
        RotatePlayerIcon();
    }

    // �� ��ġ ����ϴ� �Լ�
    void TryCalculate()
    {
        // Player�� Plane�� ������� ��ġ��
        // Plane(�θ�) - Player(�ڽ�) ������ localPosition���� ������ �� ������ ĳ���� ������ ���� �ٲ�
        // �̷��� �����ġ ������
        Vector3 relativePos = _playerObject.transform.position - _plane.transform.position;
        
        // ScrollRect�� Pivot�� ���ϴ��̱� ������ ��ġ ���� Plane�� ���� ���� ������ ���� ������
        relativePos.x += _worldSize.x * 0.5f;
        relativePos.z += _worldSize.z * 0.5f;

        // ���� �÷��̾��� ��ġ�� ������ �˱� ���Ͽ� InverseLerp �Լ� ���(0 ~ 1 ���̰�)
        float playerX = Mathf.InverseLerp(0, _worldSize.x, relativePos.x);
        float playerY = Mathf.InverseLerp(0, _worldSize.z, relativePos.z);
        // ���� ��ġ ����
        _scrollRect.normalizedPosition = new Vector2(playerX, playerY);
    }

    // �÷��̾� ������ ȸ����Ű�� �Լ�
    void RotatePlayerIcon()
    {
        // �÷��̾� ������Ʈ ȸ�� y�� ī�޶� ���� ���� �ݴ� ������ ���� �ֱ� ������ 360���� ���� ��
        float rotateValue = 360f - _playerObject.transform.rotation.eulerAngles.y;
        // �÷��̾� ������ RectTransform ������
        RectTransform iconRect = _playerIcon.GetComponent<RectTransform>();
        // ������ rotation z�� �ٲ���
        iconRect.rotation = Quaternion.Euler(0, 0, rotateValue);
    }
}
