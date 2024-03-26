using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Lab8
{
    public class EvalVisitor : PLC_Lab8_exprBaseVisitor<(Type Type,object Value)>
    {
        SymbolTable symbolTable = new SymbolTable();

        private float ToFloat(object value)
        {
            if (value is int x) return (float)x;
            return (float)value;
        }

        public override (Type Type, object Value) VisitProgram([NotNull] PLC_Lab8_exprParser.ProgramContext context)
        {
            foreach(var statement in context.statement())
            {
                Visit(statement);
            }
            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitDeclaration([NotNull] PLC_Lab8_exprParser.DeclarationContext context)
        {
            var type = Visit(context.primitiveType());
            foreach(var identifier in context.IDENTIFIER())
            {
                symbolTable.Add(identifier.Symbol, type.Type);
            }
            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitPrintExpr([NotNull] PLC_Lab8_exprParser.PrintExprContext context)
        {
            var value = Visit(context.expr());
            if (value.Type != Type.Error) Console.WriteLine(value.Value);
            else
            {
                Errors.PrintAndClearErrors();
            }
            return (Type.Error, 0);
        }
        public override (Type Type, object Value) VisitPrimitiveType([NotNull] PLC_Lab8_exprParser.PrimitiveTypeContext context)
        {
            if (context.type.Text.Equals("int")) return (Type.Int,0);
            else return (Type.Float, 0);
        }

        public override (Type Type, object Value) VisitFloat([NotNull] PLC_Lab8_exprParser.FloatContext context)
        {
            return (Type.Float, float.Parse(context.FLOAT().GetText()));
        }
        public override (Type Type, object Value) VisitInt([NotNull] PLC_Lab8_exprParser.IntContext context)
        {
            return (Type.Int, int.Parse(context.INT().GetText()));
        }
        public override (Type Type, object Value) VisitId([NotNull] PLC_Lab8_exprParser.IdContext context)
        {
            return symbolTable[context.IDENTIFIER().Symbol];
        }
        public override (Type Type, object Value) VisitParens([NotNull] PLC_Lab8_exprParser.ParensContext context)
        {
            return Visit(context.expr());
        }

        public override (Type Type, object Value) VisitAddSub([NotNull] PLC_Lab8_exprParser.AddSubContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);
            if (left.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);
            if (left.Type == Type.Float || right.Type == Type.Float)
            {
                if (context.op.Type == PLC_Lab8_exprParser.ADD) return (Type.Float, ToFloat(left.Value) + ToFloat(right.Value));
                else return (Type.Float, ToFloat(left.Value) - ToFloat(right.Value));
            }else
            {
                if (context.op.Type == PLC_Lab8_exprParser.ADD) return (Type.Int, (int)left.Value + (int)right.Value);
                else return (Type.Int, (int)left.Value - (int)right.Value);
            }
        }

        public override (Type Type, object Value) VisitMulDiv([NotNull] PLC_Lab8_exprParser.MulDivContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);
            if (left.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);
            if (left.Type == Type.Float || right.Type == Type.Float)
            {
                if (context.op.Type == PLC_Lab8_exprParser.MUL) return (Type.Float, ToFloat(left.Value) * ToFloat(right.Value));
                else return (Type.Float, ToFloat(left.Value) / ToFloat(right.Value));
            }
            else
            {
                if (context.op.Type == PLC_Lab8_exprParser.MUL) return (Type.Int, (int)left.Value * (int)right.Value);
                else return (Type.Int, (int)left.Value / (int)right.Value);
            }
        }

        public override (Type Type, object Value) VisitAssignment([NotNull] PLC_Lab8_exprParser.AssignmentContext context)
        {
            var right = Visit(context.expr());
            var variable = symbolTable[context.IDENTIFIER().Symbol];
            if (variable.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);
            if (variable.Type == Type.Int && right.Type == Type.Float)
            {
                Errors.ReportError(context.IDENTIFIER().Symbol, $"Variable '{context.IDENTIFIER().GetText()}' type is int, but the assigned value is float.");
                return (Type.Error, 0);
            }
            if (variable.Type == Type.Float && right.Type == Type.Int)
            {
                var value = (Type.Float, ToFloat(right.Value));
                symbolTable[context.IDENTIFIER().Symbol] = value;
                return value;
            }
            else
            {
                symbolTable[context.IDENTIFIER().Symbol] = right;
                return right;
            }
        }
    }
}
