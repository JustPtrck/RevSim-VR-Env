using System;
using UnityEngine;
using System.Collections.Generic;



/// <summary>
/// Runs the ActionQueue functions on main thread
/// </summary>
public class ActionBus : MonoBehaviour
{

    private static readonly Queue<Action> actionQueue = new Queue<Action>();
    private static ActionBus instance = null;

    public static ActionBus Instance()
    {
        if (instance == null)
        {
            throw new Exception("ActionBus' parent gameObject needs to be on scene, please add it to your scene.");
        }
        return instance;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(this.gameObject); - commented out when adding to an existing don't destroy on load gameObject
        }
    }

    void OnDestroy()
    {
        instance = null;
    }

    public void Update()
    {
        lock (actionQueue)
        {
            while (actionQueue.Count > 0)
            {
                Action action = actionQueue.Dequeue();
                action();
            }
        }
    }

    public void Add(Action action) {
        lock (actionQueue)
        {
            actionQueue.Enqueue(action);
        }
    }

}