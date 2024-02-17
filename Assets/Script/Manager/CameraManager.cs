using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 카메라들 관리하는 클래스
public class CameraManager
{
    // 시네머신 카메라
    public GameObject _cmCam;
    public CMCameraController _cmCamCotroller;

    // FieldManager에서 사용할 Init() 함수
    public void Init()
    {
        // 시네머신 컨트롤러
        _cmCamCotroller = _cmCam.GetComponent<CMCameraController>();
    }
}
