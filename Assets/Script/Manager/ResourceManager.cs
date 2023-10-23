using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    // ����Ʈ ����
    private List<Sprite> _Image;

    // ���� �Ŵ������� Init
    public void Init()
    {
        // ����Ʈ new
        _Image = new List<Sprite>();
        
        // Resources �������� ���ҽ��� �����ͼ� �迭�� ����
        Sprite[] image = Resources.LoadAll<Sprite>("Resource/Image");

        // �迭�� ����Ʈ��
        ListAdd(_Image, image);
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
        foreach(Sprite one in _Image)
        {
            if(one.name.Equals(imageName))
            {
                return one;
            }
        }
        // ������ null return
        return null;
    }
}
