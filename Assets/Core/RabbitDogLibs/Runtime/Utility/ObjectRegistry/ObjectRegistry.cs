using System;
using System.Collections.Generic;
using UnityEngine;

namespace CookApps.Utility
{
    public enum RegistryKey
    {
        None,
        MainCamera,
        PlayButton,
        CoinPanel,
        HeartPanel,
    }
    
    public interface IRegistrable
    {
        RegistryKey Key { get; }
    }
    
    /// <summary>
    /// 같은 키로 여러 오브젝트를 등록할 수 있으며,
    /// Get 시 활성화된 객체를 우선 반환하는 레지스트리.
    /// </summary>
    public class ObjectRegistry : Singleton<ObjectRegistry>
    {
        private readonly Dictionary<RegistryKey, List<IRegistrable>> _registry = new ();

        public static event Action<RegistryKey, IRegistrable> Registered;
        public static event Action<RegistryKey, IRegistrable> Unregistered;

        public void Register(IRegistrable obj)
        {
            if (!_registry.TryGetValue(obj.Key, out var list))
            {
                list = new List<IRegistrable>();
                _registry[obj.Key] = list;
            }

            if (!list.Contains(obj))
            {
                list.Add(obj);
                Registered?.Invoke(obj.Key, obj);
            }
        }

        public void Unregister(IRegistrable obj)
        {
            if (_registry.TryGetValue(obj.Key, out var list))
            {
                if (list.Remove(obj))
                {
                    Unregistered?.Invoke(obj.Key, obj);
                }

                if (list.Count == 0)
                {
                    _registry.Remove(obj.Key);
                }
            }
        }

        public bool TryGetObject<T>(RegistryKey key, out T res) where T : class, IRegistrable
        {
            res = null;
            if (!_registry.TryGetValue(key, out var list) || list.Count == 0)
                return false;

            foreach (var obj in list)
            {
                if (obj is MonoBehaviour { gameObject: { activeInHierarchy: true } })
                {
                    res = obj as T;
                    return true;
                }
            }

            return false;
        }
        
        public bool TryGetObject(RegistryKey key, out RegisteredObject res)
        {
            return TryGetObject<RegisteredObject>(key, out res);
        }
    }
}
