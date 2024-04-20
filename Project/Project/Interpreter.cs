using System;
using System.Collections.Generic;
using System.IO;

namespace PJP_Project
{
    public class Interpreter
    {
        private Dictionary<int, int> labels = new();
        private List<string> code = new();
        Dictionary<string, (Type type, object value)> memory = new();
        Stack<(Type type, object value)> stack = new();
        public Interpreter(string filename)
        {
            var lines = File.ReadAllLines(filename);
            int i = 0;
            foreach (var line in lines)
            {
                code.Add(line);
                if (line.StartsWith("label"))
                {
                    var index = int.Parse(line.Split(' ')[1]);
                    labels.Add(index, i);
                }
                i++;
            }
        }
        public void Run()
        {
            int current = 0;
            while (current < code.Count)
            {
                var command = code[current].Split(" ");

                if (command[0] == "jmp")
                {
                    var to = int.Parse(command[1]);
                    current = labels[to];
                }
                else if (command[0] == "fjmp")
                {
                    var value = stack.Pop();
                    if ((bool)value.value)
                    {
                        current++;
                    }
                    else
                    {
                        current = labels[int.Parse(command[1])];
                    }
                }
                else if (command[0] == "tjmp")
                {
                    var value = stack.Pop();
                    if ((bool)value.value)
                    {
                        current = labels[int.Parse(command[1])];
                    }
                    else
                    {
                        current++;
                    }
                }
                else
                {
                    switch (command[0])
                    {
                        case "label":
                            break;
                        case "load":
                            Load(command[1]);
                            break;
                        case "save":
                            Save(command[1]);
                            break;
                        case "print":
                            var n = int.Parse(command[1]);
                            Print(n);
                            break;
                        case "read":
                            var type = command[1];
                            Read(type);
                            break;
                        case "pop":
                            stack.Pop();
                            break;
                        case "push":
                            type = command[1];
                            var obj = command[2];
                            if (type == "S")
                                for (int i = 3; i < command.Length; i++)
                                    obj += " " + command[i];
                            Push(type, obj);
                            break;
                        case "itof":
                            Itof();
                            break;
                        case "not":
                            Not();
                            break;
                        case "eq":
                            Eq();
                            break;
                        case "lt":
                            Lt();
                            break;
                        case "gt":
                            Gt();
                            break;
                        case "or":
                            Or();
                            break;
                        case "and":
                            And();
                            break;
                        case "concat":
                            Concat();
                            break;
                        case "uminus":
                            Uminus();
                            break;
                        case "mod":
                            Mod();
                            break;
                        case "div":
                            Div();
                            break;
                        case "mul":
                            Mul();
                            break;
                        case "sub":
                            Sub();
                            break;
                        case "add":
                            Add();
                            break;
                    }
                    current++;
                } 
            }
        }

        private void Load(string name)
        {
            if(memory.ContainsKey(name))
            {
                stack.Push(memory[name]);
            }
            else throw new Exception($"Variable {name} was not initialized");
        }
        private void Save(string name)
        {
            var value = stack.Pop();
            memory[name] = value;
        }
        private void Print(int n)
        {
            var items = new object[n];
            for (int i = n - 1; i >= 0; i--)
            {
                items[i] = stack.Pop().value;
            }
            Console.WriteLine(string.Join("", items));
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
                            throw new Exception($"Read value isnt of expected type. {value} is not int");
                    }
                    break;
                case "F":
                    {
                        float output;
                        if (float.TryParse(value, out output))
                            stack.Push((Type.Float, output));
                        else
                            throw new Exception($"Read value isnt of expected type. {value} is not float");
                    }
                    break;
                case "B":
                    {
                        if (value.Equals("true"))
                            stack.Push((Type.Boolean, true));
                        else if (value.Equals("false"))
                            stack.Push((Type.Boolean, false));
                        else
                            throw new Exception($"Read value isnt of expected type.  {value} is not bool");

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
                        stack.Push((Type.String, $"{(string)leftValue.value}{(string)rightValue.value}"));
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
    }
}
