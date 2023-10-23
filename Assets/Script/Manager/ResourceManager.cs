using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    // 리스트 생성
    private List<Sprite> _Image;

    // 게임 매니저에서 Init
    public void Init()
    {
        // 리스트 new
        _Image = new List<Sprite>();
        
        // Resources 폴더에서 리소스들 가져와서 배열로 만듦
        Sprite[] image = Resources.LoadAll<Sprite>("Resource/Image");

        // 배열을 리스트로
        ListAdd(_Image, image);
    }

    // Resources 폴더에서 가져온 배열 리스트에 넣는 함수1
    public void ListAdd(List<GameObject> _list, GameObject[] arr)
    {
        foreach(GameObject one in arr)
        {
            _list.Add(one);
        }
    }

    // Resources 폴더에서 가져온 배열 리스트에 넣는 함수2 - Sprite 인 경우
    public void ListAdd(List<Sprite> _list, Sprite[] arr)
    {
        foreach(Sprite one in arr)
        {
            _list.Add(one);
        }
    }

    // 이름으로 검색해서 이미지 반환
    public Sprite GetImage(string imageName)
    {
        foreach(Sprite one in _Image)
        {
            if(one.name.Equals(imageName))
            {
                return one;
            }
        }
        // 없으면 null return
        return null;
    }
}
