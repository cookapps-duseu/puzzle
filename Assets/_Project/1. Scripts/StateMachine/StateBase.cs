namespace Template
{
    public abstract class StateBase
    {
        protected IStateMachine Machine { get; private set; }
        public virtual void StateSetup(IStateMachine machine) => Machine = machine;
        public abstract void StateInit(object owner);
        public abstract void StateStart();
        public abstract void StateRunning(float dt);
        public abstract void StateEnd(bool isForced);
    }
}
