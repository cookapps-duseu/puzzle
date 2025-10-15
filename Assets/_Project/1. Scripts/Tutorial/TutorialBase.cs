using System;

namespace Template
{
    public abstract class TutorialBase
    {
        private Action onEndTutorial;
        
        public void RunTutorial(Action OnEndTutorial)
        {
            onEndTutorial = OnEndTutorial;
            OnStartTutorial();
        }
        
        protected abstract void OnStartTutorial();
        
        protected void EndTutorial() => onEndTutorial?.Invoke();
        public void Kill()
        {
            onEndTutorial = null;
            OnKill();
        }

        protected abstract void OnKill();
    }
}
