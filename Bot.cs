using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleMap
{
    public class Bot<T>
    {
        public float score = -1;
        public int timeTrappedFor = 0;
        public Node<T> currentNode;

        public Queue<T> Path { get; set; }
        public Queue<Instruction<T>> Instructions { get; set; }
        List<Instruction<T>> instructionList;


        public Bot() { }
        public Bot(T startLocation)
        {
            Path = new Queue<T>();
            Instructions = new Queue<Instruction<T>>();
            instructionList = new List<Instruction<T>>();
        }

        Queue<T> DuplicatePath()
        {
            Queue<T> returnPath = new Queue<T>();
            foreach (T t in Path)
            {
                returnPath.Enqueue(t);
            }
            return returnPath;
        }

        List<Instruction<T>> DuplicateList()
        {
            List<Instruction<T>> final = new List<Instruction<T>>();

            foreach (Instruction<T> t in instructionList)
            {
                final.Add(new Instruction<T>(t.value, t.time, t.score, t.distance));
            }
            return final;
        }

        public Bot<T> Duplicate()
        {
            return new Bot<T>
            {
                score = score,
                Path = DuplicatePath(),
                instructionList = DuplicateList(),
                currentNode = currentNode
            };
        }

        public void CreatePath()
        {
            if (instructionList.Count > 0)
            {
                Path = new Queue<T>();
                int count = 0;
                for (int i = 0; i < instructionList.Count - 1; i++)
                {
                    while (count < instructionList[i + 1].time)
                    {
                        Path.Enqueue(instructionList[i].value);
                        count++;
                    }
                }
                Path.Enqueue(instructionList[instructionList.Count - 1].value);
            }
        }

        public void CreateInstructionQueue()
        {
            Instructions = new Queue<Instruction<T>>();
            foreach( Instruction<T> instruction in instructionList )
            {
                Instructions.Enqueue(instruction);
            }
        }

        public void TravelAlong(Edge<T> _edge, int _time)
        {
            Bot<T> newBot = Duplicate();
            if (instructionList.Count == 0 ||
                    EqualityComparer<T>.Default.Equals(instructionList[instructionList.Count - 1].value, currentNode.value))
            {
                newBot.AddInstruction(_edge.destination.value, _time, false, _edge.length);
            }

            if (_edge.length > 1)
            {
                newBot.timeTrappedFor = _edge.length - 1;
                _edge.botsTravellingDown.Add(newBot);
            }
            else if (_edge.length == 1)
            {
                newBot.ArriveAt(_edge.destination);
            }
        }

        public Bot<T> ArriveAt(Node<T> _node)
        {
            Bot<T> newBot = Duplicate();
            newBot.currentNode = _node;

            if (_node.freeBot == null)
            {
                _node.freeBot = newBot;
            }
            else
            {
                if (newBot.score > _node.freeBot.score)
                {
                    _node.freeBot = newBot;
                }
            }
            return newBot;
        }


        public void AddInstruction(T _value, int _time, bool _score, int _distance = 1)
        {
            if (instructionList.Count == 0)
            {
                Instruction<T> newInstruction = new Instruction<T>(_value, _time, _score, _distance);
                instructionList.Add(newInstruction);
            }
            else if (EqualityComparer<T>.Default.Equals(instructionList[instructionList.Count - 1].value, _value) == false)
            {
                Instruction<T> newInstruction = new Instruction<T>(_value, _time, _score, _distance);
                instructionList.Add(newInstruction);
            }
            else if (instructionList[instructionList.Count - 1].score == false && _score == true)
            {
                int lastIndex = instructionList.Count - 1;
                instructionList[lastIndex].score = true;
                int routeReduction = -1;
                do
                {
                    routeReduction += instructionList[lastIndex].distance;
                    instructionList[lastIndex].time = _time - routeReduction;
                    lastIndex--;
                } while (lastIndex >= 0 && instructionList[lastIndex].score == false);
            }
        }
    }

    public class Instruction<T>
    {
        public T value;
        public int time;
        public bool score;
        public int distance = 1;

        public Instruction(T _value, int _time, bool _score, int _distance)
        {
            value = _value;
            time = _time;
            score = _score;
            distance = _distance;
        }

        public Instruction(T _value, int _time, bool _score)
        {
            value = _value;
            time = _time;
            score = _score;
        }
    }
}