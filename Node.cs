using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleMap
{
    public class Node<T>
    {
        public T value;

        public Bot<T> freeBot = null;
        List<Bot<T>> trappedBots;

        public List<Edge<T>> edges = new List<Edge<T>>();
        Dictionary<int, List<Time>> schedules = new Dictionary<int, List<Time>>();


        public Node(T _value)
        {
            value = _value;
        }

        public Edge<T> AddEdge(Node<T> _otherNode, int _distance = 1)
        {
            Edge<T> newEdge = new Edge<T>(_otherNode, _distance);
            edges.Add(newEdge);
            return newEdge;
        }

        public void Reset()
        {
            freeBot = null;
            trappedBots = new List<Bot<T>>();
        }

        public void AddScheduleItem(float _preference, int _startTime, int _length = 0)
        {
            if (schedules.ContainsKey(_startTime) == false)
            {
                schedules.Add(_startTime, new List<Time>());
            }
            schedules[_startTime].Add(new Time(_preference, _startTime, _length));
        }

        List<Time> GetScheduleData(int _time)
        {
            if (schedules.ContainsKey(_time))
            {
                return schedules[_time];
            }
            return new List<Time>();
        }

        public Bot<T> TrapCopyOfBot(Bot<T> _botToTrap, int _timeToTrapFor, float _reward = 0f)
        {
            if (_timeToTrapFor < 1) return null;
            Bot<T> newBot = _botToTrap.Duplicate();
            newBot.timeTrappedFor = _timeToTrapFor;
            newBot.score += _reward;
            trappedBots.Add(newBot);
            return newBot;
        }

        public void CheckOnTrappedBots(float scoreRemaining, float scoreToBeat)
        {
            List<Bot<T>> currentlyTrapped = new List<Bot<T>>();
            foreach (Bot<T> trapped in trappedBots)
            {
                currentlyTrapped.Add(trapped);
            }
            foreach (Bot<T> bot in currentlyTrapped)
            {
                if( bot.score + scoreRemaining < scoreToBeat )
                {
                    trappedBots.Remove(bot);
                }
                else
                {
                    bot.timeTrappedFor--;
                    if (bot.timeTrappedFor <= 0)
                    {
                        trappedBots.Remove(bot);

                        if (freeBot == null)
                        {
                            freeBot = bot;
                        }
                        else if (bot.score > freeBot.score)
                        {
                            freeBot = bot;
                        }
                    }
                }
            }
        }

        public void ApplyScore(List<ScheduleItem<T>> _items, int _time)
        {
            float maxFreeScore = 0;
            foreach (ScheduleItem<T> item in _items)
            {
                if (EqualityComparer<T>.Default.Equals(item.value, value) == false) continue;
                if (item.length > 0)
                {
                    Bot<T> newBot = TrapCopyOfBot(freeBot, item.length, item.points);
                    if (newBot != null) newBot.AddInstruction(value, _time, true);
                }
                else
                {
                    if (item.points > maxFreeScore)
                    {
                        maxFreeScore = item.points;
                    }
                }
            }

            freeBot.score += maxFreeScore;

            if (maxFreeScore > 0)
            {
                freeBot.AddInstruction(value, _time, true);
            }
        }
    }
}