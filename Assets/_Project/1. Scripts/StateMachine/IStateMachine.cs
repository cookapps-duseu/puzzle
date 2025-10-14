namespace Template
{
    public interface IStateMachine
    {
        public T AddNextState<T>() where T : StateBase, new ();
    }
}
