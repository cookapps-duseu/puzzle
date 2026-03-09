using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CookApps.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CookApps
{
    public static class AddressableDownloader
    {
        public static async UniTask<long> GetTotalDownloadSize(IReadOnlyList<string> remoteLabels)
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(remoteLabels);
            var totalSize = await sizeHandle.WaitUntilDone();
            Addressables.Release(sizeHandle);
            return totalSize;
        }

        public static async UniTask DownloadAllAsync(IReadOnlyList<string> remoteLabels, Action<float> onDownloadProgress = null)
        {
            var downloadHandle = Addressables.DownloadDependenciesAsync(remoteLabels, true);
            while (!downloadHandle.IsDone)
            {
                onDownloadProgress?.Invoke(downloadHandle.PercentComplete);
                await UniTask.NextFrame();
            }
        }

        public static void ClearDownloadCacheAsync()
        {
            Caching.ClearCache();
        }
    }
}
