using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class Interpreter
    {
        private Dictionary<int, int> labels = new Dictionary<int, int>();
        private List<string> code = new List<string>();
        Dictionary<string, (Type type, object value)> memory = new Dictionary<string, (Type type, object value)>();
        Stack<(Type type, object value)> stack = new Stack<(Type type, object value)>();

        public Interpreter(string filename)
        {
            var input = File.ReadAllLines(filename);
            int i = 0;
            foreach (var line in input)
            {
                code.Add(line);
                if(line.StartsWith("label"))
                {
                    var split = line.Split(' ');
                    var index = int.Parse(split[1]);

                    labels.Add(index, i);
                }
                i++;
            }
        }
        private void Load(string name)
        {
            if(memory.ContainsKey(name))
            {
                stack.Push(memory[name]);
                //return memory[name];
            }else
                throw new Exception($"Variable {name} was not initialized");
        }
        private void Save(string name)
        {
            var value = stack.Pop();
            memory[name] = value;
        }
        private void Print(int n)
        {
            List<object> items = new List<object>();
            for (int i = 0; i < n; i++)
            {
                items.Add(stack.Pop().value);
            }
            items.Reverse();
            foreach(var item in items)
            {
                Console.Write(item);
            }
            Console.Write("\n");
        }
        private void Read(string type)
        {
            string value = Console.ReadLine();
            switch (type)
            {
                case "I":
                    {
                        int output;
                        if (int.TryParse(value, out output))
                           stack.Push((Type.Int, output));
                        else
                            throw new Exception($"Read value isnt of expected type");
                    }
                    break;
                case "F":
                    {
                        float output;
                        if (float.TryParse(value, out output))
                            stack.Push((Type.Float, output));
                        else
                            throw new Exception($"Read value isnt of expected type");
                    }
                    break;
                case "B":
                    {
                        if (value.Equals("true"))
                            stack.Push((Type.Boolean, true));
                        else if (value.Equals("false"))
                            stack.Push((Type.Boolean, false));
                        else
                            throw new Exception($"Read value isnt of expected type");

                    }
                    break;
                case "S":
                    {
                        stack.Push((Type.String, value.Replace("\"", String.Empty)));
                    }
                    break;
            }
        }
        private void Push(string type, string value)
        {
            switch (type)
            {
                case "I":
                    {
                        int output;
                        if (int.TryParse(value, out output))
                            stack.Push((Type.Int, output));
                        else
                            throw new Exception($"Pushed value isnt of expected type");
                    }
                    break;
                case "F":
                    {
                        float output;
                        if (float.TryParse(value, out output))
                            stack.Push((Type.Float, output));
                        else
                            throw new Exception($"Pushed value isnt of expected type");
                    }
                    break;
                case "B":
                    {
                        if (value.Equals("true"))
                            stack.Push((Type.Boolean, true));
                        else if (value.Equals("false"))
                            stack.Push((Type.Boolean, false));
                        else
                            throw new Exception($"Pushed value isnt of expected type");

                    }
                    break;
                case "S":
                    {
                        stack.Push((Type.String, value.Replace("\"", String.Empty)));
                    }
                    break;
            }
        }
        private void Itof()
        {
            var value = stack.Pop();

            if (value.type == Type.Int)
                stack.Push((Type.Float, (float)(Convert.ToInt32(value.value))));
            else
                stack.Push((Type.Float, value.value));
        }
        private void Not()
        {
            (Type type, object value) value = stack.Pop();
            stack.Push((Type.Boolean, !(bool)value.value));
        }
        private void Eq()
        {
            (Type type, object value) rightValue = stack.Pop();
            (Type type, object value) leftValue = stack.Pop();
            switch (leftValue.type)
            {
                case Type.Int:
                    {
                        stack.Push((Type.Boolean, (int)leftValue.value == (int)rightValue.value));
                    }
                    break;
                case Type.String:
                    {
                        stack.Push((Type.Boolean, (string)leftValue.value == (string)rightValue.value));
                    }
                    break;
                case Type.Float:
                    {
                        stack.Push((Type.Boolean, (float)leftValue.value == (float)rightValue.value));
                    }
                    break;
            }
        }
        private void Lt()
        {
            (Type type, object value) rightValue = stack.Pop();
            (Type type, object value) leftValue = stack.Pop();
            switch (leftValue.type)
            {
                case Type.Int:
                    {
                        stack.Push((Type.Boolean, (int)leftValue.value < (int)rightValue.value));
                    }
                    break;
                case Type.Float:
                    {
                        stack.Push((Type.Boolean, (float)leftValue.value > (float)rightValue.value));
                    }
                    break;
            }
        }
        private void Gt()
        {
            (Type type, object value) rightValue = stack.Pop();
            (Type type, object value) leftValue = stack.Pop();
            switch (leftValue.type)
            {
                case Type.Int:
                    {
                        stack.Push((Type.Boolean, (int)leftValue.value > (int)rightValue.value));
                    }
                    break;
                case Type.Float:
                    {
                        stack.Push((Type.Boolean, (float)leftValue.value > (float)rightValue.value));
                    }
                    break;
            }
        }
        private void Or()
        {
            (Type type, object value) rightValue = stack.Pop();
            (Type type, object value) leftValue = stack.Pop();
            switch (leftValue.type)
            {
                case Type.Boolean:
                    {
                        stack.Push((Type.Boolean, (bool)leftValue.value || (bool)rightValue.value));
                    }
                    break;
            }
        }
        private void And()
        {
            (Type type, object value) rightValue = stack.Pop();
            (Type type, object value) leftValue = stack.Pop();
            switch (leftValue.type)
            {
                case Type.Boolean:
                    {
                        stack.Push((Type.Boolean, (bool)leftValue.value && (bool)rightValue.value));
                    }
                    break;
            }
        }
        private void Concat()
        {
            (Type type, object value) rightValue = stack.Pop();
            (Type type, object value) leftValue = stack.Pop();
            switch (leftValue.type)
            {
                case Type.String:
                    {
                        stack.Push((Type.String, (string)leftValue.value + (string)rightValue.value));
                    }
                    break;
            }
        }
        private void Uminus()
        {
            (Type type, object value) value = stack.Pop();
            switch (value.type)
            {
                case Type.Int:
                    {
                        stack.Push((Type.Int, -(int)value.value));
                    }
                    break;
                case Type.Float:
                    {
                        stack.Push((Type.Float, -(float)value.value));
                    }
                    break;
            }
        }
        private void Mod()
        {
            (Type type, object value) rightValue = stack.Pop();
            (Type type, object value) leftValue = stack.Pop();

            stack.Push((Type.Int, (int)leftValue.value % (int)rightValue.value));

        }
        private void Div()
        {
            (Type type, object value) rightValue = stack.Pop();
            (Type type, object value) leftValue = stack.Pop();
            switch (leftValue.type)
            {
                case Type.Float:
                    {
                        stack.Push((Type.Float, (float)leftValue.value / (float)rightValue.value));
                    }
                    break;
                case Type.Int:
                    {
                        stack.Push((Type.Int, (int)leftValue.value / (int)rightValue.value));
                    }
                    break;
            }
        }
        private void Mul()
        {
            (Type type, object value) rightValue = stack.Pop();
            (Type type, object value) leftValue = stack.Pop();
            switch (leftValue.type)
            {
                case Type.Float:
                    {
                        stack.Push((Type.Float, (float)leftValue.value * (float)rightValue.value));
                    }
                    break;
                case Type.Int:
                    {
                        stack.Push((Type.Int, (int)leftValue.value * (int)rightValue.value));
                    }
                    break;
            }
        }
        private void Add()
        {
            (Type type, object value) rightValue = stack.Pop();
            (Type type, object value) leftValue = stack.Pop();
            switch (leftValue.type)
            {
                case Type.Float:
                    {
                        stack.Push((Type.Float, (float)leftValue.value + (float)rightValue.value));
                    }
                    break;
                case Type.Int:
                    {
                        stack.Push((Type.Int, (int)leftValue.value + (int)rightValue.value));
                    }
                    break;
            }
        }
        private void Sub()
        {
            (Type type, object value) rightValue = stack.Pop();
            (Type type, object value) leftValue = stack.Pop();
            switch (leftValue.type)
            {
                case Type.Float:
                    {
                        stack.Push((Type.Float, (float)leftValue.value - (float)rightValue.value));
                    }
                    break;
                case Type.Int:
                    {
                        stack.Push((Type.Int, (int)leftValue.value - (int)rightValue.value));
                    }
                    break;
            }
        }
        public void Run()
        {
            int actual = 0;
            while(actual < code.Count)
            {
                var command = code[actual].Split(" ");
                if(command[0].Equals("jmp"))
                {
                    var to = int.Parse(command[1]);
                    actual = labels[to];
                }
                else if (command[0].Equals("fjmp"))
                {
                    var value = stack.Pop();
                    if((bool)value.value)
                    {
                        actual++;
                    }
                    else
                    {
                        actual = labels[int.Parse(command[1])];
                    }
                }
                else if (command[0].Equals("label"))
                {
                    actual++;
                }else if (command[0].Equals("load"))
                {
                    Load(command[1]);
                    actual++;
                }else if (command[0].Equals("save"))
                {
                    Save(command[1]);
                    actual++;
                }else if (command[0].Equals("print"))
                {
                    var n = int.Parse(command[1]);
                    Print(n);
                    actual++;
                }else if (command[0].Equals("read"))
                {
                    var type = command[1];
                    Read(type);
                    actual++;
                }else if (command[0].Equals("pop"))
                {
                    stack.Pop();
                    actual++;
                }else if (command[0].Equals("push"))
                {
                    var type = command[1];
                    var obj = command[2];
                    if(type == "S")
                        for (int i = 3; i < command.Length; i++)
                            obj += " " + command[i];
                    Push(type, obj);
                    actual++;
                }else if (command[0].Equals("itof"))
                {
                    Itof();
                    actual++;
                }else if (command[0].Equals("not"))
                {
                    Not();
                    actual++;
                }else if (command[0].Equals("eq"))
                {
                    Eq();
                    actual++;
                }else if (command[0].Equals("lt"))
                {
                    Lt();
                    actual++;
                }else if (command[0].Equals("gt"))
                {
                    Gt();
                    actual++;
                }else if (command[0].Equals("or"))
                {
                    Or();
                    actual++;
                }else if (command[0].Equals("and"))
                {
                    And();
                    actual++;
                }else if (command[0].Equals("concat"))
                {
                    Concat();
                    actual++;
                }else if (command[0].Equals("uminus"))
                {
                    Uminus();
                    actual++;
                }else if (command[0].Equals("mod"))
                {
                    Mod();
                    actual++;
                }else if (command[0].Equals("div"))
                {
                    Div();
                    actual++;
                }else if (command[0].Equals("mul"))
                {
                    Mul();
                    actual++;
                }else if (command[0].Equals("sub"))
                {
                    Sub();
                    actual++;
                }else if (command[0].Equals("add"))
                {
                    Add();
                    actual++;
                }
            }
        }
    }
}
