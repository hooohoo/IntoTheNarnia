using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���̽�ƽ ��Ʈ�ѷ�
// ī�޶� �ٶ󺸴� ������ ��. ī�޶�� ĳ���Ͱ� ���ֺ��� �ִٸ� ��������� ������ ĳ���Ͱ� ī�޶� ������ �ٰ���
public class JoystickController : MonoBehaviour
{
    // ī�޶� ������Ʈ �������� �ɷ� ���߿� ����, �ϴ� public
    public GameObject _Camera;
    public GameObject _Character;
    Vector3 _direction;
    

    void Start()
    {
        _direction = _Camera.transform.forward;
        
    }

    void Update()
    {
        
    }
}
