using System.Collections;
using System.Collections.Generic;
using THNetworkLibrary;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Dictionary<System.Type, IManager> m_managerCollection;

    #region Singleton Management

    public static GameManager instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeOnLoad()
    {
        if (instance != null)
        {
            return;
        }

        GameObject nObj = new GameObject("GAME_MANAGER");
        DontDestroyOnLoad(nObj);
        instance = nObj.AddComponent<GameManager>();

        instance.StartCoroutine(instance.Initialize());
    }

    #endregion

    /// <summary>
    /// Add any needed debug tools for ease of access.
    /// </summary>
    private void AddDebugTools()
    {
        instance.gameObject.AddComponent<DebugTool_InputKeys>();
    }

    /// <summary>
    /// Called after Singleton initializes for the first time.
    /// </summary>
    private IEnumerator Initialize()
    {
        AddDebugTools();

        Log.RegisterMsgEvent((str) => 
        {
            Debug.Log(str);
        });

        m_managerCollection = new Dictionary<System.Type, IManager>();

        //Add Managers to GameManager.
        AddManager<AssetManager>();
        AddManager<InputManager>();
        AddManager<NetworkManager>();

        //Handle Initialization of Managers
        ManagerInitializeErrorCode errorCode = ManagerInitializeErrorCode.NONE;
        System.Type managerType = typeof(IManager);
        foreach (var manager in m_managerCollection)
        {
            errorCode = manager.Value.Initialize();
            if (errorCode != ManagerInitializeErrorCode.NONE)
            {
                managerType = manager.Value.GetType();
                break;
            }
        }
        if (errorCode != ManagerInitializeErrorCode.NONE)
        {
            Debug.Log($"ERROR: {errorCode} - {managerType.ToString()}");

            //Close application
            //Application.Quit();
        }

        yield return null;
    }

    /// <summary>
    /// Add a manager to the Manager collection pool maintained by the Singleton.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private void AddManager<T>() where T : IManager, new()
    {
        if (!m_managerCollection.ContainsKey(typeof(T)))
        {
            T newManager = new T();
            m_managerCollection.Add(typeof(T), newManager);
        }
        else
        {
            Debug.Log($"ERROR: GameManager contains existing IManager of type {typeof(T)}");
        }
    }

    /// <summary>
    /// Poll for a handled Manager instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetManager<T>() where T : IManager
    {
        if (instance.m_managerCollection.ContainsKey(typeof(T)))
        {
            return (T)instance.m_managerCollection[typeof(T)];
        }

        return null;
    }

    private void Update()
    {
        foreach (var manager in m_managerCollection)
        {
            manager.Value.Update(Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        foreach (var manager in m_managerCollection)
        {
            manager.Value.FixedUpdate(Time.fixedDeltaTime);
        }
    }

    private void OnDestroy()
    {
        //Clear out managers
        foreach (var manager in m_managerCollection)
        {
            manager.Value.OnDestroy();
        }

        m_managerCollection.Clear();
        m_managerCollection = null;
    }
}
