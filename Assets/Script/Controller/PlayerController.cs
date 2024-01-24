using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : MonoBehaviour
{
    // JoystickController ���� �޾ƿ� direction ��
    protected Vector2 _inputDir;
    protected Vector3 _tempVector;
    protected Vector3 _tempDir;

    // ī�޶� ������Ʈ
    public GameObject _camera;
    Vector3 _faceDirection;
    // ī�޶� ����
    float _camerAngle;

    // ĳ���� ����
    public CreatureState _creatureState;

    public float _moveSpeed;
    public float _rotationSpeed;
    //public float _autoMoveSpeed;
    public bool _walkOrRun;         // ������ true �ٸ� false

    // �ִϸ�����
    private Animator _animator;

    void Start()
    {
        Init();
    }

    // ĳ���� ���¿� ���� �Լ� ���� & �ִϸ��̼� ����
    void Update()
    {
        // �ǽð����� ����Ǵ� ī�޶� �ޱ� ���ϱ�
        GetCameraAngle();

        //Debug.Log("camerAngle : " + camerAngle);
        switch (_creatureState)
        {
            case CreatureState.Idle:
                Idle();
                _animator.SetInteger("State" ,0);
                break;
            case CreatureState.Move:
                Move();
                // �ȱ�� ���� int, �ٱ�� ���� int�� animation ����
                int tempAniInt = _walkOrRun ? 2 : 3;
                //Debug.Log("walk or run : " + tempAniInt);
                _animator.SetInteger("State" , tempAniInt);
                break;
            case CreatureState.Attack:
                //_animator.SetInteger("State", );
                break;
            case CreatureState.Dead:
                //_animator.SetInteger("State", );
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
        // ��� �� �̵�
        if (GameManager.Ui._joyStickController._joystickState == JoystickState.InputTrue)
        {
            _creatureState = CreatureState.Move;
        }
    }

    protected void Move()
    {
        // �̵� �� ���
        if (GameManager.Ui._joyStickController._joystickState == JoystickState.InputFalse)
        {
            _creatureState = CreatureState.Idle;
        }
        _inputDir = GameManager.Ui._joyStickController.inputDirection;
        //Debug.Log("�÷��̾� : " + _inputDir);
        //Debug.Log("_inputDir.magnitude : " + _inputDir.magnitude);
        bool isMove = _inputDir.magnitude != 0;
        //if (GameManager.Ui._joyStickController._joystickState == JoystickState.InputTrue)
        if(isMove)
        {
            // �ȴ��� �ٴ��� lever ���̷� ���� �Ǻ�
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

            // �̵�
            float x = _inputDir.x;
            float y = _inputDir.y;
            _tempVector = new Vector3(x, 0, y);
            _tempVector = _tempVector * Time.deltaTime * _moveSpeed;
            // ���� ���� �̵�
            //transform.position += _tempVector;
            // ī�޶� �ٶ󺸴� �������� �̵�
            transform.Translate(_tempVector, _camera.transform);
            // ȸ��
            _tempDir = new Vector3(x, 0, y);
            // ī�޶� ���� ����
            _tempDir = Quaternion.Euler(0, _camerAngle, 0) * _tempDir;
            _tempDir = Vector3.RotateTowards(transform.forward, _tempDir, Time.deltaTime * _rotationSpeed, 0);
            transform.rotation = Quaternion.LookRotation(_tempDir.normalized);
        }
    } // end Move()

    // ī�޶� �ٶ󺸴� ���� ���ϴ� �Լ�(���� ����)
    public void GetCameraAngle()
    {
        _faceDirection = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z);
        _camerAngle = Vector3.SignedAngle(Vector3.forward, _faceDirection, Vector3.up);
    }
}
