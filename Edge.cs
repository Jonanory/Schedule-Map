using System.Collections;
using System.Collections.Generic;

namespace ScheduleMap
{
    public class Edge<T>
    {
        public int Length = 1;
        public Node<T> Destination;
        public List<Bot<T>> BotsTravellingDown = new List<Bot<T>>();

        public Edge(Node<T> _destination, int _length = 1)
        {
            if (_length < 1)
            {
                return;
            }
            Destination = _destination;
            Length = _length;
        }

        public void CheckOnTrappedBots(float scoreRemaining, float scoreToBeat)
        {
            List<Bot<T>> currentBots = new List<Bot<T>>();
            foreach (Bot<T> bot in BotsTravellingDown)
            {
                currentBots.Add(bot);
            }
            foreach (Bot<T> bot in currentBots)
            {
                if (bot.Score + scoreRemaining < scoreToBeat)
                {
                    BotsTravellingDown.Remove(bot);
                }
                else
                {
                    bot.TimeTrappedFor--;
                    if (bot.TimeTrappedFor < 1)
                    {
                        bot.ArriveAt(Destination);
                        BotsTravellingDown.Remove(bot);
                    }
                }
            }
        }
    }
}