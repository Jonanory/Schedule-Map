using System.Collections;
using System.Collections.Generic;

namespace ScheduleMap
{
    public class Path<T>
    {
        public int length = 1;
        public Node<T> destination;
        public List<Bot<T>> botsTravellingDown = new List<Bot<T>>();

        public Path(Node<T> _destination, int _length = 1)
        {
            if (_length < 1)
            {
                return;
            }
            destination = _destination;
            length = _length;
        }

        public void CheckOnTrappedBots()
        {
            List<Bot<T>> currentBots = new List<Bot<T>>();
            foreach (Bot<T> bot in botsTravellingDown)
            {
                currentBots.Add(bot);
            }
            foreach (Bot<T> bot in currentBots)
            {
                bot.timeTrappedFor--;
                if (bot.timeTrappedFor < 1)
                {
                    bot.ArriveAt(destination);
                    botsTravellingDown.Remove(bot);
                }
            }
        }
    }
}