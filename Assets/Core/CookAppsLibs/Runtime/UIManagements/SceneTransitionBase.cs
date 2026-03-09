using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CookApps.UIManagements
{
    public abstract class SceneTransitionBase : CachedMonoBehaviour
    {
        public abstract void Initialize(object viewOption);
        public abstract UniTask FadeInAsync();
        public abstract UniTask FadeOutAsync();
    }
}
