using System;
using System.Collections.Generic;
using CookApps.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CookApps
{
    public static class AddressableDownloader
    {
        public static async Awaitable<long> GetTotalDownloadSize(IReadOnlyList<string> remoteLabels)
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(remoteLabels);
            var totalSize = await sizeHandle.WaitUntilDone();
            Addressables.Release(sizeHandle);
            return totalSize;
        }

        public static async Awaitable DownloadAllAsync(IReadOnlyList<string> remoteLabels, Action<float> onDownloadProgress = null)
        {
            var downloadHandle = Addressables.DownloadDependenciesAsync(remoteLabels, true);
            while (!downloadHandle.IsDone)
            {
                onDownloadProgress?.Invoke(downloadHandle.PercentComplete);
                await Awaitable.NextFrameAsync();
            }
        }

        public static void ClearDownloadCacheAsync()
        {
            Caching.ClearCache();
        }
    }
}
