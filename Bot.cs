using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScheduleMap
{
    public class Bot<T>
    {
        public float Score = -1;
        public int TimeTrappedFor = 0;
        public Node<T> CurrentNode;

        public Queue<T> Path { get; private set; }
        public Queue<Instruction<T>> Instructions { get; private set; }
        public Queue<Duration<T>> Durations { get; private set; }
        public Queue<ScheduleItem<T>> ScheduleItems { get; private set; }
        List<Duration<T>> DurationList;
        List<ScheduleItem<T>> ScheduleItemList;


        public Bot() { }
        public Bot(T startLocation)
        {
            Path = new Queue<T>();
            Instructions = new Queue<Instruction<T>>();
            Durations = new Queue<Duration<T>>();
            ScheduleItems = new Queue<ScheduleItem<T>>();
            
            DurationList = new List<Duration<T>>();
            ScheduleItemList = new List<ScheduleItem<T>>();
        }

        List<Duration<T>> DuplicationDurationList()
        {
            List<Duration<T>> final = new List<Duration<T>>();

            foreach (Duration<T> duration in DurationList)
            {
                final.Add(duration.Duplicate());
            }
            return final;
        }

        List<ScheduleItem<T>> DuplicateScheduleItemsList()
        {
            List<ScheduleItem<T>> final = new List<ScheduleItem<T>>();

            foreach (ScheduleItem<T> scheduleItem in ScheduleItemList)
            {
                final.Add(scheduleItem.Duplicate());
            }
            return final;
        }

        public Bot<T> Duplicate()
        {
            return new Bot<T>
            {
                Score = Score,
                DurationList = DuplicationDurationList(),
                ScheduleItemList = DuplicateScheduleItemsList(),
                CurrentNode = CurrentNode
            };
        }

        public void MakePath()
        {
            if (DurationList.Count > 0)
            {
                Path = new Queue<T>();
                int count = 0;
                for (int i = 0; i < DurationList.Count - 1; i++)
                {
                    while (count < DurationList[i].CanStayUntil)
                    {
                        Path.Enqueue(DurationList[i].Value);
                        count++;
                    }
                }
                Path.Enqueue(DurationList[DurationList.Count - 1].Value);
            }
        }

        public void MakeDurations()
        {
            Durations = new Queue<Duration<T>>();
            foreach (Duration<T> duration in DurationList)
            {
                Durations.Enqueue(duration);
            }
        }

        public void MakeInstructions()
        {
            Instructions = new Queue<Instruction<T>>();
            foreach (Duration<T> duration in DurationList)
            {
                Instructions.Enqueue(
                    new Instruction<T>(
                        duration.Value,
                        duration.MustArriveBy
                    )
                );
            }
        }

        public void MakeScheduleItems()
        {
            ScheduleItems = new Queue<ScheduleItem<T>>();
            ScheduleItemList = ScheduleItemList.OrderBy(o => o.StartTime).ToList();
            foreach (ScheduleItem<T> scheduleItem in ScheduleItemList)
            {
                ScheduleItems.Enqueue(scheduleItem);
            }
        }

        public void TravelAlong(Edge<T> _edge, int _time)
        {
            Bot<T> newBot = Duplicate();
            if (DurationList.Count == 0 ||
                    EqualityComparer<T>.Default.Equals(DurationList[DurationList.Count - 1].Value, CurrentNode.Value))
            {
                newBot.AddDuration(_edge.Destination.Value, _time, _edge.Length);
            }

            if (_edge.Length > 1)
            {
                newBot.TimeTrappedFor = _edge.Length - 1;
                _edge.BotsTravellingDown.Add(newBot);
            }
            else if (_edge.Length == 1)
            {
                newBot.ArriveAt(_edge.Destination);
            }
        }

        public Bot<T> ArriveAt(Node<T> _node)
        {
            Bot<T> newBot = Duplicate();
            newBot.CurrentNode = _node;

            if (_node.FreeBot == null)
            {
                _node.FreeBot = newBot;
            }
            else
            {
                if (newBot.Score > _node.FreeBot.Score)
                {
                    _node.FreeBot = newBot;
                }
            }
            return newBot;
        }

        public Duration<T> CreateDuration(T _value, int _time, int _distance = 1)
        {
            Duration<T> newDuration = new Duration<T>(_value, _time, _distance);
            if (DurationList.Count > 0)
            {
                DurationList[DurationList.Count - 1].DistanceToNext = _distance;
            }
            DurationList.Add(newDuration);
            return newDuration;
        }

        public Duration<T> AddDuration(T _value, int _time, int _distance = 1)
        {
            if (DurationList.Count == 0)
            {
                return CreateDuration(_value, _time, _distance);
            }
            else if (EqualityComparer<T>.Default.Equals(DurationList[DurationList.Count - 1].Value, _value) == false)
            {
                return CreateDuration(_value, _time, _distance);
            }
            else
            {
                return DurationList[DurationList.Count - 1];
            }
        }

        public void AddScheduleItem(ScheduleItem<T> _scheduleItem)
        {
            ScheduleItemList.Add(_scheduleItem);
        }


        public void ApplyScoreToDuration(T _value, int _time, int _length = 0 )
        {
            if (DurationList.Count > 0
                && EqualityComparer<T>.Default.Equals(DurationList[DurationList.Count - 1].Value, _value) == true)
            {
                int lastIndex = DurationList.Count - 1;
                int currentTime = _time;
                DurationList[lastIndex].AppliedScore = true;

                if (DurationList[lastIndex].MustStayUntil < _time + _length)
                {
                    DurationList[lastIndex].MustStayUntil = _time + _length;
                }
                do
                {
                    if( DurationList[lastIndex].MustArriveBy > currentTime )
                    {
                        DurationList[lastIndex].MustArriveBy = currentTime;
                    }
                    if( lastIndex > 0 && DurationList[lastIndex - 1].CanStayUntil > currentTime - DurationList[lastIndex].DistanceToLast )
                    {
                        DurationList[lastIndex - 1].CanStayUntil = currentTime - DurationList[lastIndex].DistanceToLast;
                    }

                    currentTime -= DurationList[lastIndex].DistanceToLast;
                    lastIndex--;
                } while (lastIndex >= 0 && DurationList[lastIndex].AppliedScore == false);
            }
        }
    }

    public class Instruction<T>
    {
        public T Value;
        public int Time;

        public Instruction(T _value, int _time)
        {
            Value = _value;
            Time = _time;
        }
    }

    public class Duration<T>
    {
        public T Value;

        public int CanArriveAt;
        public int MustArriveBy = Int32.MaxValue;

        public int MustStayUntil;
        public int CanStayUntil = Int32.MaxValue;

        public bool AppliedScore = false;

        public int DistanceToLast;
        public int DistanceToNext = 1;

        public Duration() { }

        public Duration(T _value, int _time, int _distance = 1)
        {
            Value = _value;
            AppliedScore = false;

            CanArriveAt = _time + _distance - 1;
            MustStayUntil = _time + _distance - 1;

            DistanceToLast = _distance;
        }

        public Duration<T> Duplicate()
        {
            return new Duration<T>()
            {
                Value = Value,
                CanArriveAt = CanArriveAt,
                MustArriveBy = MustArriveBy,
                MustStayUntil = MustStayUntil,
                CanStayUntil = CanStayUntil,
                AppliedScore = AppliedScore,
                DistanceToLast = DistanceToLast,
                DistanceToNext = DistanceToNext
            };
        }
    }
}