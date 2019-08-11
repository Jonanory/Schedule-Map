using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleMap
{
    public class Node<T>
    {
        public T Value;

        public Bot<T> FreeBot = null;
        List<Bot<T>> TrappedBots;

        public List<Edge<T>> Edges = new List<Edge<T>>();
        Dictionary<int, List<Time>> Schedules = new Dictionary<int, List<Time>>();


        public Node(T _value)
        {
            Value = _value;
        }

        public Edge<T> AddEdge(Node<T> _otherNode, int _distance = 1)
        {
            Edge<T> newEdge = new Edge<T>(_otherNode, _distance);
            Edges.Add(newEdge);
            return newEdge;
        }

        public void Reset()
        {
            FreeBot = null;
            TrappedBots = new List<Bot<T>>();
        }

        public void AddScheduleItem(float _preference, int _startTime, int _length = 0)
        {
            if (Schedules.ContainsKey(_startTime) == false)
            {
                Schedules.Add(_startTime, new List<Time>());
            }
            Schedules[_startTime].Add(new Time(_preference, _startTime, _length));
        }

        List<Time> GetScheduleData(int _time)
        {
            if (Schedules.ContainsKey(_time))
            {
                return Schedules[_time];
            }
            return new List<Time>();
        }

        public Bot<T> TrapCopyOfBot(Bot<T> _botToTrap, int _timeToTrapFor, float _reward = 0f)
        {
            if (_timeToTrapFor < 1) return null;
            Bot<T> newBot = _botToTrap.Duplicate();
            newBot.TimeTrappedFor = _timeToTrapFor;
            newBot.Score += _reward;
            TrappedBots.Add(newBot);
            return newBot;
        }

        public void CheckOnTrappedBots(float scoreRemaining, float scoreToBeat)
        {
            List<Bot<T>> currentlyTrapped = new List<Bot<T>>();
            foreach (Bot<T> trapped in TrappedBots)
            {
                currentlyTrapped.Add(trapped);
            }
            foreach (Bot<T> bot in currentlyTrapped)
            {
                if (bot.Score + scoreRemaining < scoreToBeat)
                {
                    TrappedBots.Remove(bot);
                }
                else
                {
                    bot.TimeTrappedFor--;
                    if (bot.TimeTrappedFor <= 0)
                    {
                        TrappedBots.Remove(bot);

                        if (FreeBot == null)
                        {
                            FreeBot = bot;
                        }
                        else if (bot.Score > FreeBot.Score)
                        {
                            FreeBot = bot;
                        }
                    }
                }
            }
        }

        public void ApplyScore(List<ScheduleItem<T>> _items, int _time, float _discount = 1f)
        {
            bool discounting = _discount == 1f;
            float maxFreeScore = 0;
            ScheduleItem<T> maxFreeScheduleItem = null;
            foreach (ScheduleItem<T> item in _items)
            {
                if (EqualityComparer<T>.Default.Equals(item.Value, Value) == false) continue;
                if (item.Length > 0)
                {
                    Bot<T> newBot = TrapCopyOfBot(FreeBot, item.Length, item.Points);
                    if (newBot != null)
                    {
                        newBot.ApplyScoreToDuration(Value, _time, item.Length);
                        newBot.AddScheduleItem(item);
                    }
                }
                else
                {
                    if (item.Points > maxFreeScore)
                    {
                        maxFreeScore = item.Points;
                        maxFreeScheduleItem = item;
                    }
                }
            }

            if (discounting)
            {
                FreeBot.Score += maxFreeScore * _discount;
            }
            else
            {
                FreeBot.Score += maxFreeScore;
            }

            if (maxFreeScore > 0)
            {
                FreeBot.ApplyScoreToDuration(Value, _time, 0);
                FreeBot.AddScheduleItem(maxFreeScheduleItem);
            }
        }
    }
}