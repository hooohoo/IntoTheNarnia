using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    // 리스트 생성
    private List<GameObject> _player;
    private List<Sprite> _image;

    // 게임 매니저에서 Init
    public void Init()
    {
        // 리스트 new
        _player = new List<GameObject>();
        _image = new List<Sprite>();

        // Resources 폴더에서 리소스들 가져와서 배열로 만듦
        GameObject[] player = Resources.LoadAll<GameObject>("Prefabs/Character");
        Sprite[] image = Resources.LoadAll<Sprite>("Resource/Image");

        // 배열을 리스트로
        ListAdd(_player, player);
        ListAdd(_image, image);
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
        foreach(Sprite one in _image)
        {
            if(one.name.Equals(imageName))
            {
                return one;
            }
        }
        // 없으면 null return
        return null;
    }

    // 이름으로 검색해서 플레이어 PlayerController 타입으로 반환
    public GameObject GetPlayer(string playerName)
    {
        foreach(GameObject one in _player)
        {
            if(one.name.Equals(playerName))
            {
                return one;
            }
        }
        // 없으면 null
        return null;
    }
}
