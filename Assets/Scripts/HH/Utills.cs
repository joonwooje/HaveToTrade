using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Utills : MonoBehaviour
{
    
}
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour   // �̱����� �����ų Ŭ������
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(T)) as T;
                if (instance == null)
                    Debug.LogError("Not Activated : " + typeof(T));
            }
            return instance;
        }
    }
}
public class Data<T>//������ ����, Value�� ���Ҷ����� �޼ҵ� ȣ��
{
    private T v;//�̰� ���Ҷ����� �޼ҵ� ȣ��
    public T Value
    {
        get { return v; }//����� �޾ƿ�
        set//����� ���Ҷ�����
        {
            v = value;
            onChange?.Invoke(value);//�����Ϳ� ������ �Լ��� ȣ��
        }
    }
    public Action<T> onChange;//onChange�� ������ �ȿ� �޼ҵ� �߰� �� ����(����)
}

