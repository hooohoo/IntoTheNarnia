using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    // ����Ʈ ����
    private List<GameObject> _player;
    private List<Sprite> _image;

    // ���� �Ŵ������� Init
    public void Init()
    {
        // ����Ʈ new
        _player = new List<GameObject>();
        _image = new List<Sprite>();

        // Resources �������� ���ҽ��� �����ͼ� �迭�� ����
        GameObject[] player = Resources.LoadAll<GameObject>("Prefabs/Character");
        Sprite[] image = Resources.LoadAll<Sprite>("Resource/Image");

        // �迭�� ����Ʈ��
        ListAdd(_player, player);
        ListAdd(_image, image);
    }

    // Resources �������� ������ �迭 ����Ʈ�� �ִ� �Լ�1
    public void ListAdd(List<GameObject> _list, GameObject[] arr)
    {
        foreach(GameObject one in arr)
        {
            _list.Add(one);
        }
    }

    // Resources �������� ������ �迭 ����Ʈ�� �ִ� �Լ�2 - Sprite �� ���
    public void ListAdd(List<Sprite> _list, Sprite[] arr)
    {
        foreach(Sprite one in arr)
        {
            _list.Add(one);
        }
    }

    // �̸����� �˻��ؼ� �̹��� ��ȯ
    public Sprite GetImage(string imageName)
    {
        foreach(Sprite one in _image)
        {
            if(one.name.Equals(imageName))
            {
                return one;
            }
        }
        // ������ null return
        return null;
    }

    // �̸����� �˻��ؼ� �÷��̾� PlayerController Ÿ������ ��ȯ
    public GameObject GetPlayer(string playerName)
    {
        foreach(GameObject one in _player)
        {
            if(one.name.Equals(playerName))
            {
                return one;
            }
        }
        // ������ null
        return null;
    }
}
