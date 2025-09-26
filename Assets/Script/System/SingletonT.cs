using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonT<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T m_Instance;
    private static object m_lock = new object();
    public static T Instance
    {
        get
        {
            lock (m_lock)
            {
                if (m_Instance == null)
                {
                    m_Instance = FindObjectOfType<T>();
                    if (m_Instance == null)
                    {
                        string componentName = typeof(T).ToString();
                        GameObject findObject = GameObject.Find(componentName);
                        if (findObject == null)
                            findObject = new GameObject(componentName);
                        m_Instance = findObject.AddComponent<T>();
                    }
                }
                return m_Instance;
            }
        }
    }
}
