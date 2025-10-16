using System;
using System.Threading;
using RabbitDog.UIManagements;
using RabbitDog.WorldTouch;
using UnityEngine;

namespace Template
{
    public class Tutorial_Test : TutorialBase, ISelectableBlocker, ITouchBlocker
    {
        private CancellationTokenSource cts;
        
        public Tutorial_Test()
        {
            Debug.Log("Tutorial_Test Constructed");
        }
        
        ~Tutorial_Test()
        {
            Debug.Log("Tutorial_Test Destructed");
        }
        
        protected override void OnStartTutorial()
        {
            cts = new ();
            SelectableBlockerManager.Instance.AddBlocker(this);
            TouchManager.AddBlocker(this);
            _ = WaitAndEndTutorialAsync(cts.Token);
        }

        private async Awaitable WaitAndEndTutorialAsync(CancellationToken ctsToken)
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(5, ctsToken);
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation if needed
            }
            finally
            {
                SelectableBlockerManager.Instance.RemoveBlocker(this);
                TouchManager.RemoveBlocker(this);
                cts = null;
                EndTutorial();
            }
        }

        protected override void OnKill()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }

        #region SelectableBlocker & TouchBlocker
        bool ISelectableBlocker.IsAllowSelectable(string selectableName)
        {
            return false;
        }

        bool ITouchBlocker.IsAllowListener(string listenerName)
        {
            return false;
        }

        void ITouchBlocker.OnClicked(string buttonName)
        {
        }

        void ISelectableBlocker.OnClicked(string selectableName)
        {
        }

        public int GetPriority()
        {
            return 0;
        }
        #endregion
    }
}
