using System.Collections.Generic;
using RabbitDog.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace RabbitDog
{
    [CreateAssetMenu(fileName = "AtlasManager", menuName = "ScriptableObjects/AtlasManager", order = 1)]
    public class AtlasManagerScriptableObject : ScriptableObject
    {
        [HideInInspector] public List<string> folderPathGuids = new ();
        [HideInInspector] public List<AssetReferenceT<SpriteAtlas>> atlasRefs = new ();
        [HideInInspector] public SerializableDictionary<ulong, ulong> spriteNameToAtlasDict = new ();
    }

    public class AtlasManager : Singleton<AtlasManager>
    {
        private class AtlasCache
        {
            public ulong hash;
            public AssetReferenceT<SpriteAtlas> atlasRef;
            public SpriteAtlas atlas;
            public Dictionary<string, Sprite> sprites = new ();
            public int refCount;
        }

        private AtlasManagerScriptableObject so;
        private Dictionary<ulong, AtlasCache> atlasCaches;

        public async Awaitable<Sprite> GetSprite(string spriteName)
        {
            if (!so.spriteNameToAtlasDict.TryGetValue(spriteName.djb2Hash(), out var atlasNameHash))
            {
                Debug.LogError($"Sprite {spriteName} not found in AtlasManager");
                return null;
            }

            if (atlasCaches == null)
            {
                atlasCaches = new Dictionary<ulong, AtlasCache>();
                foreach (var spRef in so.atlasRefs)
                {
                    var hash = spRef.AssetGUID.djb2Hash();
                    atlasCaches[hash] = new AtlasCache()
                    {
                        hash = hash,
                        atlasRef = spRef,
                    };
                }
            }

            if (!atlasCaches.TryGetValue(atlasNameHash, out var atlasCache))
            {
                return null;
            }

            atlasCache.refCount++;
            if (atlasCache.sprites.TryGetValue(spriteName, out var sprite))
            {
                return sprite;
            }

            if (atlasCache.atlas == null)
            {
                var spRef = atlasCache.atlasRef;
                var oper = spRef.OperationHandle;
                if (!spRef.OperationHandle.IsValid())
                    oper = spRef.LoadAssetAsync();
                await oper.WaitUntilDone();
                if (!oper.IsValid())
                    return null;
                atlasCache.atlas = oper.Result as SpriteAtlas;
            }

            sprite = atlasCache.atlas?.GetSprite(spriteName);
            atlasCache.sprites[spriteName] = sprite;
            return sprite;
        }

        public void UnloadSprite(string spriteName)
        {
            if (!so.spriteNameToAtlasDict.TryGetValue(spriteName.djb2Hash(), out var atlasNameHash))
            {
                return;
            }

            if (atlasCaches == null)
            {
                return;
            }

            if (!atlasCaches.TryGetValue(atlasNameHash, out var atlasCache))
            {
                return;
            }

            atlasCache.refCount--;
            if (atlasCache.refCount > 0)
            {
                return;
            }

            foreach (var sprite in atlasCache.sprites.Values)
            {
                Object.Destroy(sprite);
            }

            atlasCache.sprites.Clear();
            atlasCache.atlas = null;
            atlasCache.atlasRef.ReleaseAsset();
        }

        public async Awaitable Initialize(string soAddressable)
        {
            var handle = Addressables.LoadAssetAsync<AtlasManagerScriptableObject>(soAddressable);
            so = await handle.WaitUntilDone();
        }
    }
}
