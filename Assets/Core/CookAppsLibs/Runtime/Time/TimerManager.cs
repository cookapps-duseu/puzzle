using System;
using System.Collections.Generic;
using UnityEngine;

namespace CookApps.Utility
{
    public interface ITimerDataSource
    {
        bool GetTimer(int type, out DateTime startTime, out DateTime endTime);
        void SaveTimer(int type, DateTime startTime, DateTime endTime);
        void ClearTimer(int type);
    }

    public enum TimerEventType
    {
        TimerAdded,   // 타이머가 시작되거나 연장되었을 때 호출
        TimerRunning, // 타이머가 돌고 있을 때 초당 호출
        TimerEnd      // 타이머가 종료됬을 때 호출
    }

    class TimerListener
    {
        public TimerListener(int key, TimerEventType eventType, Action<DateTime, DateTime> callback)
        {
            this.key = key;
            this.eventType = eventType;
            this.callback = callback;
            isEventInvoked = false;
        }

        public readonly int key;
        public readonly TimerEventType eventType;
        public readonly Action<DateTime, DateTime> callback;
        public bool isEventInvoked;
    }

    public class TimerManager : Singleton<TimerManager>
    {
        private ITimerDataSource dataSource;
        private Dictionary<int, List<TimerListener>> listenersDict = new ();
        private int listenerKey = 0;

        private List<KeyValuePair<int, TimerListener>> willAddListeners = new ();
        private List<KeyValuePair<int, int>> willRemoveListeners = new ();

        public void Initialize(ITimerDataSource dataSource)
        {
            this.dataSource = dataSource;
            Update().Forget();
        }

        #region Listener logic
        public int AddListener(int type, TimerEventType eventType, Action<DateTime, DateTime> callback)
        {
            willAddListeners.Add(new KeyValuePair<int, TimerListener>(type, new TimerListener(++listenerKey, eventType, callback)));
            return listenerKey;
        }

        public void RemoveListener(int type, int key)
        {
            willRemoveListeners.Add(new KeyValuePair<int, int>(type, key));
        }

        private async Awaitable Update()
        {
            while (true)
            {
                if (willRemoveListeners.Count > 0)
                {
                    for (int i = 0; i < willRemoveListeners.Count; i++)
                    {
                        var type = willRemoveListeners[i].Key;
                        var key = willRemoveListeners[i].Value;
                        if (listenersDict.TryGetValue(type, out var listeners))
                        {
                            listeners.RemoveAll(x => x.key == key);
                        }

                        willAddListeners.RemoveAll(x => x.Key == type && x.Value.key == key);
                    }

                    willRemoveListeners.Clear();
                }

                var serverTime = TimeSystem.GetUtcTime();
                foreach (var pair in listenersDict)
                {
                    if (!dataSource.GetTimer(pair.Key, out var saveStartTime, out var saveEndTime))
                        continue;
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        switch (pair.Value[i].eventType)
                        {
                            case TimerEventType.TimerRunning:
                            {
                                if (saveStartTime <= serverTime && serverTime <= saveEndTime)
                                {
                                    pair.Value[i].callback.Invoke(saveStartTime, saveEndTime);
                                }
                            }
                                break;
                            case TimerEventType.TimerEnd:
                            {
                                if (!pair.Value[i].isEventInvoked && saveEndTime < serverTime)
                                {
                                    pair.Value[i].isEventInvoked = true;
                                    pair.Value[i].callback.Invoke(saveStartTime, saveEndTime);
                                }
                            }
                                break;
                        }
                    }
                }

                if (willAddListeners.Count > 0)
                {
                    for (int i = 0; i < willAddListeners.Count; i++)
                    {
                        var type = willAddListeners[i].Key;
                        var listener = willAddListeners[i].Value;
                        if (!listenersDict.TryGetValue(type, out var listeners))
                        {
                            listeners = new List<TimerListener>();
                            listenersDict.Add(type, listeners);
                        }

                        listeners.Add(listener);
                    }

                    willAddListeners.Clear();
                }

                await Awaitable.WaitForSecondsAsync(0.25f);
            }
        }
        #endregion

        public bool IsRunning(int type)
        {
            if (!dataSource.GetTimer(type, out var saveStartTime, out var saveEndTime))
                return false;

            var serverTime = TimeSystem.GetUtcTime();
            if (saveStartTime <= serverTime && serverTime <= saveEndTime)
            {
                return true;
            }
            else
            {
                dataSource.ClearTimer(type);
                return false;
            }
        }

        public void AddTimer(int type, int min)
        {
            AddTimerSecond(type, min * 60);
        }

        public void AddTimerSecond(int type, int seconds)
        {
            var serverTime = TimeSystem.GetUtcTime();

            if (dataSource.GetTimer(type, out var startTime, out var endTime))
            {
                if (endTime < serverTime || serverTime < startTime)
                {
                    startTime = serverTime;
                    endTime = startTime;
                }

                endTime = endTime.AddSeconds(seconds);
            }
            else
            {
                startTime = serverTime;
                endTime = startTime.AddSeconds(seconds);
            }

            if (listenersDict.ContainsKey(type))
            {
                for (int i = 0; i < listenersDict[type].Count; i++)
                {
                    if (listenersDict[type][i].eventType == TimerEventType.TimerAdded)
                    {
                        listenersDict[type][i].callback.Invoke(startTime, endTime);
                    }

                    if (listenersDict[type][i].eventType == TimerEventType.TimerEnd)
                    {
                        listenersDict[type][i].isEventInvoked = false;
                    }
                }
            }

            dataSource.SaveTimer(type, startTime, endTime);
        }

        public void AddTimer(int type, DateTime endTime)
        {
            var serverTime = TimeSystem.GetUtcTime();
            if (dataSource.GetTimer(type, out var savedStartTime, out var savedEndTime))
            {
                if (endTime == savedEndTime)
                {
                    return;
                }

                if (endTime < savedEndTime)
                {
                    Debug.LogWarning($"AddTimer {type} savedTime({savedEndTime}) is longer than passedTime({endTime})");
                }
            }
            else
            {
                savedStartTime = serverTime;
            }

            if (listenersDict.ContainsKey(type))
            {
                for (var i = 0; i < listenersDict[type].Count; i++)
                {
                    if (listenersDict[type][i].eventType == TimerEventType.TimerAdded)
                    {
                        listenersDict[type][i].callback.Invoke(savedStartTime, endTime);
                    }

                    if (listenersDict[type][i].eventType == TimerEventType.TimerEnd)
                    {
                        listenersDict[type][i].isEventInvoked = false;
                    }
                }
            }

            dataSource.SaveTimer(type, savedStartTime, endTime);
        }

        public void RemoveTimer(int type)
        {
            dataSource.ClearTimer(type);
        }

        public DateTime GetEndTime(int type)
        {
            dataSource.GetTimer(type, out var startTime, out var endTime);
            return endTime;
        }
    }
}