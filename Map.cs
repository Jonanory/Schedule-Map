using System.Collections;
using System.Collections.Generic;

namespace ScheduleMap
{
    public class Map<T>
    {
        Dictionary<T, Node<T>> NodeDictionary = new Dictionary<T, Node<T>>();

        List<Edge<T>> Edges = new List<Edge<T>>();

        Bot<T> MaxBot = null;

        public float TotalPointsAvailable;
        Dictionary<int, float> MaxPossiblePointsRemaining;

        bool Discounting = false;
        float DiscountRate = 1f;

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
            Dictionary<T, List<T>> _connections,
            float _discountRate = 1f
            ) : this(_values, _connections)
        {
            SetDiscountRate(_discountRate);
        }

        public Map(
            List<T> _values,
            Dictionary<T, Dictionary<T, int>> _connections
            )
        {
            AddLocations(_values);
            MakeEdges(_connections);
        }

        public Map(
            List<T> _values,
            Dictionary<T, Dictionary<T, int>> _connections,
            float _discountRate = 1f
            ) : this(_values, _connections)
        {
            SetDiscountRate(_discountRate);
        }

        void AddLocations(List<T> _values)
        {
            foreach (T value in _values)
            {
                NodeDictionary.Add(value, new Node<T>(value));
            }
        }

        void SetDiscountRate(float _discountRate = 1f)
        {
            if (_discountRate != 1f)
            {
                DiscountRate = _discountRate;
                Discounting = true;
            }
            else
            {
                DiscountRate = 1f;
                Discounting = false;
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
            if (NodeDictionary.ContainsKey(_start) == false || NodeDictionary.ContainsKey(_end) == false) return;
            Node<T> startNode = NodeDictionary[_start];
            Node<T> endNode = NodeDictionary[_end];
            Edges.Add(startNode.AddEdge(endNode, _distance));
        }

        void Reset()
        {
            foreach (Node<T> node in NodeDictionary.Values)
            {
                node.Reset();
            }
            MaxBot = null;
        }

        void CreateStartNode(T _startLocation, int _startTime, List<ScheduleItem<T>> _startingScheduleList)
        {
            Node<T> startNode = NodeDictionary[_startLocation];

            Bot<T> startingBot = new Bot<T>(_startLocation)
            {
                CurrentNode = startNode,
                Score = 0
            };
            startNode.FreeBot = startingBot;
            startNode.FreeBot.AddDuration(_startLocation, _startTime, 1);

            foreach (ScheduleItem<T> item in _startingScheduleList)
            {
                if (EqualityComparer<T>.Default.Equals(item.Value, _startLocation))
                {
                    if (item.Length > 0)
                    {
                        startNode.TrapCopyOfBot(startingBot, item.Length, item.Points);
                    }
                }
            }
        }

        public Bot<T> GetBot(ScheduleList<T> _list, T _startLocation, int _startTime = 0, int _pathLength = 8)
        {
            Reset();

            float Discount = 1f;

            _list.ClampTime(_startTime, _startTime + _pathLength);

            CreateStartNode(_startLocation, _startTime, _list.GetSchedules(_startTime));

            for (int i = 1; i <= _pathLength; i++)
            {
                // Determine all the bots that are able to move
                List<Bot<T>> oldBots = new List<Bot<T>>();
                foreach (Node<T> node in NodeDictionary.Values)
                {
                    if (node.FreeBot != null)
                    {
                        if (MaxBot != null && node.FreeBot.Score + _list.TotalPointsAvailable < MaxBot.Score)
                        {
                            node.FreeBot = null;
                        }
                        else
                        {
                            oldBots.Add(node.FreeBot);
                        }
                    }
                }

                foreach (Edge<T> edge in Edges)
                {
                    edge.CheckOnTrappedBots(_list.TotalPointsAvailable, MaxBot != null ? MaxBot.Score : 0f);
                }

                foreach (Bot<T> bot in oldBots)
                {
                    foreach (Edge<T> edge in bot.CurrentNode.Edges)
                    {
                        bot.TravelAlong(edge, _startTime + i);
                    }
                }

                foreach (Node<T> node in NodeDictionary.Values)
                {
                    node.CheckOnTrappedBots(_list.TotalPointsAvailable, MaxBot != null ? MaxBot.Score : 0f);

                    if (node.FreeBot != null)
                    {
                        node.ApplyScore(_list.GetSchedules(_startTime + i), _startTime + i, Discount);

                        if (MaxBot == null || node.FreeBot.Score > MaxBot.Score)
                        {
                            MaxBot = node.FreeBot;
                        }
                    }
                }

                Discount *= DiscountRate;
            }

            MaxBot.MakeDurations();
            MaxBot.MakeInstructions();
            MaxBot.MakeScheduleItems();
            MaxBot.MakePath();
            return MaxBot;
        }

        public Queue<Duration<T>> GetDurations(ScheduleList<T> _list, T _startLocation, int _startTime = 0, int _pathLength = 8)
        {
            Bot<T> bot = GetBot(_list, _startLocation, _startTime, _pathLength);
            return bot.Durations;
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