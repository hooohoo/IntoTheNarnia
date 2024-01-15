using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

// �ó׸ӽ� ��Ʈ�ѷ�
public class CMCameraController : MonoBehaviour, IPointerClickHandler
{
    // �� ��ũ��Ʈ ����� �ó׸ӽ� ī�޶�
    CinemachineFreeLook _camera;

    // ���� �����ϱ� ���� Ŭ���� ������ üũ�ϱ� ���� ����(ex) UI Ŭ���ϸ� false)
    public bool viewChange;

    void Awake()
    {
        // ������ ���� ī�޶� �־��ֱ�
        _camera = transform.GetComponent<CinemachineFreeLook>();
        // �ϴ� true�� ����
        viewChange = true;
        CinemachineCore.GetInputAxis = ClickControl;
    }

    void Start()
    {
        //
    }

    void Update()
    {
        CheckClickScreen();
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

    public void CheckClickScreen()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Input.GetMouseButtonDown(0))
        {
            if(Physics.Raycast(ray, out hit))
            {
                Debug.Log("hit : " + hit.transform.gameObject.name);
            }
        }
        //if(Input.GetMouseButtonDown(0))
        //{
        //    Debug.Log("Ŭ�� ������Ʈ : " + EventSystem.current.currentSelectedGameObject.name);
        //}
    }
}
