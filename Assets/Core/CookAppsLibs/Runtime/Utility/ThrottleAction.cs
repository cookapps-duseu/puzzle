using System;
using System.Threading;
using CookApps.Utility;
using UnityEngine;

public abstract class ThrottleActionBase
{
    protected readonly float delayDuration;
    protected float currDuration;
    protected bool isFastForward;
    protected Awaitable currentTask;

    private CancellationTokenSource cts;

    public ThrottleActionBase(float duration)
    {
        delayDuration = duration;
        cts = new ();
        CallEventActionLambda = CallEventAction;
    }

    ~ThrottleActionBase()
    {
        // Debug.Log("~ThrottleActionBase");
        cts.Dispose();
    }

    private async Awaitable Throttle(CancellationToken token)
    {
        // Debug.Log("Throttle Start");
        isFastForward = false;
        currDuration = delayDuration;
        float prevRealTime = Time.realtimeSinceStartup;
        while (currDuration > 0 && !isFastForward)
        {
            // Debug.Log($"currDuration: {currDuration}, isFastForward: {isFastForward}");
            await Awaitable.NextFrameAsync(token);
            var deltaTime = Time.realtimeSinceStartup - prevRealTime;
            prevRealTime = Time.realtimeSinceStartup;
            currDuration -= deltaTime;
        }

        // await Awaitable.SwitchToMainThread();
        // Debug.Log($"Throttle End");
    }

    protected void ThrottleInvoke()
    {
        if (currentTask?.IsCompleted ?? true)
        {
            // Debug.Log($"ThrottleInvoke");
            currentTask = Throttle(cts.Token);
            currentTask.ContinueWith(CallEventActionLambda);
        }
        else
        {
            // Debug.Log($"Throttle Delay");
            currDuration = delayDuration;
        }
    }
    
    public void FastForward()
    {
        // Debug.Log($"Throttle FastForward");
        isFastForward = true;
    }

    protected void InvokeImmediately()
    {
        // Debug.Log($"Throttle InvokeImmediately");
        cts.Cancel();
        cts = new ();
        CallEventAction();
    }

    private Action CallEventActionLambda;
    protected abstract void CallEventAction();
}

public class ThrottleAction : ThrottleActionBase
{
    private event Action onEventAction;

    public ThrottleAction(float duration) : base(duration)
    {
    }
    
    ~ThrottleAction()
    {
    }

    public void AddListener(Action listener)
    {
        onEventAction += listener;
    }

    public void RemoveListener(Action listener)
    {
        onEventAction -= listener;
    }

    public void RemoveAllListeners()
    {
        onEventAction = null;
    }

    public new void ThrottleInvoke()
    {
        base.ThrottleInvoke();
    }

    public new void InvokeImmediately()
    {
        base.InvokeImmediately();
    }

    protected override void CallEventAction()
    {
        onEventAction?.Invoke();
    }
}

public class ThrottleAction<T> : ThrottleActionBase
{
    private event Action<T> onEventAction;
    private T data;

    public ThrottleAction(float duration) : base(duration)
    {
    }

    public void AddListener(Action<T> listener)
    {
        onEventAction += listener;
    }

    public void RemoveListener(Action<T> listener)
    {
        onEventAction -= listener;
    }

    public void RemoveAllListeners()
    {
        onEventAction = null;
    }

    public void ThrottleInvoke(T data)
    {
        this.data = data;
        ThrottleInvoke();
    }

    public void InvokeImmediately(T data)
    {
        this.data = data;
        InvokeImmediately();
    }

    protected override void CallEventAction()
    {
        onEventAction?.Invoke(data);
    }
}

public class ThrottleAction<T1, T2> : ThrottleActionBase
{
    private event Action<T1, T2> onEventAction;
    private T1 data1;
    private T2 data2;

    public ThrottleAction(float duration) : base(duration)
    {
    }

    public void AddListener(Action<T1, T2> listener)
    {
        onEventAction += listener;
    }

    public void RemoveListener(Action<T1, T2> listener)
    {
        onEventAction -= listener;
    }

    public void RemoveAllListeners()
    {
        onEventAction = null;
    }

    public void ThrottleInvoke(T1 data1, T2 data2)
    {
        this.data1 = data1;
        this.data2 = data2;
        ThrottleInvoke();
    }

    public void InvokeImmediately(T1 data1, T2 data2)
    {
        this.data1 = data1;
        this.data2 = data2;
        InvokeImmediately();
    }

    protected override void CallEventAction()
    {
        onEventAction?.Invoke(data1, data2);
    }
}
