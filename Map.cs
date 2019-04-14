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
            Dictionary<T, List<T>> connections,
            List<ScheduleItem<T>> scheduleItems
            )
        {
            AddLocations(values);
            MakePaths(connections);
            AddScheduleItems(scheduleItems);
        }

        public Map(
            List<T> values,
            Dictionary<T, Dictionary<T,int>> connections,
            List<ScheduleItem<T>> scheduleItems
            )
        {
            AddLocations(values);
            MakePaths(connections);
            AddScheduleItems(scheduleItems);
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

        public void AddScheduleItem( ScheduleItem<T> scheduleItem )
        {
            nodeDictionary[scheduleItem.value].AddScheduleItem(scheduleItem.points, scheduleItem.startTime, scheduleItem.length);
        }

        public void MakePaths(Dictionary<T, List<T>> connections)
        {
            foreach (T startKey in connections.Keys)
            {
                foreach( T endKey in connections[startKey])
                {
                    MakePath(startKey, endKey, 1);
                }
            }
        }

        public void MakePaths(Dictionary<T, Dictionary<T,int>> connections)
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
            paths.Add(startNode.AddPath(endNode, distance) );
        }

        public void ResetNodes()
        {
            foreach (Node<T> node in nodeDictionary.Values)
            {
                node.Reset();
            }
        }

        public void CreateStartNode(T startLocation, int startTime )
        {
            ResetNodes();
            Node<T> startNode = nodeDictionary[startLocation];

            Bot<T> startingBot = new Bot<T>(startLocation)
            {
                currentNode = startNode,
                score = 0
            };
            startNode.freeBot = startingBot;

            // If there are any occurrences at the start that take some time to complete,
            // make a copy of the starting bot and trap it
            foreach (Time time in startNode.GetScheduleData(startTime))
            {
                if (time.length > 0)
                {
                    startNode.TrapCopyOfBot(startingBot, time.length, time.points);
                }
            }
        }

        public Queue<T> GetRoute(T startLocation, int startTime = 0, int pathLength = 8)
        {
            ResetNodes();
            Node<T> startNode = nodeDictionary[startLocation];

            CreateStartNode( startLocation, startTime);

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
                    foreach( Path<T> path in bot.currentNode.paths)
                    {
                        bot.TravelAlong(path);
                    }
                }

                foreach (Node<T> node in nodeDictionary.Values)
                {
                    node.CheckOnTrappedBots();   

                    if (node.freeBot != null)
                    {
                        node.ApplyScore(startTime+i);

                        if (maxBot == null || node.freeBot.score > maxBot.score)
                        {
                            maxBot = node.freeBot;
                        }
                    }
                }
            }

            return maxBot.path;
        }
    }
}