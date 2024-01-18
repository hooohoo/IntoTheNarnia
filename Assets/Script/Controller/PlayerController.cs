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
        switch(_creatureState)
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
        if (isMove)
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
            // ���̳� ������ �ε����� ������ x 
            if(CheckHitSturcture(_tempVector))
            {
               //_tempVector = Vector3.zero;
            }
            transform.position += _tempVector;
            // ȸ��
            _tempDir = new Vector3(x, 0, y);
            float thisPosX = transform.position.x;
            float thisPosY = transform.position.y;
            Vector3 camPos = new Vector3(_camera.transform.position.x, 0, _camera.transform.position.y);
            //_tempDir = new Vector3( _camera.transform.position.x - thisPosX - x, 0, _camera.transform.position.y - thisPosY - y);
            _tempDir = Vector3.RotateTowards(transform.forward, _tempDir, Time.deltaTime * _rotationSpeed, 0);
            transform.rotation = Quaternion.LookRotation(_tempDir.normalized);
        }
    } // end Move()

    // �������� �ε����� �� üũ�ϴ� �Լ�
    bool CheckHitSturcture(Vector3 movement)
    {
        // �����ӿ� ���� ���� ���͸� ���� ���ͷ� ��ȯ���ش�.
        movement = transform.TransformDirection(movement);
        // scope�� ray �浹�� Ȯ���� ������ ������ �� �ִ�.
        float scope = 1f;

        // �÷��̾��� �Ӹ�, ����, �� �� 3�������� ray�� ���.
        List<Vector3> rayPositions = new List<Vector3>();
        rayPositions.Add(transform.position + Vector3.up * 0.1f);
        rayPositions.Add(transform.position + Vector3.up * transform.GetComponent<CapsuleCollider>().height * 0.5f);
        rayPositions.Add(transform.position + Vector3.up * transform.GetComponent<CapsuleCollider>().height);

        // ������� ���� ray�� ȭ�鿡 �׸���.
        foreach(Vector3 pos in rayPositions)
        {
            Debug.DrawRay(pos, movement * scope, Color.red);
        }

        // ray�� ���� �浹�� Ȯ���Ѵ�.
        foreach(Vector3 pos in rayPositions)
        {
            if(Physics.Raycast(pos, movement, out RaycastHit hit, scope))
            {
                if(hit.collider.CompareTag("Structure"))
                {
                    //Debug.Log("hit : " + hit.transform.name);
                    return true;
                }
                //Debug.Log("�浹 ����");
            }
        }
        //Debug.Log("walking");
        return false;
    }
}
