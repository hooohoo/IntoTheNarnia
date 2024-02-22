using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

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

    // ��� ��ġ ���� or ���
    public bool _openOrLocked;
    // �� ���� or ����
    public bool _openOrClose;

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
        // ����Ʈ�� ���, ���� ����
        _openOrLocked = false;
        _openOrClose = false;
        // �� �ִϸ����� �־��ֱ�
        _doorAnimator = GetComponent<Animator>();
        // �� ������ �ִϸ����� �־��ֱ�
        _doorHandleAnimator = _doorHandle.GetComponent<Animator>();
        // �Ķ���� ���� �־��ֱ�, "State"
        _animParameter = Parameter.State.ToString();
    }

    // ������, �ݱ�
    // ��������� ����ִٴ� ǥ��
    public void OpenAndCloseDoor()
    {
        // �̹� ���� �����̸�
        if(_openOrClose)
        {
            // �� �ݱ�
            CloseDoor();
            _openOrClose = false;
            return;
        }
        // ����ִ��� Ȯ��
        if(_openOrLocked)
        {
            // ������
            ShowDoorOpen();
            _openOrClose = true;
        }
        else
        {
            // �����ֽ��ϴ�
            ShowDoorLocked();
        }
    }

    // �� �ݴ� �ִϸ��̼�
    public void CloseDoor()
    {
        // open -> Idle
        _doorAnimator.SetInteger(_animParameter, 0);
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
        // ����(State, 1) : ��
        _doorAnimator.SetInteger(_animParameter, 1);
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
    private void OnTriggerStay(Collider collider)
    {
        // �÷��̾� �ݶ��̴��� �浹���� ��
        if(collider.CompareTag(TagName.Player.ToString()))
        {
            // ������
            //string msg = _openOrNot ? "�����ֽ��ϴ�." : "�� ����.";
            //Debug.Log(transform.name + " : " + msg);
            // �׼� ��ư �±׿� Structure �ֱ�
            GameManager.Ui._actionButtonController._verifyTag = TagName.Structure;
            // �׼� ��ư ��Ʈ�ѷ����� ������ �� �ֵ��� ���� �� �Ѱ��ֱ�
            GameManager.Ui._actionButtonController._thisDoor = this;
        }
    }

    // �浹 ������ ��
    private void OnTriggerExit(Collider collider)
    {
        // �±� ����ֱ�
        GameManager.Ui._actionButtonController._verifyTag = TagName.None;
        GameManager.Ui._actionButtonController._thisDoor = null;
    }
}
