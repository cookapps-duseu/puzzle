using System;
using CookApps.Utility;
using Unity.Notifications;
using UnityEngine;

namespace Template
{
    public static class LocalNotificationManager
    {
        private const string AndroidChannelId = "default_channel";
        private const string CommonCategory = "Common";
        
        public static void Initialize()
        {
            var arg = new NotificationCenterArgs();
            arg.AndroidChannelId = AndroidChannelId;
            arg.AndroidChannelDescription = "android default channel id";
            arg.AndroidChannelName = "Default Channel";
            arg.PresentationOptions = NotificationPresentation.Alert | NotificationPresentation.Badge |
                                      NotificationPresentation.Sound | NotificationPresentation.Vibrate;
            NotificationCenter.Initialize(arg);
            Check();
            CancelAll();

            AppLifeCycleEventsDispatcher.OnPause += OnApplicationPause;
            AppLifeCycleEventsDispatcher.OnResume += OnApplicationResume;
        }

        public static NotificationsPermissionRequest RequestPermission()
        {
            return NotificationCenter.RequestPermission();
        }

        public static void CancelAll()
        {
            NotificationCenter.CancelAllDeliveredNotifications();
            NotificationCenter.CancelAllScheduledNotifications();
            NotificationCenter.ClearBadge();
        }

        private static void OnApplicationResume()
        {
            Check();
            CancelAll();
        }

        private static void OnApplicationPause()
        {
            CancelAll();
            Schedule();
        }

        public static void Check()
        {
            var clickedNotification = NotificationCenter.LastRespondedNotification;
            if (!clickedNotification.HasValue)
                return;

            var clicked = clickedNotification.Value;
            RespondedLog(clicked);
        }

        public static void Schedule()
        {
            if (!SettingOptions.IsAlarm)
                return;
            if (!UserDataManager.IsAlive())
                return;

            // TODO: schedule logic
            Schedule_HeartFull();
        }

        private static void Schedule_HeartFull()
        {
            
            var assetData = UserDataManager.Instance.GetAssetData();
            var maxHeart = assetData.GetMaxHeart();
            var curHeart = assetData.GetAssetAmount(AssetType.Heart);
            if (curHeart >= maxHeart)
            {
                return;
            }
            var leftTime = assetData.GetRemainHeartFullRecoverSeconds();
            if (leftTime <= 0)
            {
                return;
            }

            var notification = new Notification
            {
                Identifier = null,
                Title = LocalizationManager.Instance.GetString("LocalPush_HeartMax_Title"),
                Text = LocalizationManager.Instance.GetString("LocalPush_HeartMax_Desc"),
                Group = CommonCategory,
                Data = "HeartFull",
                ShowInForeground = false,
                Badge = 1
            };

            var when = TimeSystem.GetUtcTime().AddSeconds(leftTime);
            NotificationCenter.ScheduleNotification(notification, AndroidChannelId,
                new NotificationDateTimeSchedule(when));
            ScheduledLog(notification, when);
        }

        private static void ScheduledLog(Notification n, DateTime when)
        {
            Debug.Log($"Notification scheduled : [{n.Data}] [{n.Title}] [{n.Text}] : {when}");
        }

        private static void RespondedLog(Notification n)
        {
            Debug.Log($"Notification responded : [{n.Data}] [{n.Title}] [{n.Text}]");
        }
    }
}
