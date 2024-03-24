using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ��ȭâ ��Ʈ�ѷ�
public class MessageBoxController : MonoBehaviour
{
    // �̸� ĭ �ؽ�Ʈ
    public Text _nameText;
    // ��� ĭ �ؽ�Ʈ
    public Text _contentsText;
    // ���� ��ȭ ��ư
    public Button _nextButton;

    // ���� ��� ����
    private Queue<string> _lineListQueue = new Queue<string>();

    // �ڷ�ƾ ����
    private Coroutine _coTalking;

    void Start(){}

    void Update(){}

    // �޼��� �ڽ� �ѹ��� �۵���Ű�� �Լ�(�̸�, ���[])
    public void SetMessageBox(string nameString, string[] lines)
    {
        // UI MessageBox Ȱ��ȭ
        GameManager.Ui._messageBox.SetActive(true);
        // �̸� �ֱ�
        SetNameBoxText(nameString);
        // ��� �ֱ�
        SetContentsBoxText(lines);
    }

    // �̸� ĭ�� �ؽ�Ʈ �ִ� �Լ�
    public void SetNameBoxText(string newText)
    {
        // �Ű������� �޾ƿ� string �ֱ�
        _nameText.text = newText;
    }

    // ��� ĭ �ؽ�Ʈ �ִ� �Լ�
    // List ���� �迭 ���� ������ ��� �����̸� Split() �Լ��� �ɰ��� �迭�� ���ϵǱ� ����
    public void SetContentsBoxText(string[] lines)
    {
        // ��� ����Ʈ �ʱ�ȭ
        _lineListQueue.Clear();
        
        // �� ���徿 ť�� �ֱ�
        foreach(string one in lines)
        {
            _lineListQueue.Enqueue(one);
        }

        PlayNextLine();
    }

    // �� ������ ��� ���ϴ� �ڷ�ƾ
    private IEnumerator SpeekOneByOne(char[] lineSyllableArr)
    {
        // ���� ��� �ʱ�ȭ
        _contentsText.text = string.Empty;
        // �� ������ �� ������ �ɰ��� �ݺ�
        foreach(char one in lineSyllableArr)
        {
            // �ؽ�Ʈ �ڽ��� �ϳ��� �����ֱ�
            _contentsText.text += one;
            // 0.1�ʸ��� ���ڳֱ�
            yield return new WaitForSeconds(0.01f);
        }
    }

    // ���� ��ȭ ��ư
    public void PlayNextLine()
    {
        // �ڷ�ƾ �������̸�
        if(_coTalking != null)
        {
            // ����
            StopCoroutine(_coTalking);
        }

        // ���� ��� ���� 0���� Ŭ ��
        if(_lineListQueue.Count > 0)
        {
            // �ڷ�ƾ ����, char[] Ÿ������ �־���
            _coTalking = StartCoroutine(SpeekOneByOne(_lineListQueue.Dequeue().ToCharArray()));
        }
        else
        {
            // ���� ��� ������ ����
            GameManager.Ui._messageBox.SetActive(false);
        }
    }
}
