using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : MonoBehaviour
{
    protected Vector2 _inputDir;
    protected Vector3 _tempVector;
    protected Vector3 _tempDir;

    public CreatureState _creatureState;

    public float _moveSpeed;
    public float _rotationSpeed;
    public float _rollSpeed;
    public float _autoMoveSpeed;

    public Animator _animator;

    void Start()
    {
        Init();
    }

    // ĳ���� ���¿� ���� �Լ� ���� & �ִϸ��̼� ����
    void Update()
    {
        switch (_creatureState)
        {
            case CreatureState.Idle:
                Idle();
                //_animator.SetInteger();
                break;
            case CreatureState.Move:
                Move();
                break;
            case CreatureState.Attack:
                //_animator.SetInteger();
                break;
            case CreatureState.Dead:
                //_animator.SetInteger();
                break;
            /*
            */
            case CreatureState.None:
                break;
        }
    }

    public void Init()
    {
        _moveSpeed = 7.0f;
        _rotationSpeed = 10f;
        _rollSpeed = 10.0f;
        _creatureState = CreatureState.Idle;
       // _animator = GetComponent<Animator>();
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
        Debug.Log("_inputDir.magnitude : " + _inputDir.magnitude);
        bool isMove = _inputDir.magnitude != 0;
        //if (GameManager.Ui._joyStickController._joystickState == JoystickState.InputTrue)
        if (isMove)
        {
            // �ȴ��� �ٴ��� lever ���̷� ���� �Ǻ�
            if(_inputDir.magnitude < 2.5f)
            {
                // Walk
                _creatureState = CreatureState.Walk;
            }
            else
            {
                // Run
                _creatureState = CreatureState.Run;
            }

            // �̵�
            float x = _inputDir.x;
            float y = _inputDir.y;
            _tempVector = new Vector3(x, 0, y);
            _tempVector = _tempVector * Time.deltaTime * _moveSpeed;
            transform.position += _tempVector;
            // ȸ��
            _tempDir = new Vector3(x, 0, y);
            _tempDir = Vector3.RotateTowards(transform.forward, _tempDir, Time.deltaTime * _rotationSpeed, 0);
            transform.rotation = Quaternion.LookRotation(_tempDir.normalized);
        }
    } // end Move()
}
