using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T : new()
{
    public static T Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = new T();

            return _Instance;
        }
    }

    protected static T _Instance;
}

public abstract class MonoSingletone<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance = null;
    private static object _syncobj = new object();
    private static bool appIsClosing = false;

    public static T Instance {
        get
        {
            if (appIsClosing)
                return null;

            lock(_syncobj)
            {
                if(_instance == null)
                {
                    T[] objs = FindObjectsOfType<T>();

                    if(objs.Length > 0)
                    {
                        _instance = objs[0];
                    }
                    if(objs.Length > 1)
                    {
                        Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
                        for(int i=1; i<objs.Length; i++)
                        {
                            Destroy(objs[i].gameObject);
                        }
                    }
                    if(_instance == null)
                    {
                        string goName = typeof(T).ToString();
                        GameObject go = GameObject.Find(goName);
                        if(go == null)
                        {
                            go = new GameObject(goName);
                        }
                        _instance = go.AddComponent<T>();
                    }
                }

                return _instance;
            }


           
        }
    
    
    }

    protected virtual void OnApplicationQuit()
    {
        appIsClosing = true;
    }


}
