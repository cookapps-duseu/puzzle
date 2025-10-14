using System;
using MemoryPack;
using RabbitDog.CypherPrefs;
using RabbitDog.Utility;

namespace Template
{
    [MemoryPackable]
    public partial class TimerData
    {
        public long StartTimeUnix;
        public long EndTimeUnix;
    }

    public class TimerDataSourceImpl : Preference<int, TimerData>, ITimerDataSource
    {
        public TimerDataSourceImpl() : base(PreferenceGetterSetter.Default)
        {
            base.Load();
        }

        public bool GetTimer(int type, out DateTime startTime, out DateTime endTime)
        {
            var timerData = GetData(type);
            if (timerData == null)
            {
                startTime = DateTime.MinValue;
                endTime = DateTime.MinValue;
                return false;
            }
            
            startTime = timerData.StartTimeUnix.ToUtcDateTime();
            endTime = timerData.EndTimeUnix.ToUtcDateTime();
            return true;
        }

        public void SaveTimer(int type, DateTime startTime, DateTime endTime)
        {
            var timerData = GetData(type);
        
            if (timerData != null)
            {
                timerData.StartTimeUnix = startTime.ToLongTimestamp();
                timerData.EndTimeUnix = endTime.ToLongTimestamp();
                isDirty = true;
            }
            else
            {
                SetData(type, new TimerData
                {
                    StartTimeUnix = startTime.ToLongTimestamp(),
                    EndTimeUnix = endTime.ToLongTimestamp()
                });
            }
            Save();
        }

        public void ClearTimer(int type)
        {
            RemoveData(type);
        }

        public override string PreferenceKey => "TimerData";
    }
}
