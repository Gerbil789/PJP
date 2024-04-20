using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>
            {
                { "jmp", args => current = labels[int.Parse(args[0])] },
                { "fjmp", args => current = (bool)stack.Pop().value ? current : labels[int.Parse(args[0])] },
                { "tjmp", args => current = (bool)stack.Pop().value ? labels[int.Parse(args[0])] : current },
                { "label", args => { /* Do nothing for label */ } },
                { "load", args => Load(args[0]) },
                { "save", args => Save(args[0]) },
                { "print", args => Print(int.Parse(args[0])) },
                { "read", args => Read(args[0]) },
                { "pop", args => stack.Pop() },
                { "push", args => { 
                        var type = args[0];
                        var obj = args[1];
                        if (type == "S")
                            for (int i = 3; i < args.Length; i++)
                                obj += " " + args[i];
                        Push(type, obj);
                    } },
                { "itof", args => Itof() },
                { "not", args => Not() },
                { "eq", args => Eq() },
                { "lt", args => Lt() },
                { "gt", args => Gt() },
                { "or", args => Or() },
                { "and", args => And() },
                { "concat", args => Concat() },
                { "uminus", args => Uminus() },
                { "mod", args => Mod() },
                { "div", args => Div() },
                { "mul", args => Mul() },
                { "sub", args => Sub() },
                { "add", args => Add() }
            };


            while (current < code.Count)
            {
                var command = code[current].Split(" ");
                var commandName = command[0];
                var commandArgs = command.Skip(1).ToArray();

                if (commands.ContainsKey(commandName))
                {
                    commands[commandName](commandArgs);
                }
                else
                {
                    throw new InvalidOperationException($"Unknown command: {commandName}");
                }

                current++;
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
        private void Read(string typeId)
        {
            var value = Console.ReadLine();
            object parsedValue = typeId switch
            {
                "I" when int.TryParse(value, out int intValue) => intValue,
                "F" when float.TryParse(value, out float floatValue) => floatValue,
                "B" when bool.TryParse(value, out bool boolValue) => boolValue,
                "S" => value.Replace("\"", String.Empty),
                _ => throw new Exception($"Expected '{typeId}' for Read operation.")
            };

            var type = typeId switch
            {
                "I" => Type.Int,
                "F" => Type.Float,
                "B" => Type.Boolean,
                "S" => Type.String,
                _ => throw new ArgumentException($"Unsupported type '{typeId}' specified.")
            };

            stack.Push((type, parsedValue));
        }
        private void Push(string typeId, string value)
        {
            object parsedValue = typeId switch
            {
                "I" when int.TryParse(value, out int intValue) => intValue,
                "F" when float.TryParse(value, out float floatValue) => floatValue,
                "B" when bool.TryParse(value, out bool boolValue) => boolValue,
                "S" => value.Replace("\"", string.Empty),
                _ => throw new Exception($"Unsupported type '{typeId}' specified for Push operation.")
            };

            var type = typeId switch
            {
                "I" => Type.Int,
                "F" => Type.Float,
                "B" => Type.Boolean,
                "S" => Type.String,
                _ => throw new ArgumentException($"Unknown type '{typeId}' specified.")
            };

            stack.Push((type, parsedValue));
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
            var value = stack.Pop();
            stack.Push((Type.Boolean, !(bool)value.value));
        }
        private void Eq()
        {
            var rightValue = stack.Pop();
            var leftValue = stack.Pop();

            bool result = leftValue.type switch
            {
                Type.Int => (int)Convert.ToInt32(leftValue.value) == (int)Convert.ToInt32(rightValue.value),
                Type.String => (string)leftValue.value == (string)rightValue.value,
                Type.Float => (float)leftValue.value == (float)rightValue.value,
                _ => throw new ArgumentException($"Unsupported type '{leftValue.type}' for equality comparison.")
            };

            stack.Push((Type.Boolean, result));
        }
        private void Lt()
        {
            var rightValue = stack.Pop();
            var leftValue = stack.Pop();

            bool result = leftValue.type switch
            {
                Type.Int => (int)Convert.ToInt32(leftValue.value) < (int)Convert.ToInt32(rightValue.value),
                Type.Float => (float)leftValue.value < (float)rightValue.value,
                _ => throw new ArgumentException($"Unsupported type '{leftValue.type}' for less than comparison.")
            };

            stack.Push((Type.Boolean, result));
        }
        private void Gt()
        {
            var rightValue = stack.Pop();
            var leftValue = stack.Pop();

            bool result = leftValue.type switch
            {
                Type.Int => (int)Convert.ToInt32(leftValue.value) > (int)Convert.ToInt32(rightValue.value),
                Type.Float => (float)leftValue.value > (float)rightValue.value,
                _ => throw new ArgumentException($"Unsupported type '{leftValue.type}' for greater than comparison.")
            };

            stack.Push((Type.Boolean, result));
        }
        private void Or()
        {
            var rightValue = stack.Pop();
            var leftValue = stack.Pop();

            if (leftValue.type != Type.Boolean)
                throw new ArgumentException($"Unsupported type '{leftValue.type}' or '{rightValue.type}' for logical OR operation.");

            stack.Push((Type.Boolean, (bool)leftValue.value || (bool)rightValue.value));
        }
        private void And()
        {
            var rightValue = stack.Pop();
            var leftValue = stack.Pop();

            if (leftValue.type != Type.Boolean)
                throw new ArgumentException($"Unsupported type '{leftValue.type}' or '{rightValue.type}' for logical AND operation.");

            stack.Push((Type.Boolean, (bool)leftValue.value && (bool)rightValue.value));
        }
        private void Concat()
        {
            var rightValue = stack.Pop();
            var leftValue = stack.Pop();

            if (leftValue.type != Type.String)
                throw new ArgumentException($"Unsupported type '{leftValue.type}' or '{rightValue.type}' for string concatenation.");

            stack.Push((Type.String, $"{(string)leftValue.value}{(string)rightValue.value}"));
        }
        private void Uminus()
        {
            var value = stack.Pop();

            object negatedValue = value.type switch
            {
                Type.Int => -(int)value.value,
                Type.Float => -(float)value.value,
                _ => throw new ArgumentException($"Unsupported type '{value.type}' for unary minus operation.")
            };
            stack.Push((value.type, negatedValue));
        }
        private void Mod()
        {
            var rightValue = stack.Pop();
            var leftValue = stack.Pop();

            stack.Push((Type.Int, (int)leftValue.value % (int)rightValue.value));
        }
        private void Div()
        {
            var rightValue = stack.Pop();
            var leftValue = stack.Pop();

            object result = leftValue.type switch
            {
                Type.Float => (float)leftValue.value / (float)rightValue.value,
                Type.Int => (int)leftValue.value / (int)rightValue.value,
                _ => throw new ArgumentException($"Unsupported type '{leftValue.type}' for division operation.")
            };

            stack.Push((leftValue.type, result));
        }
        private void Mul()
        {
            var rightValue = stack.Pop();
            var leftValue = stack.Pop();

            object result = leftValue.type switch
            {
                Type.Float => (float)leftValue.value * (float)rightValue.value,
                Type.Int => (int)leftValue.value * (int)rightValue.value,
                _ => throw new ArgumentException($"Unsupported type '{leftValue.type}' for multiplication operation.")
            };

            stack.Push((leftValue.type, result));
        }
        private void Add()
        {
            var rightValue = stack.Pop();
            var leftValue = stack.Pop();
           
            var result = leftValue.type switch
            {
                Type.Float => (float)leftValue.value + (float)rightValue.value,
                Type.Int => (int)Convert.ToInt32(leftValue.value) + (int)Convert.ToInt32(rightValue.value),
                _ => throw new ArgumentException($"Unsupported type '{leftValue.type}' for addition operation.")
            };

            stack.Push((leftValue.type, result));
        }
        private void Sub()
        {
            var rightValue = stack.Pop();
            var leftValue = stack.Pop();
            
            object result = leftValue.type switch
            {
                Type.Float => (float)leftValue.value - (float)rightValue.value,
                Type.Int => (int)Convert.ToInt32(leftValue.value) - (int)Convert.ToInt32(rightValue.value),
                _ => throw new ArgumentException($"Unsupported type '{leftValue.type}' for subtraction operation.")
            };

            stack.Push((leftValue.type, result));
        }
    }
}
