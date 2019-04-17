using System.Collections;
using System.Collections.Generic;

namespace ScheduleMap
{
    public class Node<T>
    {
        public T value;

        public Bot<T> freeBot = null;
        public List<Bot<T>> trappedBot;

        public List<Path<T>> paths = new List<Path<T>>();
        Dictionary<int, List<Time>> schedules = new Dictionary<int, List<Time>>();


        public Node(T _value)
        {
            value = _value;
        }

        public Path<T> AddPath(Node<T> otherNode, int distance = 1)
        {
            Path<T> newPath = new Path<T>(otherNode, distance);
            paths.Add(newPath);
            return newPath;
        }

        public void Reset()
        {
            freeBot = null;
            trappedBot = new List<Bot<T>>();
        }

        public void AddScheduleItem(float preference, int startTime, int length = 0)
        {
            if (schedules.ContainsKey(startTime) == false)
            {
                schedules.Add(startTime, new List<Time>());
            }
            schedules[startTime].Add(new Time(preference, startTime, length));
        }

        public List<Time> GetScheduleData(int time)
        {
            if (schedules.ContainsKey(time))
            {
                return schedules[time];
            }
            return new List<Time>();
        }

        public Bot<T> TrapCopyOfBot(Bot<T> botToTrap, int timeToTrapFor, float reward = 0f)
        {
            if (timeToTrapFor < 1) return null;
            Bot<T> newBot = botToTrap.Duplicate();
            newBot.timeTrappedFor = timeToTrapFor;
            newBot.score += reward;
            trappedBot.Add(newBot);
            return newBot;
        }

        public void CheckOnTrappedBots()
        {
            List<Bot<T>> currentlyTrapped = new List<Bot<T>>();
            foreach (Bot<T> trapped in trappedBot)
            {
                currentlyTrapped.Add(trapped);
            }
            foreach (Bot<T> bot in currentlyTrapped)
            {
                bot.timeTrappedFor--;
                if (bot.timeTrappedFor <= 0)
                {
                    trappedBot.Remove(bot);

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

        public void ApplyScore(List<ScheduleItem<T>> items, int time)
        {
            float maxFreeScore = 0;
            foreach (ScheduleItem<T> item in items)
            {
                if (EqualityComparer<T>.Default.Equals(item.value, value) == false) continue;
                if (item.length > 0)
                {
                    Bot<T> newBot = TrapCopyOfBot(freeBot, item.length, item.points);
                    if (newBot != null) newBot.AddInstruction(value, time, true);
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
                freeBot.AddInstruction(value, time, true);
            }
        }
    }
}