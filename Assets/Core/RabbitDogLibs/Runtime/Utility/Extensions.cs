using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = System.Random;

namespace RabbitDog.Utility
{
    public static class Vector2Extension
    {
        public static Vector2 Rotate(this Vector2 v, float degree)
        {
            return Quaternion.Euler(0, 0, degree) * v;
        }
    }

    public static class NullChecker<T> where T : class
    {
        public static Predicate<T> NullCheck = x => x == null;
    }

    public static class RandomExtensions
    {
        public static float Next(this Random random, float min, float max)
        {
            return (float) (random.NextDouble() * (max - min)) + min;
        }

        public static float Next(this Random random, float max)
        {
            return (float) (random.NextDouble() * max);
        }

        public static long Next(this Random random, long min, long max)
        {
            if (max <= min)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "max must be > min!");
            }

            var uRange = (ulong) (max - min);
            ulong back = (uint) random.Next();
            ulong front = (uint) random.Next();
            ulong ulongRand = (front << 32) | back;
            return (long) (ulongRand % uRange) + min;
        }

        public static void Shuffle<T>(this Random random, IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static Vector2 InsideCircle(this Random random, float maxRad)
        {
            float degree = random.Next(0f, 360f);
            float distance = random.Next(0f, maxRad);
            Vector2 unitVector = Vector2.left.Rotate(degree);
            return unitVector * distance;
        }
    }

    public static class StringExtensions
    {
        public static Color HexColor(this string hexCode)
        {
            if (ColorUtility.TryParseHtmlString(hexCode, out Color color))
            {
                return color;
            }

            return Color.white;
        }

        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static ulong djb2Hash(this string s)
        {
            ulong hash = 5381;
            for (var i = 0; i < s.Length; i++)
            {
                hash = (hash << 5) + hash + s[i];
            }

            return hash;
        }

        public static ulong djb2HashCaseInsensitive(this string s)
        {
            ulong hash = 5381;
            for (var i = 0; i < s.Length; i++)
            {
                hash = ((hash << 5) + hash) ^ (s[i] & ~0x20UL);
            }

            return hash;
        }

        public static ulong djb2Hash(this ReadOnlySpan<char> s)
        {
            ulong hash = 5381;
            for (var i = 0; i < s.Length; i++)
            {
                hash = ((hash << 5) + hash) + s[i];
            }

            return hash;
        }

        public static string ToInvariantString<T>(this T obj) where T : unmanaged
        {
            switch (obj)
            {
                case sbyte value: return value.ToString(CultureInfo.InvariantCulture);
                case byte value: return value.ToString(CultureInfo.InvariantCulture);
                case short value: return value.ToString(CultureInfo.InvariantCulture);
                case ushort value: return value.ToString(CultureInfo.InvariantCulture);
                case int value: return value.ToString(CultureInfo.InvariantCulture);
                case uint value: return value.ToString(CultureInfo.InvariantCulture);
                case long value: return value.ToString(CultureInfo.InvariantCulture);
                case ulong value: return value.ToString(CultureInfo.InvariantCulture);
                case char value: return value.ToString(CultureInfo.InvariantCulture);
                case float value: return value.ToString(CultureInfo.InvariantCulture);
                case double value: return value.ToString(CultureInfo.InvariantCulture);
                case decimal value: return value.ToString(CultureInfo.InvariantCulture);
                case bool value: return value.ToString(CultureInfo.InvariantCulture);
            }

            return obj.ToString();
        }

        public static string ToCommaString(this long value)
        {
            return $"{value:#,##0}";
        }

        public static string ToCommaString(this int value)
        {
            return $"{value:#,##0}";
        }
    }

    public static class AwaitableExtensions
    {
        public static async Awaitable WhenAll(this IList<Awaitable> tasks)
        {
            for (var i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                await task;
            }
        }
        
        public static async Awaitable WhenAll<T>(this IList<Awaitable<T>> tasks)
        {
            for (var i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                await task;
            }
        }
        
        public static async Awaitable WaitUntil(Func<bool> predicate, CancellationToken cancellationToken = default)
        {
            while (!predicate())
                await Awaitable.NextFrameAsync(cancellationToken);
        }
        
        public static async Awaitable<T> WaitUntilDone<T>(this AsyncOperationHandle<T> handle)
        {
            while (true)
            {
                if (!handle.IsValid())
                    return default;
        
                if (handle.IsDone)
                    return handle.Result;

                await Awaitable.NextFrameAsync();
            }
        }

        public static async Awaitable WaitUntilDone(this AsyncOperationHandle handle)
        {
            while (true)
            {
                if (!handle.IsValid())
                    return;
        
                if (handle.IsDone)
                    return;

                await Awaitable.NextFrameAsync();
            }
        }
        
        public static async void Forget(this Awaitable awaitable)
        {
            try
            {
                await awaitable;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static async void Forget<T>(this Awaitable<T> awaitable)
        {
            try
            {
                await awaitable;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void ContinueWith(this Awaitable awaitable, Action continuationAction)
        {
            Awaitable.Awaiter awaiter = awaitable.GetAwaiter();
            awaiter.OnCompleted(continuationAction);
        }

        public static async void ContinueWith<TState>(this Awaitable awaitable, Action<TState> continuationAction,
            TState state)
        {
            try
            {
                await awaitable;
                continuationAction.Invoke(state);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw e;
            }
        }

        public static async void ContinueWith<T>(this Awaitable<T> awaitable, Action<T> continuationAction)
        {
            try
            {
                var res = await awaitable;
                continuationAction.Invoke(res);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw e;
            }
        }

        public static async void ContinueWith<T, TState>(this Awaitable<T> awaitable, Action<T, TState> continuationAction,
            TState state)
        {
            try
            {
                var res = await awaitable;
                continuationAction?.Invoke(res, state);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw e;
            }
        }
    }
    
    public static class TransformExtensions
    {
        public static List<Transform> GetAllChildRecursive(this Transform tr)
        {
            var children = new List<Transform>();
            GetAllChildRecursive(tr, children);
            return children;
        }

        private static void GetAllChildRecursive(Transform parent, List<Transform> children)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                children.Add(child);
                GetAllChildRecursive(child, children); // 재귀 호출로 자식의 자식들도 추가
            }
        }
    }
    
    public static class DateTimeExtensions
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int ToIntTimestamp(this DateTime value)
        {
            var elapsedTime = value.ToUniversalTime() - Epoch;
            return (int)elapsedTime.TotalSeconds;
        }

        public static DateTime ToLocalDateTime(this int value)
        {
            var utc = ToUtcDateTime(value);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
        }

        public static DateTime ToUtcDateTime(this int value)
        {
            return Epoch.AddSeconds(value);
        }

        public static long ToLongTimestamp(this DateTime value)
        {
            var elapsedTime = value.ToUniversalTime() - Epoch;
            return (long)elapsedTime.TotalMilliseconds;
        }

        public static DateTime ToLocalDateTime(this long value)
        {
            var utc = ToUtcDateTime(value);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
        }
        
        public static DateTime ToUtcDateTime(this long value)
        {
            return Epoch.AddMilliseconds(value);
        }
    }
}
