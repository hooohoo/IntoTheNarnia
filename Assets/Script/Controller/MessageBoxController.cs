using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 대화창 컨트롤러
public class MessageBoxController : MonoBehaviour
{
    // 이름 칸 텍스트
    public Text _nameText;
    // 대사 칸 텍스트
    public Text _contentsText;

    void Start()
    {
        //
    }

    void Update()
    {
        //
    }

    // 이름 칸에 텍스트 넣는 함수
    public void SetNameBoxText(string newText)
    {
        // 매개변수로 받아온 string 넣기
        _nameText.text = newText;
    }

    // 대사 칸 텍스트 넣는 함수
    public void SetContentsBoxText(string newText)
    {
        // 매개변수로 받아온 string 넣기
        _contentsText.text = newText;
    }
}
