using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ī�޶�� �����ϴ� Ŭ����
public class CameraManager
{
    // �ó׸ӽ� ī�޶�
    public GameObject _cmCam;
    public CMCameraController _cmCamCotroller;

    // FieldManager���� ����� Init() �Լ�
    public void Init()
    {
        // �ó׸ӽ� ��Ʈ�ѷ�
        _cmCamCotroller = _cmCam.GetComponent<CMCameraController>();
    }
}
