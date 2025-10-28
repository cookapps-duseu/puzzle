using System;
using System.Threading;
using UnityEngine;

namespace CookApps.Utility
{
    public abstract class SwallowDelayActionBase
    {
        protected float delayDuration;
        protected float currDuration;
        protected bool isFastForward;
        protected Awaitable currentTask;

        private CancellationTokenSource cts;

        public SwallowDelayActionBase(float duration)
        {
            delayDuration = duration;
            cts = new ();
        }

        ~SwallowDelayActionBase()
        {
            cts.Dispose();
        }

        public void SetDuration(float duration)
        {
            delayDuration = duration;
        }

        private async Awaitable Swallow(CancellationToken token)
        {
            isFastForward = false;
            currDuration = delayDuration;
            float prevRealTime = Time.realtimeSinceStartup;
            while (currDuration > 0 && !isFastForward)
            {
                await Awaitable.NextFrameAsync(token);
                var deltaTime = Time.realtimeSinceStartup - prevRealTime;
                prevRealTime = Time.realtimeSinceStartup;
                currDuration -= deltaTime;
            }
        }

        protected void DelayedInvoke()
        {
            if (currentTask.IsCompleted)
            {
                currentTask = Swallow(cts.Token);
                currentTask.ContinueWith(CallEventAction);
            }
        }

        public void FastForward()
        {
            isFastForward = true;
        }

        protected void InvokeImmediately()
        {
            cts.Cancel();
            cts = new ();
            CallEventAction();
        }

        protected abstract void CallEventAction();
    }

    public class SwallowDelayAction : SwallowDelayActionBase
    {
        private event Action onEventAction;

        public SwallowDelayAction(float duration) : base(duration)
        {
        }

        ~SwallowDelayAction()
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

        public new void DelayedInvoke()
        {
            base.DelayedInvoke();
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

    public class SwallowDelayAction<T> : SwallowDelayActionBase
    {
        private event Action<T> onEventAction;
        private T data;

        public SwallowDelayAction(float duration) : base(duration)
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

        public void DelayedInvoke(T data)
        {
            this.data = data;
            DelayedInvoke();
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

    public class SwallowDelayAction<T1, T2> : SwallowDelayActionBase
    {
        private event Action<T1, T2> onEventAction;
        private T1 data1;
        private T2 data2;

        public SwallowDelayAction(float duration) : base(duration)
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

        public void DelayedInvoke(T1 data1, T2 data2)
        {
            this.data1 = data1;
            this.data2 = data2;
            DelayedInvoke();
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
}
