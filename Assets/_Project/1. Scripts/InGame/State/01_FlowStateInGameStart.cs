using Cysharp.Threading.Tasks;
using CookApps.Utility;
using UnityEngine;

namespace Template
{
    public class FlowStateInGameStart : StateBase
    {
        public override void StateInit(object owner)
        {
        }

        public override void StateStart()
        {
            // 게임 초기 흐름
            RunGameStartSequence().Forget();
        }

        private async UniTask RunGameStartSequence()
        {
            await UniTask.WaitForSeconds(0.5f);
        }

        public override void StateRunning(float dt)
        {

        }

        public override void StateEnd(bool isForced)
        {

        }
    }
}
