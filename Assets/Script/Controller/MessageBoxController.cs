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
    // 다음 대화 버튼
    public Button _nextButton;

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
    // List 말고 배열 쓰는 이유는 대사 뭉텅이를 Split() 함수로 쪼개서 배열로 리턴되기 때문
    public void SetContentsBoxText(string[] lines)
    {
        // 이전 대사 초기화
        _contentsText.text = string.Empty;
        // 한 문장씩 코루틴에 넣어주기
        foreach(string one in lines)
        {
            // 코루틴 시작, char 배열로 넣어줌
            StartCoroutine(SpeekOneByOne(one.ToCharArray()));
        }
    }

    // 한 음절씩 끊어서 말하는 코루틴
    private IEnumerator SpeekOneByOne(char[] lineSyllableArr)
    {
        // 한 문장을 한 음절로 쪼개서 반복
        foreach(char one in lineSyllableArr)
        {
            // 텍스트 박스에 하나씩 더해주기
            _contentsText.text += one;
            // 0.1초마다 글자넣기
            yield return new WaitForSeconds(0.1f);
        }
        StopCoroutine(SpeekOneByOne(lineSyllableArr));
    }

    // 다음 대화 버튼
    public void PlayNextLine()
    {
        //
    }
}
