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

    void Start()
    {
        //
    }

    void Update()
    {
        //
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
        // ���� ��� �ʱ�ȭ
        _contentsText.text = string.Empty;
        // �� ���徿 �ڷ�ƾ�� �־��ֱ�
        foreach(string one in lines)
        {
            // �ڷ�ƾ ����, char �迭�� �־���
            StartCoroutine(SpeekOneByOne(one.ToCharArray()));
        }
    }

    // �� ������ ��� ���ϴ� �ڷ�ƾ
    private IEnumerator SpeekOneByOne(char[] lineSyllableArr)
    {
        // �� ������ �� ������ �ɰ��� �ݺ�
        foreach(char one in lineSyllableArr)
        {
            // �ؽ�Ʈ �ڽ��� �ϳ��� �����ֱ�
            _contentsText.text += one;
            // 0.1�ʸ��� ���ڳֱ�
            yield return new WaitForSeconds(0.1f);
        }
        StopCoroutine(SpeekOneByOne(lineSyllableArr));
    }

    // ���� ��ȭ ��ư
    public void PlayNextLine()
    {
        //
    }
}
