using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleMap
{
    public class ScheduleList<T>
    {
        Dictionary<int, List<ScheduleItem<T>>> Schedule = new Dictionary<int, List<ScheduleItem<T>>>();

        public float TotalPointsAvailable = 0f;
        Dictionary<int, float> MaxPossiblePointsRemaining = new Dictionary<int, float>();

        bool Discounting = false;
        float DiscountRate = 1f;

        public ScheduleList() { }
        public ScheduleList(Dictionary<int, List<ScheduleItem<T>>> _schedule, float _discountRate = 1f)
        {
            Schedule = _schedule;
            SetDiscountRate(_discountRate);
            CalculateMaxPoints();
        }
        public ScheduleList(List<ScheduleItem<T>> _schedule, float _discountRate = 1f)
        {
            Schedule = new Dictionary<int, List<ScheduleItem<T>>>();
            foreach (ScheduleItem<T> item in _schedule)
            {
                Add(item);
            }
            SetDiscountRate(_discountRate);
        }

        void SetDiscountRate(float _discountRate = 1f)
        {
            if (_discountRate != 1f)
            {
                DiscountRate = _discountRate;
                Discounting = true;
            }
            else
            {
                DiscountRate = 1f;
                Discounting = false;
            }
            CalculateMaxPoints();
        }

        void CalculateMaxPoints()
        {
            TotalPointsAvailable = 0f;
            MaxPossiblePointsRemaining = new Dictionary<int, float>();

            foreach (int time in Schedule.Keys)
            {
                float currentMax = 0f;
                foreach (ScheduleItem<T> item in Schedule[time])
                {
                    if (item.Points > currentMax)
                    {
                        currentMax = item.Points;
                    }
                }
                if (currentMax > 0f)
                {
                    MaxPossiblePointsRemaining.Add(time, currentMax);
                    if (Discounting)
                    {
                        TotalPointsAvailable += currentMax * Mathf.Pow(DiscountRate, time);
                    }
                    else
                    {
                        TotalPointsAvailable += currentMax;
                    }
                }
            }
        }

        public void ClampTime(int startTime, int endTime)
        {
            Dictionary<int, List<ScheduleItem<T>>> newSchedule = new Dictionary<int, List<ScheduleItem<T>>>();

            foreach (int time in Schedule.Keys)
            {
                if (time < startTime || time > endTime)
                {
                    continue;
                }
                newSchedule.Add(time, Schedule[time]);
            }
            Schedule = newSchedule;

            CalculateMaxPoints();
        }

        public void Add(ScheduleItem<T> _scheduleItem)
        {
            if (Schedule.ContainsKey(_scheduleItem.StartTime) == false)
            {
                Schedule.Add(_scheduleItem.StartTime, new List<ScheduleItem<T>>());
            }
            Schedule[_scheduleItem.StartTime].Add(_scheduleItem);

            float discount = 1f;
            if (Discounting)
            {
                discount = Mathf.Pow(DiscountRate, _scheduleItem.StartTime);
            }
            if (MaxPossiblePointsRemaining.ContainsKey(_scheduleItem.StartTime) == false)
            {
                MaxPossiblePointsRemaining.Add(_scheduleItem.StartTime, _scheduleItem.Points);
                TotalPointsAvailable += _scheduleItem.Points * discount;
            }
            else if (MaxPossiblePointsRemaining[_scheduleItem.StartTime] < _scheduleItem.Points)
            {
                TotalPointsAvailable += discount * (_scheduleItem.Points - MaxPossiblePointsRemaining[_scheduleItem.StartTime]);
                MaxPossiblePointsRemaining[_scheduleItem.StartTime] = _scheduleItem.Points;
            }
        }

        public List<ScheduleItem<T>> GetSchedules(int _time)
        {
            if (Schedule.ContainsKey(_time))
            {
                return Schedule[_time];
            }
            else
            {
                return new List<ScheduleItem<T>>();
            }
        }

        public void RemoveTime(int _time)
        {
            if (Schedule.ContainsKey(_time))
            {
                Schedule.Remove(_time);
            }
            if (MaxPossiblePointsRemaining.ContainsKey(_time))
            {
                if (Discounting)
                {
                    TotalPointsAvailable -= MaxPossiblePointsRemaining[_time] * Mathf.Pow(DiscountRate, _time);
                }
                else
                {
                    TotalPointsAvailable -= MaxPossiblePointsRemaining[_time];
                }
                MaxPossiblePointsRemaining.Remove(_time);
            }
        }

        public void Remove(ScheduleItem<T> _scheduleItem)
        {
            if (Contains(_scheduleItem))
            {
                Schedule[_scheduleItem.StartTime].Remove(_scheduleItem);
            }
        }

        public bool Contains(ScheduleItem<T> _scheduleItem)
        {
            return Schedule.ContainsKey(_scheduleItem.StartTime) && Schedule[_scheduleItem.StartTime].Contains(_scheduleItem);
        }
    }

    public class ScheduleItem<T> : Time
    {
        public T Value;

        public ScheduleItem(T _value, float _score, int _startTime, int _length = 0) : base(_score, _startTime, _length)
        {
            Value = _value;
        }

        public ScheduleItem<T> Duplicate()
        {
            return new ScheduleItem<T>(Value, Points, StartTime, Length);
        }
    }

    public class Time
    {
        public int Length;
        public int StartTime;
        public float Points;
        public Time(float _points, int _startTime, int _length = 0)
        {
            Points = _points;
            StartTime = _startTime;
            Length = _length;
        }
    }
}