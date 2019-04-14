using System.Collections;
using System.Collections.Generic;

namespace ScheduleMap
{
    public class Bot<T>
    {
        public float score = -1;
        public int timeTrappedFor = 0;
        public Queue<T> path;
        public Node<T> currentNode;


        public Bot() { }
        public Bot(T startLocation)
        {
            path = new Queue<T>();
        }

        public Queue<T> DuplicatePath()
        {
            Queue<T> returnPath = new Queue<T>();
            foreach (T t in path)
            {
                returnPath.Enqueue(t);
            }
            return returnPath;
        }

        public Bot<T> Duplicate()
        {
            return new Bot<T>
            {
                score = score,
                path = DuplicatePath(),
                currentNode = currentNode
            };
        }

        public void TravelAlong(Path<T> path)
        {
            if (path.length > 1)
            {
                Bot<T> newBot = Duplicate();
                newBot.timeTrappedFor = path.length-1;
                path.botsTravellingDown.Add(newBot);
            }
            else if (path.length == 1)
            {
                ArriveAt(path.destination);
            }
        }

        public void ArriveAt(Node<T> node)
        {
            Bot<T> newBot = Duplicate();
            newBot.currentNode = node;

            if (node.freeBot == null || newBot.score > node.freeBot.score)
            {
                node.freeBot = newBot;
            }
        }
    }
}