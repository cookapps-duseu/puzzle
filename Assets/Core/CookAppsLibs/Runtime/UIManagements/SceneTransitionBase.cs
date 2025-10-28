using UnityEngine;

namespace CookApps.UIManagements
{
    public abstract class SceneTransitionBase : CachedMonoBehaviour
    {
        public abstract void Initialize(object viewOption);
        public abstract Awaitable FadeInAsync();
        public abstract Awaitable FadeOutAsync();
    }
}
