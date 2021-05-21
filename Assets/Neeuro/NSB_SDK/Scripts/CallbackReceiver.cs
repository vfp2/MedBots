using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// For internal use\n
/// To process threaded calls to functions that should only be accessed
/// by Unity's main or UI thread\n
/// </summary>
abstract public class CallbackReceiver : MonoBehaviour
{
    int started_by = -1;
    protected void Start()
    {
        started_by = Thread.CurrentThread.ManagedThreadId;
    }

    protected bool SeparateThread
    {
        get
        {
            return started_by != Thread.CurrentThread.ManagedThreadId;
        }
    }

    class CallbackEvent
    {
        public System.Delegate method;
        public object[] args;
    }
    List<CallbackEvent> events = new List<CallbackEvent>();
    object eventlock = new object();
    public void QueueInvoke(System.Delegate method, params object[] args)
    {
        lock (eventlock)
        {
            events.Add(new CallbackEvent { method = method, args = args });
        }
    }

    protected void Update()
    {
        if (SeparateThread) return;

        lock (eventlock)
        {
            while (events.Count > 0)
            {
                var to_remove = events[0];
                to_remove.method.DynamicInvoke(to_remove.args);
                events.Remove(to_remove);
            }
        }
    }
}