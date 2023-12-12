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

    void Start()
    {
        Init();
    }

    void Update()
    {
        switch (_creatureState)
        {
            /*
            case CreatureState.Idle:
                Idle();
                break;
            */
            case CreatureState.Move:
                Move();
                break;
            /*
            case CreatureState.AutoMove:
                AutoMove();
                break;
            case CreatureState.Attack:
                Attack();
                break;
            case CreatureState.Dead:
                Dead();
                break;
            case CreatureState.Rolling:
                Roll();
                break;
            case CreatureState.Skill:
                Skill1();
                break;
            case CreatureState.Skill2:
                Skill2();
                break;
            case CreatureState.Skill3:
                Skill3();
                break;
            */
            case CreatureState.None:
                break;
        }
    }

    public void Init()
    {
        _moveSpeed = 10.0f;
        _rotationSpeed = 10f;
        _rollSpeed = 10.0f;
    }

    protected void Move()
    {
        /*
        // 이동 중 대기
        if (GameManager.Ui._joyStickController._joystickState == JoystickState.InputFalse)
        {
            _creatureState = CreatureState.Idle;
        }
        */
        _inputDir = GameManager.Ui._joyStickController.inputDirection;
        // Debug.Log("플레이어 : " + _inputDir);
        bool isMove = _inputDir.magnitude != 0;
        //if (GameManager.Ui._joyStickController._joystickState == JoystickState.InputTrue)
        if (isMove)
        {
            // 이동
            float x = _inputDir.x;
            float y = _inputDir.y;
            _tempVector = new Vector3(x, 0, y);
            _tempVector = _tempVector * Time.deltaTime * _moveSpeed;
            transform.position += _tempVector;
            // 회전
            /*
            if (playerStat.Job == Job.Cyborg.ToString() && _isSkill1 == true)
            {
                transform.rotation = Quaternion.LookRotation(_tempDir.normalized);
            }
            else
            */
            {
                _tempDir = new Vector3(x, 0, y);
                _tempDir = Vector3.RotateTowards(transform.forward, _tempDir, Time.deltaTime * _rotationSpeed, 0);
                transform.rotation = Quaternion.LookRotation(_tempDir.normalized);
            }
        }
    }
}
