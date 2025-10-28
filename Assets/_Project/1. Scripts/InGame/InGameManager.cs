using System.Collections.Generic;
using CookApps;
using UnityEngine;

namespace Template
{
    public class InGameManager : CachedMonoBehaviour, IStateMachine
    {
        public static InGameManager Instance { get; private set; }

        protected void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #region IStateMachine
        private StateBase flowState = null;
        public StateBase CurrentFlowState { get => flowState; }
        Queue<StateBase> nextStates = new ();

        public void Update()
        {
            if (nextStates.Count > 0)
            {
                var nextState = nextStates.Dequeue();
                flowState?.StateEnd(false);
                flowState = nextState;
                flowState.StateInit(null);
                flowState.StateStart();
            }
            
            flowState?.StateRunning(Time.deltaTime);
        }

        public T AddNextState<T>() where T : StateBase, new()
        {
            var state = new T();
            state.StateSetup(this);
            nextStates.Enqueue(state);
            return state;
        }
        #endregion
    }
}
