using System.Collections;
using System.Collections.Generic;

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

        public Queue<T> DuplicatePath()
        {
            Queue<T> returnPath = new Queue<T>();
            foreach (T t in Path)
            {
                returnPath.Enqueue(t);
            }
            return returnPath;
        }

        public List<Instruction<T>> DuplicateList()
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

        public void TravelAlong(Path<T> path, int time)
        {
            Bot<T> newBot = Duplicate();
            if (instructionList.Count == 0 ||
                    EqualityComparer<T>.Default.Equals(instructionList[instructionList.Count - 1].value, currentNode.value))
            {
                newBot.AddInstruction(path.destination.value, time, false, path.length);
            }

            if (path.length > 1)
            {
                newBot.timeTrappedFor = path.length - 1;
                path.botsTravellingDown.Add(newBot);
            }
            else if (path.length == 1)
            {
                newBot.ArriveAt(path.destination);
            }
        }

        public Bot<T> ArriveAt(Node<T> node)
        {
            Bot<T> newBot = Duplicate();
            newBot.currentNode = node;

            if (node.freeBot == null)
            {
                node.freeBot = newBot;
            }
            else
            {
                if (newBot.score > node.freeBot.score)
                {
                    node.freeBot = newBot;
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