using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Template
{
    public class EnterFlow_Template : LobbyEnterFlowBase
    {
        private LobbyMain lobbyMain;
        private CancellationTokenSource cts;

        public EnterFlow_Template(LobbyMain lobbyMain)
        {
            this.lobbyMain = lobbyMain;
        }

        public override void Prepare()
        {
            
        }

        public override bool IsRunnable()
        {
            return true;
        }

        public override async UniTask Run(Action runNextCallback, Action runKilledCallback)
        {
            runNextCallback?.Invoke();
        }

        public override void ForceKill()
        {
            base.ForceKill();
            if (cts == null)
                return;
            
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }
}