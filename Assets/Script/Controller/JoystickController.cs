using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 조이스틱 컨트롤러
// 카메라가 바라보는 방향이 앞. 카메라와 캐릭터가 마주보고 있다면 ↓방향으로 눌러야 캐릭터가 카메라 쪽으로 다가옴
public class JoystickController : MonoBehaviour
{
    // 카메라 오브젝트 가져오는 걸로 나중에 수정, 일단 public
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
