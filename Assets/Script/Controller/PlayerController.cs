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
    private Coroutine _coJump;

    public float _moveSpeed;
    public float _rotationSpeed;
    public bool _walkOrRun;         // ������ true �ٸ� false
    public bool _doJump;

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

        // ���� �׽�Ʈ�� �ӽ��ڵ�
        if(Input.GetKeyDown(KeyCode.J))
        {
            _doJump = true;
        }

        //Debug.Log("camerAngle : " + camerAngle);
        switch (_creatureState)
        {
            case CreatureState.Idle:
                Idle();
                _animator.SetInteger("State" ,0);
                // Idle�� ��Ȳ������ ���� �����ϱ� ������ üũ
                CheckJump();
                break;
            case CreatureState.Move:
                Move();
                // �ȱ�� ���� int ��, �ٱ�� ���� int ������ animation ����
                int tempAniInt = _walkOrRun ? 2 : 3;
                _animator.SetInteger("State" , tempAniInt);
                // ���������� üũ�ϰ� �ִϸ����� �Ķ���� ����
                CheckJump();
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
        _doJump = false;
        _animator = GetComponent<Animator>();
    }

    protected void Idle()
    {
        // ��� �� �̵�
        if(GameManager.Ui._joyStickController._joystickState == JoystickState.InputTrue)
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
            // �������̸� ���� �߰�
            _tempVector = _doJump ? new Vector3(x, 5, y) : new Vector3(x, 0, y);
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

    // �����ϴ� �Լ�
    private void Jump()
    {
        // �ڷ�ƾ null �̸�
        if(_coJump == null)
        {
            // ��ŸƮ �ڷ�ƾ
            _coJump = StartCoroutine(JumpCoroutine());
        }
    }

    // ���� �ڷ�ƾ
    private IEnumerator JumpCoroutine()
    {
        yield return new WaitForSeconds(1f);
        if(GameManager.Ui._joyStickController._joystickState == JoystickState.InputFalse)
        {
            _creatureState = CreatureState.Idle;
        }
        if(GameManager.Ui._joyStickController._joystickState == JoystickState.InputTrue)
        {
            _creatureState = CreatureState.Move;
        }
        // ���� ��
        _doJump = false;
        // �ڷ�ƾ �ٽ� null
        _coJump = null;
    }

    // ���������� üũ�ϴ� �Լ�
    private void CheckJump()
    {
        // �������̸�
        if(_doJump)
        {
            // ����
            Jump();
            // ���������� ������ �ִϸ����� �Լ��� ��
            _animator.SetInteger("State", 4);
        }
    }
}
