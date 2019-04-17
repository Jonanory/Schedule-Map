using System.Collections;
using System.Collections.Generic;

namespace ScheduleMap
{
    public class ScheduleList<T>
    {
        public Dictionary<int, List<ScheduleItem<T>>> schedule = new Dictionary<int, List<ScheduleItem<T>>>();

        public ScheduleList() { }
        public ScheduleList(Dictionary<int, List<ScheduleItem<T>>> _schedule)
        {
            schedule = _schedule;
        }
        public ScheduleList(List<ScheduleItem<T>> _schedule)
        {
            schedule = new Dictionary<int, List<ScheduleItem<T>>>();
            foreach (ScheduleItem<T> item in _schedule)
            {
                AddScheduleItem(item.startTime, item);
            }
        }

        public void AddScheduleItem(int time, ScheduleItem<T> scheduleItem)
        {
            if (schedule.ContainsKey(time) == false)
            {
                schedule.Add(time, new List<ScheduleItem<T>>());
            }
            schedule[time].Add(scheduleItem);
        }

        public List<ScheduleItem<T>> GetSchedules(int time)
        {
            if (schedule.ContainsKey(time))
            {
                return schedule[time];
            }
            else
            {
                return new List<ScheduleItem<T>>();
            }
        }
    }

    public class ScheduleItem<T> : Time
    {
        public T value;

        public ScheduleItem(T _value, float _score, int _startTime, int _length = 0) : base(_score, _startTime, _length)
        {
            value = _value;
        }
    }

    public class Time
    {
        public int length;
        public int startTime;
        public float points;
        public Time(float _points, int _startTime, int _length = 0)
        {
            points = _points;
            startTime = _startTime;
            length = _length;
        }
    }
}