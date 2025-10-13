using System;
using UnityEngine;

namespace RabbitDog.Utility
{
    public enum ServerTimeState
    {
        Invalid,
        Valid
    }

    public class TimeSystem : Singleton<TimeSystem>
    {
        private DateTime serverDate = DateTime.UtcNow;
        private float receivedTime;
        private static ServerTimeState state = ServerTimeState.Invalid;

        public static bool IsValidServerTime { get => state == ServerTimeState.Valid; }
        public static DateTime GetUtcTime()
        {
            if (false)
            // if (isProd)
            {
                return Instance.GetServerUtcTimeInternal();
            }
            else
            {
                return DateTime.UtcNow;
            }
        }

        private DateTime GetServerUtcTimeInternal()
        {
            var sec = Time.realtimeSinceStartup - receivedTime;
            return serverDate.AddSeconds(sec);
        }

        public static bool TryGetServerUtcTime(out DateTime ret)
        {
            ret = GetUtcTime();
            return IsValidServerTime;
        }

        public void HandleServerTime(int serverUtcTimestamp)
        {
            if (serverUtcTimestamp == 0)
                serverUtcTimestamp = DateTime.UtcNow.ToIntTimestamp();
            receivedTime = Time.realtimeSinceStartup;
            serverDate = serverUtcTimestamp.ToUtcDateTime();
        }
    }
}
