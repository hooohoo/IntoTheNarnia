using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

// 시네머신 컨트롤러
public class CMCameraController : MonoBehaviour, IPointerClickHandler
{
    // 이 스크립트 적용된 시네머신 카메라
    CinemachineFreeLook _camera;

    // 시점 변경하기 위해 클릭한 것인지 체크하기 위한 변수(ex) UI 클릭하면 false)
    public bool viewChange;

    void Awake()
    {
        // 변수에 실제 카메라 넣어주기
        _camera = transform.GetComponent<CinemachineFreeLook>();
        // 일단 true로 설정
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

    // 마우스 클릭할 때만 시점 변경할 수 있도록 하는 함수
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
        Debug.Log("game object 이름 : " + eventData.pointerCurrentRaycast.gameObject.name);
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
        //    Debug.Log("클릭 오브젝트 : " + EventSystem.current.currentSelectedGameObject.name);
        //}
    }
}
