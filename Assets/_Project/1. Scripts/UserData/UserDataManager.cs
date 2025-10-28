using System;
using System.Collections.Generic;
using CookApps;
using UnityEngine;

namespace Template
{
    public class UserDataManager : SingletonMonoBehaviour<UserDataManager>
    {
        private readonly Dictionary<Type, UserDataContainerBase> userDataMap = new();
        
        public void Initialize()
        {
            LoadAllDatas();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var container in userDataMap.Values)
            {
                container.Dispose();
            }
            userDataMap.Clear();
        }
        
        private void LoadAllDatas()
        {
            userDataMap.Add(typeof(UserBasicDataContainer), new UserBasicDataContainer());
            userDataMap.Add(typeof(UserAssetDataContainer), new UserAssetDataContainer());
            userDataMap.Add(typeof(UserShopDataContainer), new UserShopDataContainer());
            foreach (var container in userDataMap.Values)
            {
                container.InitData();
            }
        }
        
        private void LateUpdate()
        {
            foreach (var container in userDataMap.Values)
            {
                container.Save();
            }
        }
        
        public UserBasicDataContainer GetBasicData()
        {
            if (userDataMap.TryGetValue(typeof(UserBasicDataContainer), out var container))
            {
                return container as UserBasicDataContainer;
            }
            throw new Exception($"No UserDataContainer of type UserBasicDataContainer found.");
        }
        
        public UserAssetDataContainer GetAssetData()
        {
            if (userDataMap.TryGetValue(typeof(UserAssetDataContainer), out var container))
            {
                return container as UserAssetDataContainer;
            }
            throw new Exception($"No UserDataContainer of type UserAssetDataContainer found.");
        }

        public UserShopDataContainer GetShopData()
        {
            if (userDataMap.TryGetValue(typeof(UserShopDataContainer), out var container))
            {
                return container as UserShopDataContainer;
            }
            throw new Exception($"No UserDataContainer of type UserShopDataContainer found.");
        }

        private float elapsedTime = 0f;
        private void Update()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= 0.33333f)
            {
                GetAssetData().CheckHeartTime();
            }
        }
    }
}
