using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �� ����/����, �� ������ ��Ʈ�ѷ�
// �׼ǹ�ư���� ��Ʈ��
public class DoorController : MonoBehaviour
{
    // �� ������ ������Ʈ
    public GameObject _doorHandle;

    // �� �ִϸ�����
    private Animator _doorAnimator;
    // �� ������ �ִϸ�����
    private Animator _doorHandleAnimator;

    // �ִϸ����� �Ķ����
    private string _animParameter;

    // �� ���� or ����
    private bool _openOrNot;

    void Start()
    {
        Init();
    }

    void Update()
    {
        // �ӽ��ڵ�
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ShowDoorLocked();
        }
        if(Input.GetKeyDown(KeyCode.O))
        {
            ShowDoorOpen();
        }
    }

    // �ʱ�ȭ
    private void Init()
    {
        // ����Ʈ�� ������
        _openOrNot = false;
        // �� ������ �ִϸ����� �־��ֱ�
        _doorHandleAnimator = _doorHandle.GetComponent<Animator>();
        // �Ķ���� ���� �־��ֱ�, "State"
        _animParameter = Define.Parameter.State.ToString();
    }

    // ������
    // ��������� ����ִٴ� ǥ��
    public void OpenDoor()
    {
        // �����ִ��� Ȯ��
        if(_openOrNot)
        {
            // ������
            ShowDoorOpen();
        }
        else
        {
            // �����ֽ��ϴ�
            ShowDoorLocked();
        }
    }

    // ����� ���� �ִϸ��̼� �÷���(������)
    public void ShowDoorLocked()
    {
        // ���(State, 1)
        _doorHandleAnimator.SetInteger(_animParameter, 1);
        StartCoroutine("ReturnToIdle");
    }

    // ������ ���� �ִϸ��̼� �÷���(��, ������ ���)
    public void ShowDoorOpen()
    {
        // �÷��̾� ĳ���� + �ó׸ӽ� ������ ������ ������ ���� ����

        // ����(State, 2) : ������
        _doorHandleAnimator.SetInteger(_animParameter, 2);
        StartCoroutine("ReturnToIdle");
        // ����(State, ??) : ��
        //_doorAnimator.SetInteger(_animParameter, );
    }

    // ������ ��� ���� �ٽ� Idle�� ���� ��ȯ�ϱ� ���� �ڷ�ƾ
    private IEnumerator ReturnToIdle()
    {
        // 1�� ��ٸ���
        yield return new WaitForSeconds(1f);
        // Idle ���·� ��ȯ
        _doorHandleAnimator.SetInteger(_animParameter, 0);
    }

    // �÷��̾�� �浹üũ
    private void OnTriggerEnter(Collider collider)
    {
        // �÷��̾� �ݶ��̴��� �浹���� ��
        if(collider.CompareTag(Define.TagName.Player.ToString()))
        {
            // ������
            string msg = _openOrNot ? "�����ֽ��ϴ�." : "�� ����.";
            Debug.Log(transform.name + " : " + msg);
            // �׼ǹ�ư �±׿� Structure �ֱ�
            GameManager.Ui._actionButtonController._verifyTag = Define.TagName.Structure;
        }
    }
}
