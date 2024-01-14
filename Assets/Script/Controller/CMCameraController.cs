using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

// �ó׸ӽ� ��Ʈ�ѷ�
public class CMCameraController : MonoBehaviour, IPointerClickHandler
{
    // ���� �����ϱ� ���� Ŭ���� ������ üũ�ϱ� ���� ����(ex) UI Ŭ���ϸ� false)
    public bool viewChange;

    void Awake()
    {
        viewChange = true;
        CinemachineCore.GetInputAxis = ClickControl;
    }

    void Start()
    {
        //
    }

    void Update()
    {
        //
    }

    // ���콺 Ŭ���� ���� ���� ������ �� �ֵ��� �ϴ� �Լ�
    public float ClickControl(string axis)
    {
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
}
