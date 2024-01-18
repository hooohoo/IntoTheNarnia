using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;
using static Define;

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
        // 클릭했을 때만 InputAxis 들어가도록
        CinemachineCore.GetInputAxis = ClickControl;
    }

    void Start()
    {
        //
    }

    void Update()
    {
        // 스크린 클릭중인지 체크
        CheckClickScreen();
    }

    // 마우스 클릭할 때만 시점 변경할 수 있도록 하는 함수
    public float ClickControl(string axis)
    {
        // 마우스 좌클릭 하고 조이스틱 안눌렀을 때만
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

    // 조이스틱 클릭하고있는 것인지 스크린 조작하는 것인지
    public void CheckClickScreen()
    {
        // 만약 조이스틱 클릭한 상태라면
        if(GameManager.Ui._joyStickController._joystickState == JoystickState.InputTrue)
        {
            // 시점 변경 false
            viewChange = false;
        }
        else
        {
            // 시점 변경 true
            viewChange = true;
        }
        //Debug.Log("viewChange : " + viewChange);
    }
}
