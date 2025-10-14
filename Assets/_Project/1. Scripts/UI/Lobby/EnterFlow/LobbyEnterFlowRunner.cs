using System;
using System.Collections.Generic;

namespace Template
{
    public class LobbyEnterFlowRunner
    {
        private List<LobbyEnterFlowBase> flows = new ();
        public event Action OnKilled;
        private Action finishCallback;
        private bool isRunning = false;
        public bool IsRunning => isRunning;
        private int runningFlowIdx = 0;

        public void AddFlow(LobbyEnterFlowBase flow)
        {
            flows.Add(flow);
        }

        private void PrepareAllFlows()
        {
            for (int i = 0; i < flows.Count; i++)
            {
                flows[i].Prepare();
            }
        }

        public void StartRunFlow(Action finishCallback)
        {
            PrepareAllFlows();
            runningFlowIdx = 0;
            this.finishCallback = finishCallback;
            RunFlow();
            isRunning = true;
        }

        private void RunFlow()
        {
            LobbyEnterFlowBase flow;
            do
            {
                if (runningFlowIdx >= flows.Count)
                {
                    finishCallback?.Invoke();
                    Killed();
                    return;
                }

                flow = flows[runningFlowIdx++];
            } while (!flow.IsRunnable());

            flow.Run(RunFlow, Killed);
        }

        private void Killed()
        {
            OnKilled?.Invoke();
            Clear();
        }

        public void Clear()
        {
            OnKilled = null;
            foreach (var flow in flows)
            {
                flow.ForceKill();
            }

            flows.Clear();
            runningFlowIdx = 0;
            finishCallback = null;
            isRunning = false;
        }
    }
}