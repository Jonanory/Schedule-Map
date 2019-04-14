using System.Collections;
using System.Collections.Generic;

namespace ScheduleMap
{
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