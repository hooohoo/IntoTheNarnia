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
    public void SetContentsBoxText(string newText)
    {
        // �Ű������� �޾ƿ� string �ֱ�
        _contentsText.text = newText;
    }

    // ���� ��ȭ ��ư
    public void PlayNextLine()
    {
        //
    }
}
