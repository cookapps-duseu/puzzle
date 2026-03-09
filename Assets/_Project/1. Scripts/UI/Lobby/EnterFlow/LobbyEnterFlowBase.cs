using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Template
{
    public abstract class LobbyEnterFlowBase
    {
        /// <summary>
        /// 준비에 필요한 간단한 초기화를 진행
        /// 로비가 열리는 시점에 모든 flow의 Prepare 함수가 실행됨
        /// 해당 flow에 Run까지 안넘어올 수 있으니 감안하고 코드 작성할 것
        /// </summary>
        public abstract void Prepare();

        /// <summary>
        /// Run함수를 실행가능한 상태인지를 리턴
        /// </summary>
        public abstract bool IsRunnable();

        /// <summary>
        /// 실제 flow의 로직을 실행
        /// ex) 게임 클리어 연출 실행하기, 강제 팝업 띄우기, 서버 연결 유지를 확인하기 등등
        /// </summary>
        /// <param name="runNextCallback">모든 작업이 끝나고 다음 flow로 넘길 수 있을 때 호출</param>
        /// <param name="runKilledCallback">작업 후 로비에 머무르지않아서 다음 flow로 넘길 수 없을 경우(ex. 씬이 이동되거나 할 경우) 호출</param>
        public abstract UniTask Run(Action runNextCallback, Action runKilledCallback);

        /// <summary>
        /// 외부에서 실행 중인 flow를 강제로 종료시키는 경우 호출됨
        /// </summary>
        public virtual void ForceKill() {}
    }
}