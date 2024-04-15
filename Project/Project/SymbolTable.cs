using Antlr4.Runtime;
using System.Collections.Generic;

namespace Project
{
    public class SymbolTable
    {
        Dictionary<string, (Type Type, object Value)> memory = new Dictionary<string, (Type Type, object Value)>();

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

                memory.Add(name, (type, value));
            }
        }
        public (Type Type, object Value) this[IToken variable]
        {
            get {
                var name = variable.Text.Trim();
                if (memory.ContainsKey(name))
                {
                    return memory[name];
                }else
                {
                    Errors.ReportError(variable, $"Variable {name} was NOT declared.");
                    return (Type.Error,0);
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
