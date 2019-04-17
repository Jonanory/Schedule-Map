using System.Collections;
using System.Collections.Generic;

namespace ScheduleMap
{
    public class Map<T>
    {
        Dictionary<T, Node<T>> nodeDictionary = new Dictionary<T, Node<T>>();

        public List<Path<T>> paths = new List<Path<T>>();

        public Bot<T> maxBot = null;

        public Map() { }

        public Map(
           List<T> values,
           Dictionary<T, List<T>> connections
           )
        {
            AddLocations(values);
            MakePaths(connections);
        }

        public Map(
            List<T> values,
            Dictionary<T, Dictionary<T, int>> connections
            )
        {
            AddLocations(values);
            MakePaths(connections);
        }

        public void AddLocations(List<T> values)
        {
            foreach (T value in values)
            {
                nodeDictionary.Add(value, new Node<T>(value));
            }
        }

        public void AddScheduleItems(List<ScheduleItem<T>> scheduleItems)
        {
            foreach (ScheduleItem<T> item in scheduleItems)
            {
                AddScheduleItem(item);
            }
        }

        public void AddScheduleItem(ScheduleItem<T> scheduleItem)
        {
            nodeDictionary[scheduleItem.value].AddScheduleItem(scheduleItem.points, scheduleItem.startTime, scheduleItem.length);
        }

        public void MakePaths(Dictionary<T, List<T>> connections)
        {
            foreach (T startKey in connections.Keys)
            {
                foreach (T endKey in connections[startKey])
                {
                    MakePath(startKey, endKey, 1);
                }
            }
        }

        public void MakePaths(Dictionary<T, Dictionary<T, int>> connections)
        {
            foreach (T startKey in connections.Keys)
            {
                foreach (T endKey in connections[startKey].Keys)
                {
                    MakePath(startKey, endKey, connections[startKey][endKey]);
                }
            }
        }

        void MakePath(T start, T end, int distance = 1)
        {
            if (nodeDictionary.ContainsKey(start) == false || nodeDictionary.ContainsKey(end) == false) return;
            Node<T> startNode = nodeDictionary[start];
            Node<T> endNode = nodeDictionary[end];
            paths.Add(startNode.AddPath(endNode, distance));
        }

        public void Reset()
        {
            foreach (Node<T> node in nodeDictionary.Values)
            {
                node.Reset();
            }
            maxBot = null;
        }

        public void CreateStartNode(T startLocation, int startTime, List<ScheduleItem<T>> startingScheduleList)
        {
            Node<T> startNode = nodeDictionary[startLocation];

            Bot<T> startingBot = new Bot<T>(startLocation)
            {
                currentNode = startNode,
                score = 0
            };
            startNode.freeBot = startingBot;
            startNode.freeBot.AddInstruction(startLocation, startTime, true, 0);

            foreach (ScheduleItem<T> item in startingScheduleList)
            {
                if (EqualityComparer<T>.Default.Equals(item.value, startLocation))
                {
                    if (item.length > 0)
                    {
                        startNode.TrapCopyOfBot(startingBot, item.length, item.points);
                    }
                }
            }
        }

        public Bot<T> GetBot(ScheduleList<T> list, T startLocation, int startTime = 0, int pathLength = 8)
        {
            Reset();
            Node<T> startNode = nodeDictionary[startLocation];

            CreateStartNode(startLocation, startTime, list.GetSchedules(startTime));

            for (int i = 1; i <= pathLength; i++)
            {
                // Determine all the bots that are able to move
                List<Bot<T>> oldBots = new List<Bot<T>>();
                foreach (Node<T> node in nodeDictionary.Values)
                {
                    if (node.freeBot != null)
                    {
                        oldBots.Add(node.freeBot);
                    }
                }

                foreach (Path<T> path in paths)
                {
                    path.CheckOnTrappedBots();
                }

                foreach (Bot<T> bot in oldBots)
                {
                    foreach (Path<T> path in bot.currentNode.paths)
                    {
                        bot.TravelAlong(path, startTime + i);
                    }
                }

                foreach (Node<T> node in nodeDictionary.Values)
                {
                    node.CheckOnTrappedBots();

                    if (node.freeBot != null)
                    {
                        node.ApplyScore(list.GetSchedules(startTime + i), startTime + i);

                        if (maxBot == null || node.freeBot.score > maxBot.score)
                        {
                            maxBot = node.freeBot;
                        }
                    }
                }
            }

            maxBot.CreateInstructionQueue();
            maxBot.CreatePath();
            return maxBot;
        }

        public Queue<Instruction<T>> GetInstructions(ScheduleList<T> list, T startLocation, int startTime = 0, int pathLength = 8)
        {
            Bot<T> bot = GetBot(list, startLocation, startTime, pathLength);
            return bot.Instructions;
        }

        public Queue<T> GetPath(ScheduleList<T> list, T startLocation, int startTime = 0, int pathLength = 8)
        {
            Bot<T> bot = GetBot(list, startLocation, startTime, pathLength);
            return bot.Path;
        }
    }
}