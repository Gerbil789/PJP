using System;
using System.Linq;

namespace Project
{
    public class EvalVisitor : Project_GrammarBaseVisitor<bool>
    {
        SymbolTable symbolTable = new SymbolTable();

        private float ToFloat(object value)
        {
            if (value is int x) return (float)x;
            return (float)value;
        }

        private bool IsValidIdentifier(string id)
        {
            // You can implement your own identifier validation logic here
            // (e.g., check for reserved keywords or start with a letter)
            return true; // Assuming basic identifier validation for now
        }

        public override bool VisitProgram(Project_GrammarParser.ProgramContext context)
        {
            // Check if all statements are valid
            return context.statement().ToList().TrueForAll(statement => Visit(statement));
        }

        public override bool VisitDeclaration(Project_GrammarParser.DeclarationContext context)
        {
            // Check if primitive type is valid
            var type = context.primitiveType().GetText();
            string[] validPrimitiveTypes = { "int", "float", "bool", "char" };

            if (!validPrimitiveTypes.Contains(type))
            {
                Console.WriteLine($"Error: Invalid primitive type '{context.primitiveType().GetText()}' in declaration.");
                return false;
            }


            // Check if all identifiers are valid
            foreach (var id in context.IDENTIFIER())
            {
                if (!IsValidIdentifier(id.GetText()))
                {
                    Console.Error.WriteLine($"Error: Invalid identifier '{id.GetText()}' in declaration.");
                    return false;
                }
            }

            return true;
        }
        public override bool VisitPrintExpr(Project_GrammarParser.PrintExprContext context)
        {
            // Check if the expression is valid
            return Visit(context.expr());
        }

        //public override (Type Type, object Value) VisitPrimitiveType([NotNull] Project_GrammarParser.PrimitiveTypeContext context)
        //{
        //    if (context.type.Text.Equals("int")) return (Type.Int, 0);
        //    else return (Type.Float, 0);
        //}

        //public override (Type Type, object Value) VisitFloat([NotNull] Project_GrammarParser.FloatContext context)
        //{
        //    return (Type.Float, float.Parse(context.FLOAT().GetText()));
        //}
        //public override (Type Type, object Value) VisitInt([NotNull] Project_GrammarParser.IntContext context)
        //{
        //    return (Type.Int, int.Parse(context.INT().GetText()));
        //}
        //public override (Type Type, object Value) VisitId([NotNull] Project_GrammarParser.IdContext context)
        //{
        //    return symbolTable[context.IDENTIFIER().Symbol];
        //}
        //public override (Type Type, object Value) VisitParens([NotNull] Project_GrammarParser.ParensContext context)
        //{
        //    return Visit(context.expr());
        //}

        //public override (Type Type, object Value) VisitAddSub([NotNull] Project_GrammarParser.AddSubContext context)
        //{
        //    var left = Visit(context.expr()[0]);
        //    var right = Visit(context.expr()[1]);
        //    if (left.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);
        //    if (left.Type == Type.Float || right.Type == Type.Float)
        //    {
        //        if (context.op.Type == Project_GrammarParser.ADD) return (Type.Float, ToFloat(left.Value) + ToFloat(right.Value));
        //        else return (Type.Float, ToFloat(left.Value) - ToFloat(right.Value));
        //    }
        //    else
        //    {
        //        if (context.op.Type == Project_GrammarParser.ADD) return (Type.Int, (int)left.Value + (int)right.Value);
        //        else return (Type.Int, (int)left.Value - (int)right.Value);
        //    }
        //}

        //public override (Type Type, object Value) VisitMulDiv([NotNull] Project_GrammarParser.MulDivContext context)
        //{
        //    var left = Visit(context.expr()[0]);
        //    var right = Visit(context.expr()[1]);
        //    if (left.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);
        //    if (left.Type == Type.Float || right.Type == Type.Float)
        //    {
        //        if (context.op.Type == Project_GrammarParser.MUL) return (Type.Float, ToFloat(left.Value) * ToFloat(right.Value));
        //        else return (Type.Float, ToFloat(left.Value) / ToFloat(right.Value));
        //    }
        //    else
        //    {
        //        if (context.op.Type == Project_GrammarParser.MUL) return (Type.Int, (int)left.Value * (int)right.Value);
        //        else return (Type.Int, (int)left.Value / (int)right.Value);
        //    }
        //}

        //public override (Type Type, object Value) VisitAssignment([NotNull] Project_GrammarParser.AssignmentContext context)
        //{
        //    var right = Visit(context.expr());
        //    var variable = symbolTable[context.IDENTIFIER().Symbol];
        //    if (variable.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);
        //    if (variable.Type == Type.Int && right.Type == Type.Float)
        //    {
        //        Errors.ReportError(context.IDENTIFIER().Symbol, $"Variable '{context.IDENTIFIER().GetText()}' type is int, but the assigned value is float.");
        //        return (Type.Error, 0);
        //    }
        //    if (variable.Type == Type.Float && right.Type == Type.Int)
        //    {
        //        var value = (Type.Float, ToFloat(right.Value));
        //        symbolTable[context.IDENTIFIER().Symbol] = value;
        //        return value;
        //    }
        //    else
        //    {
        //        symbolTable[context.IDENTIFIER().Symbol] = right;
        //        return right;
        //    }
        //}
    }
}
