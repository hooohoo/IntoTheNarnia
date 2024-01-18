using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : MonoBehaviour
{
    // JoystickController 에서 받아온 direction 값
    protected Vector2 _inputDir;
    protected Vector3 _tempVector;
    protected Vector3 _tempDir;

    // 카메라 오브젝트
    public GameObject _camera;

    // 캐릭터 상태
    public CreatureState _creatureState;

    public float _moveSpeed;
    public float _rotationSpeed;
    //public float _autoMoveSpeed;
    public bool _walkOrRun;         // 걸으면 true 뛰면 false

    // 애니메이터
    private Animator _animator;

    void Start()
    {
        Init();
    }

    // 캐릭터 상태에 따라 함수 실행 & 애니메이션 실행
    void Update()
    {
        switch(_creatureState)
        {
            case CreatureState.Idle:
                Idle();
                _animator.SetInteger("State" ,0);
                break;
            case CreatureState.Move:
                Move();
                // 걷기면 앞의 int, 뛰기면 뒤의 int로 animation 설정
                int tempAniInt = _walkOrRun ? 2 : 3;
                //Debug.Log("walk or run : " + tempAniInt);
                _animator.SetInteger("State" , tempAniInt);
                break;
            case CreatureState.Attack:
                //_animator.SetInteger("", );
                break;
            case CreatureState.Dead:
                //_animator.SetInteger("", );
                break;
            /*
            */
            case CreatureState.None:
                break;
        }
    }

    public void Init()
    {
        _moveSpeed = 5.0f;
        _rotationSpeed = 10f;
        _creatureState = CreatureState.Idle;
        _walkOrRun = true;
        _animator = GetComponent<Animator>();
    }

    protected void Idle()
    {
        // 대기 중 이동
        if (GameManager.Ui._joyStickController._joystickState == JoystickState.InputTrue)
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
        if (isMove)
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
            _tempVector = new Vector3(x, 0, y);
            _tempVector = _tempVector * Time.deltaTime * _moveSpeed;
            // 벽이나 가구에 부딪히면 움직임 x 
            if(CheckHitSturcture(_tempVector))
            {
               //_tempVector = Vector3.zero;
            }
            transform.position += _tempVector;
            // 회전
            _tempDir = new Vector3(x, 0, y);
            float thisPosX = transform.position.x;
            float thisPosY = transform.position.y;
            Vector3 camPos = new Vector3(_camera.transform.position.x, 0, _camera.transform.position.y);
            //_tempDir = new Vector3( _camera.transform.position.x - thisPosX - x, 0, _camera.transform.position.y - thisPosY - y);
            _tempDir = Vector3.RotateTowards(transform.forward, _tempDir, Time.deltaTime * _rotationSpeed, 0);
            transform.rotation = Quaternion.LookRotation(_tempDir.normalized);
        }
    } // end Move()

    // 구조물에 부딪혔을 때 체크하는 함수
    bool CheckHitSturcture(Vector3 movement)
    {
        // 움직임에 대한 로컬 벡터를 월드 벡터로 변환해준다.
        movement = transform.TransformDirection(movement);
        // scope로 ray 충돌을 확인할 범위를 지정할 수 있다.
        float scope = 1f;

        // 플레이어의 머리, 가슴, 발 총 3군데에서 ray를 쏜다.
        List<Vector3> rayPositions = new List<Vector3>();
        rayPositions.Add(transform.position + Vector3.up * 0.1f);
        rayPositions.Add(transform.position + Vector3.up * transform.GetComponent<CapsuleCollider>().height * 0.5f);
        rayPositions.Add(transform.position + Vector3.up * transform.GetComponent<CapsuleCollider>().height);

        // 디버깅을 위해 ray를 화면에 그린다.
        foreach(Vector3 pos in rayPositions)
        {
            Debug.DrawRay(pos, movement * scope, Color.red);
        }

        // ray와 벽의 충돌을 확인한다.
        foreach(Vector3 pos in rayPositions)
        {
            if(Physics.Raycast(pos, movement, out RaycastHit hit, scope))
            {
                if(hit.collider.CompareTag("Structure"))
                {
                    //Debug.Log("hit : " + hit.transform.name);
                    return true;
                }
                //Debug.Log("충돌 안함");
            }
        }
        //Debug.Log("walking");
        return false;
    }
}
