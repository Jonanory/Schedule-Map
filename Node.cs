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

        public Path<T> AddPath( Node<T> otherNode, int distance = 1 )
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

        public void TrapCopyOfBot(Bot<T> botToTrap, int timeToTrapFor, float reward = 0f )
        {
            if (timeToTrapFor < 1) return;
            Bot<T> newBot = botToTrap.Duplicate();
            newBot.timeTrappedFor = timeToTrapFor;
            newBot.score += reward;
            trappedBot.Add(newBot);
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
                else
                {
                    bot.path.Enqueue(value);
                }
            }
        }

        public void ApplyScore( int currentTime )
        {
            freeBot.path.Enqueue(value);

            float maxFreeScore = 0;
            foreach (Time time in GetScheduleData(currentTime))
            {
                if (time.length > 0)
                {
                    TrapCopyOfBot(freeBot, time.length, time.points);
                }
                else
                {
                    if (time.points > maxFreeScore)
                    {
                        maxFreeScore = time.points;
                    }
                }
            }

            // Clear up memory
            schedules.Remove(currentTime);

            freeBot.score += maxFreeScore;
        }
    }
}