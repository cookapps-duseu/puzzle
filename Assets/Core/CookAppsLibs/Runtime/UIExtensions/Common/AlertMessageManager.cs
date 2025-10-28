using CookApps.UIManagements;
using CookApps.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CookApps.UIExtensions
{
    public class AlertMessageManager : SingletonMonoBehaviour<AlertMessageManager>
    {
        private AsyncOperationHandle<GameObject> handle;
        AlertMessage alertMessage;

        private Transform CachedTr;

        bool isInitialized = false;

        public async Awaitable Initialize(string alertAddress)
        {
            if (isInitialized)
                return;

            CachedTr = transform;

            isInitialized = true;

            var handle = Addressables.InstantiateAsync(alertAddress, CachedTr);
            var alertGo = await handle.WaitUntilDone();
            alertGo.SetActive(false);
            alertMessage = alertGo.GetComponent<AlertMessage>();
        }

        public void Clear()
        {
            if (!isInitialized)
                return;

            isInitialized = false;

            Destroy(alertMessage.CachedGo);
            Addressables.Release(handle);
        }

        public void ShowAlertMessage(string message)
        {
            ShowAlertMessage(message, Color.white);
        }

        public void ShowAlertMessage(string message, Color textColor)
        {
            if (SceneUILayerManager.Instance.FloatingNode == null)
                return;

            if (string.IsNullOrWhiteSpace(message))
                return;

            if (alertMessage.Message == message)
                return;

            alertMessage.CachedTr.SetParent(SceneUILayerManager.Instance.FloatingNode, false);
            alertMessage.CachedGo.SetActive(true);
            alertMessage.Show(message, textColor);
        }

        public void Return(AlertMessage alert)
        {
            alert.CachedGo.SetActive(false);
            alert.CachedTr.SetParent(CachedTr, false);
        }
    }

}