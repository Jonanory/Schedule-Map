using System.Collections;
using System.Collections.Generic;

namespace ScheduleMap
{
    public class Map<T>
    {
        Dictionary<T, Node<T>> nodeDictionary = new Dictionary<T, Node<T>>();

        List<Edge<T>> edges = new List<Edge<T>>();

        Bot<T> maxBot = null;

        public float totalPointsAvailable;
        Dictionary<int, float> MaxPossiblePointsRemaining;

        public Map() { }

        public Map(
           List<T> _values,
           Dictionary<T, List<T>> _connections
           )
        {
            AddLocations(_values);
            MakeEdges(_connections);
        }

        public Map(
            List<T> _values,
            Dictionary<T, Dictionary<T, int>> _connections
            )
        {
            AddLocations(_values);
            MakeEdges(_connections);
        }

        void AddLocations(List<T> _values)
        {
            foreach (T value in _values)
            {
                nodeDictionary.Add(value, new Node<T>(value));
            }
        }

        void MakeEdges(Dictionary<T, List<T>> _connections)
        {
            foreach (T startKey in _connections.Keys)
            {
                foreach (T endKey in _connections[startKey])
                {
                    MakeEdge(startKey, endKey, 1);
                }
            }
        }

        void MakeEdges(Dictionary<T, Dictionary<T, int>> _connections)
        {
            foreach (T startKey in _connections.Keys)
            {
                foreach (T endKey in _connections[startKey].Keys)
                {
                    MakeEdge(startKey, endKey, _connections[startKey][endKey]);
                }
            }
        }

        void MakeEdge(T _start, T _end, int _distance = 1)
        {
            if (nodeDictionary.ContainsKey(_start) == false || nodeDictionary.ContainsKey(_end) == false) return;
            Node<T> startNode = nodeDictionary[_start];
            Node<T> endNode = nodeDictionary[_end];
            edges.Add(startNode.AddEdge(endNode, _distance));
        }

        void Reset()
        {
            foreach (Node<T> node in nodeDictionary.Values)
            {
                node.Reset();
            }
            maxBot = null;
        }

        void CreateStartNode(T _startLocation, int _startTime, List<ScheduleItem<T>> _startingScheduleList)
        {
            Node<T> startNode = nodeDictionary[_startLocation];

            Bot<T> startingBot = new Bot<T>(_startLocation)
            {
                currentNode = startNode,
                score = 0
            };
            startNode.freeBot = startingBot;
            startNode.freeBot.AddInstruction(_startLocation, _startTime, true, 0);

            foreach (ScheduleItem<T> item in _startingScheduleList)
            {
                if (EqualityComparer<T>.Default.Equals(item.value, _startLocation))
                {
                    if (item.length > 0)
                    {
                        startNode.TrapCopyOfBot(startingBot, item.length, item.points);
                    }
                }
            }
        }

        public Bot<T> GetBot(ScheduleList<T> _list, T _startLocation, int _startTime = 0, int _pathLength = 8)
        {
            Reset();
            Node<T> startNode = nodeDictionary[_startLocation];

            CreateStartNode(_startLocation, _startTime, _list.GetSchedules(_startTime));

            for (int i = 1; i <= _pathLength; i++)
            {
                // Determine all the bots that are able to move
                List<Bot<T>> oldBots = new List<Bot<T>>();
                foreach (Node<T> node in nodeDictionary.Values)
                {
                    if (node.freeBot != null)
                    {
                        if( maxBot != null && node.freeBot.score + _list.totalPointsAvailable < maxBot.score )
                        {
                            node.freeBot = null;
                        }
                        else
                        {
                            oldBots.Add(node.freeBot);
                        }
                    }
                }

                foreach (Edge<T> edge in edges)
                {
                    edge.CheckOnTrappedBots( _list.totalPointsAvailable, maxBot != null ? maxBot.score : 0f ); 
                }

                foreach (Bot<T> bot in oldBots)
                {
                    foreach (Edge<T> edge in bot.currentNode.edges)
                    {
                        bot.TravelAlong(edge, _startTime + i);
                    }
                }

                foreach (Node<T> node in nodeDictionary.Values)
                {
                    node.CheckOnTrappedBots( _list.totalPointsAvailable, maxBot != null ? maxBot.score : 0f );

                    if (node.freeBot != null)
                    {
                        node.ApplyScore(_list.GetSchedules(_startTime + i), _startTime + i);

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

        public Queue<Instruction<T>> GetInstructions(ScheduleList<T> _list, T _startLocation, int _startTime = 0, int _pathLength = 8)
        {
            Bot<T> bot = GetBot(_list, _startLocation, _startTime, _pathLength);
            return bot.Instructions;
        }

        public Queue<T> GetPath(ScheduleList<T> _list, T _startLocation, int _startTime = 0, int _pathLength = 8)
        {
            Bot<T> bot = GetBot(_list, _startLocation, _startTime, _pathLength);
            return bot.Path;
        }
    }
}