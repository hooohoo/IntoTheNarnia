using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using static Define;

public class PlayerController : MonoBehaviour
{
    // JoystickController 에서 받아온 direction 값
    protected Vector2 _inputDir;
    protected Vector3 _tempVector;
    protected Vector3 _tempDir;

    // 카메라 오브젝트
    public GameObject _camera;
    // 정면 방면
    Vector3 _faceDirection;
    // 카메라 각도
    float _camerAngle;

    // 캐릭터 상태
    public CreatureState _creatureState;
    // 캐릭터 이름
    public string _characterName;

    public float _moveSpeed;
    public float _rotationSpeed;
    public bool _walkOrRun;         // 걸으면 true 뛰면 false

    // 애니메이터
    private Animator _animator;
    // 애니메이터 파라미터
    private string _animParameter;

    void Start()
    {
        Init();
    }

    // 캐릭터 상태에 따라 함수 실행 & 애니메이션 실행
    void Update()
    {
        // 실시간으로 변경되는 카메라 앵글 구하기
        GetCameraAngle();

        //Debug.Log("camerAngle : " + camerAngle);
        switch (_creatureState)
        {
            case CreatureState.Idle:
                Idle();
                _animator.SetInteger(_animParameter ,0);
                break;
            case CreatureState.Move:
                Move();
                // 걷기면 앞의 int 값, 뛰기면 뒤의 int 값으로 animation 설정
                int tempAniInt = _walkOrRun ? 2 : 3;
                _animator.SetInteger(_animParameter, tempAniInt);
                break;
            case CreatureState.Attack:
                //_animator.SetInteger(_animParameter, );
                break;
            case CreatureState.Dead:
                //_animator.SetInteger(_animParameter, );
                break;
            /*
            */
            case CreatureState.None:
                break;
        }
    }

    public void Init()
    {
        // 카메라 넣어주기
        //_camera = GameManager.Camera._cmCam;
        // 시네머신 브레인 적용되어있는 메인카메라
        _camera = Camera.main.gameObject;

        // 상태 관련 변수
        _moveSpeed = 5.0f;
        _rotationSpeed = 10f;
        _creatureState = CreatureState.Idle;
        _walkOrRun = true;
        
        // 애니메이터
        _animator = GetComponent<Animator>();
        _animParameter = Parameter.State.ToString();
    }

    protected void Idle()
    {
        // 대기 중 이동
        if(GameManager.Ui._joyStickController._joystickState == JoystickState.InputTrue)
        {
            _creatureState = CreatureState.Move;
        }
    }

    protected void Move()
    {
        // 이동 중 대기
        if (GameManager.Ui._joyStickController._joystickState == JoystickState.InputFalse)
        {
            _creatureState = CreatureState.Idle;
        }
        _inputDir = GameManager.Ui._joyStickController.inputDirection;
        //Debug.Log("플레이어 : " + _inputDir);
        //Debug.Log("_inputDir.magnitude : " + _inputDir.magnitude);
        bool isMove = _inputDir.magnitude != 0;
        //if (GameManager.Ui._joyStickController._joystickState == JoystickState.InputTrue)
        if(isMove)
        {
            // 걷는지 뛰는지 lever 길이로 상태 판별
            if(_inputDir.magnitude < 0.5f)
            {
                // Walk
                //Debug.Log("walk");
                _walkOrRun = true;
            }
            else
            {
                // Run
                //Debug.Log("run");
                _walkOrRun = false;
            }

            // 이동
            float x = _inputDir.x;
            float y = _inputDir.y;
            // 월드 기준 이동
            //transform.position += _tempVector;
            // 카메라를 바라보는 방향으로 이동
            _tempVector = new Vector3(x, 0, y);
            _tempVector = _tempVector * Time.deltaTime * _moveSpeed;
            transform.Translate(_tempVector, _camera.transform);
            // 회전
            _tempDir = new Vector3(x, 0, y);
            // 카메라 기준 정면
            _tempDir = Quaternion.Euler(0, _camerAngle, 0) * _tempDir;
            _tempDir = Vector3.RotateTowards(transform.forward, _tempDir, Time.deltaTime * _rotationSpeed, 0);
            transform.rotation = Quaternion.LookRotation(_tempDir.normalized);
        }
    } // end Move()

    // 카메라가 바라보는 방향 구하는 함수(월드 기준)
    public void GetCameraAngle()
    {
        _faceDirection = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z);
        _camerAngle = Vector3.SignedAngle(Vector3.forward, _faceDirection, Vector3.up);
        //Debug.Log("cameraAngle : " + _camerAngle);
    }
}
