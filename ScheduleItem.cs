using System.Collections;
using System.Collections.Generic;

namespace ScheduleMap
{
    public class ScheduleList<T>
    {
        Dictionary<int, List<ScheduleItem<T>>> schedule = new Dictionary<int, List<ScheduleItem<T>>>();

        public float totalPointsAvailable = 0f;
        Dictionary<int, float> MaxPossiblePointsRemaining = new Dictionary<int, float>();

        public ScheduleList() { }
        public ScheduleList(Dictionary<int, List<ScheduleItem<T>>> _schedule)
        {
            schedule = _schedule;
            CalculateMaxPoints();
        }
        public ScheduleList(List<ScheduleItem<T>> _schedule)
        {
            schedule = new Dictionary<int, List<ScheduleItem<T>>>();
            foreach (ScheduleItem<T> item in _schedule)
            {
                Add(item);
            }
            CalculateMaxPoints();
        }

        void CalculateMaxPoints()
        {
            totalPointsAvailable = 0f;
            MaxPossiblePointsRemaining = new Dictionary<int, float>();

            foreach( int time in schedule.Keys )
            {
                float currentMax = 0f;
                foreach( ScheduleItem<T> item in schedule[time] )
                {
                    if( item.points > currentMax )
                    {
                        currentMax = item.points;
                    }
                }
                if (currentMax > 0f)
                {
                    MaxPossiblePointsRemaining.Add(time, currentMax);
                    totalPointsAvailable += currentMax;
                }
            }
        }

        public void Add(ScheduleItem<T> _scheduleItem)
        {
            if (schedule.ContainsKey(_scheduleItem.startTime) == false)
            {
                schedule.Add(_scheduleItem.startTime, new List<ScheduleItem<T>>());
            }
            schedule[_scheduleItem.startTime].Add(_scheduleItem);

            if( MaxPossiblePointsRemaining.ContainsKey(_scheduleItem.startTime) == false )
            {
                MaxPossiblePointsRemaining.Add(_scheduleItem.startTime, _scheduleItem.points);
                totalPointsAvailable += _scheduleItem.points;
            }
            else if ( MaxPossiblePointsRemaining[_scheduleItem.startTime] < _scheduleItem.points )
            {
                totalPointsAvailable += _scheduleItem.points - MaxPossiblePointsRemaining[_scheduleItem.startTime];
                MaxPossiblePointsRemaining[_scheduleItem.startTime] = _scheduleItem.points;
            }
        }

        public List<ScheduleItem<T>> GetSchedules(int _time)
        {
            if (schedule.ContainsKey(_time))
            {
                return schedule[_time];
            }
            else
            {
                return new List<ScheduleItem<T>>();
            }
        }

        public void RemoveTime( int _time )
        {
            if( schedule.ContainsKey( _time ))
            {
                schedule.Remove(_time);
            }
            if( MaxPossiblePointsRemaining.ContainsKey( _time ))
            {
                totalPointsAvailable -= MaxPossiblePointsRemaining[_time];
                MaxPossiblePointsRemaining.Remove(_time);
            }
        }

        public void Remove( ScheduleItem<T> _scheduleItem )
        {
            if( Contains(_scheduleItem) )
            {
                schedule[_scheduleItem.startTime].Remove(_scheduleItem);
            }
        }

        public bool Contains( ScheduleItem<T> _scheduleItem )
        {
            return schedule.ContainsKey(_scheduleItem.startTime) && schedule[_scheduleItem.startTime].Contains(_scheduleItem);
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