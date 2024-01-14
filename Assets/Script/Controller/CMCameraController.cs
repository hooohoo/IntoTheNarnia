using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

// 시네머신 컨트롤러
public class CMCameraController : MonoBehaviour, IPointerClickHandler
{
    // 시점 변경하기 위해 클릭한 것인지 체크하기 위한 변수(ex) UI 클릭하면 false)
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
}
