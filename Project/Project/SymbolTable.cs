using Antlr4.Runtime;
using System.Collections.Generic;

namespace Project
{
    public class SymbolTable
    {
        Dictionary<string, Type> memory = new Dictionary<string, Type>();

        public void Add(IToken variable, Type type)
        {
            var name = variable.Text.Trim();
            if (memory.ContainsKey(name))
            {
                Errors.ReportError(variable, $"Variable {name} was already declared.");
            }
            else
            {
                object value = type switch
                {
                    Type.Int => 0,
                    Type.Float => (float)0,
                    Type.String => "",
                    Type.Boolean => false,
                    _ => throw new System.Exception("Invalid type"),
                };

                memory.Add(name, type);
            }
        }
        public Type this[IToken variable]
        {
            get {
                var name = variable.Text.Trim();
                if (memory.ContainsKey(name))
                {
                    return memory[name];
                }else
                {
                    Errors.ReportError(variable, $"Variable {name} was NOT declared.");
                    return Type.Error;
                }
            }
            set
            {
                var name = variable.Text.Trim();
                memory[name] = value;
            }
        }
    }
}
