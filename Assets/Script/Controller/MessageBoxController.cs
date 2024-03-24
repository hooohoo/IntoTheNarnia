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

    // 남은 대사 모음
    private Queue<string> _lineListQueue = new Queue<string>();

    // 코루틴 변수
    private Coroutine _coTalking;

    void Start(){}

    void Update(){}

    // 메세지 박스 한번에 작동시키는 함수(이름, 대사[])
    public void SetMessageBox(string nameString, string[] lines)
    {
        // UI MessageBox 활성화
        GameManager.Ui._messageBox.SetActive(true);
        // 이름 넣기
        SetNameBoxText(nameString);
        // 대사 넣기
        SetContentsBoxText(lines);
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
        // 대사 리스트 초기화
        _lineListQueue.Clear();
        
        // 한 문장씩 큐에 넣기
        foreach(string one in lines)
        {
            _lineListQueue.Enqueue(one);
        }

        PlayNextLine();
    }

    // 한 음절씩 끊어서 말하는 코루틴
    private IEnumerator SpeekOneByOne(char[] lineSyllableArr)
    {
        // 이전 대사 초기화
        _contentsText.text = string.Empty;
        // 한 문장을 한 음절로 쪼개서 반복
        foreach(char one in lineSyllableArr)
        {
            // 텍스트 박스에 하나씩 더해주기
            _contentsText.text += one;
            // 0.1초마다 글자넣기
            yield return new WaitForSeconds(0.01f);
        }
    }

    // 다음 대화 버튼
    public void PlayNextLine()
    {
        // 코루틴 실행중이면
        if(_coTalking != null)
        {
            // 종료
            StopCoroutine(_coTalking);
        }

        // 남은 대사 수가 0보다 클 때
        if(_lineListQueue.Count > 0)
        {
            // 코루틴 시작, char[] 타입으로 넣어줌
            _coTalking = StartCoroutine(SpeekOneByOne(_lineListQueue.Dequeue().ToCharArray()));
        }
        else
        {
            // 남은 대사 없으면 종료
            GameManager.Ui._messageBox.SetActive(false);
        }
    }
}
