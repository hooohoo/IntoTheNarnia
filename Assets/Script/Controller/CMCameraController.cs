using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;
using static Define;

// �ó׸ӽ� ��Ʈ�ѷ�
public class CMCameraController : MonoBehaviour, IPointerClickHandler
{
    // �� ��ũ��Ʈ ����� �ó׸ӽ� ī�޶�
    public CinemachineFreeLook _camera;

    // ���� �����ϱ� ���� Ŭ���� ������ üũ�ϱ� ���� ����(ex) UI Ŭ���ϸ� false)
    public bool viewChange;

    void Awake()
    {
        // ������ ���� ī�޶� �־��ֱ�
        _camera = GameManager.Camera._cmCam.GetComponent<CinemachineFreeLook>();
        
        // Follow ����
        _camera.Follow = GameManager.Obj._player.transform;
        // Look At ����
        _camera.LookAt = GameManager.Obj._player.transform;
        
        // �ϴ� true�� ����
        viewChange = true;
        // Ŭ������ ���� InputAxis ������
        CinemachineCore.GetInputAxis = ClickControl;
    }

    void Start()
    {
        //
    }

    void Update()
    {
        // ��ũ�� Ŭ�������� üũ
        CheckClickScreen();
    }

    // ���콺 Ŭ���� ���� ���� ������ �� �ֵ��� �ϴ� �Լ�
    public float ClickControl(string axis)
    {
        // ���콺 ��Ŭ�� �ϰ� ���̽�ƽ �ȴ����� ����
        if(Input.GetMouseButton(0) && viewChange)
        {
            return UnityEngine.Input.GetAxis(axis);
        }
        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("game object �̸� : " + eventData.pointerCurrentRaycast.gameObject.name);
    }

    // ���̽�ƽ Ŭ���ϰ��ִ� ������ ��ũ�� �����ϴ� ������
    public void CheckClickScreen()
    {
        // ���� ���̽�ƽ Ŭ���� ���¶��
        if(GameManager.Ui._joyStickController._joystickState == JoystickState.InputTrue)
        {
            // ���� ���� false
            viewChange = false;
        }
        else
        {
            // ���� ���� true
            viewChange = true;
        }
        //Debug.Log("viewChange : " + viewChange);
    }
}
