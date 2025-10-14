using RabbitDog.Utility;
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

        private async Awaitable RunGameStartSequence()
        {
            await Awaitable.WaitForSecondsAsync(0.5f);
        }

        public override void StateRunning(float dt)
        {

        }

        public override void StateEnd(bool isForced)
        {

        }
    }
}
