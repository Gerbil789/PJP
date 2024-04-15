using Antlr4.Runtime.Misc;
using System;

using PJP_Project;

namespace Project
{
    public class EvalVisitor : project_grammarBaseVisitor<(Type Type, object Value)>
    {
        SymbolTable symbolTable = new SymbolTable();
        private float ToFloat(object value)
        {
            if (value is int x) return (float)x;
            return (float)value;
        }
        public override (Type Type, object Value) VisitProgram([NotNull] project_grammarParser.ProgramContext context)
        {
            foreach (var statement in context.statement())
            {
                if (Visit(statement).Type == Type.Error)
                    return (Type.Error, 0);
            }
            return (Type.Empty, 0);
        }
        public override (Type Type, object Value) VisitDeclaration([NotNull] project_grammarParser.DeclarationContext context)
        {
            var type = Visit(context.primitiveType());
            if (type.Type == Type.Error)
                return (Type.Error, 0);

            foreach (var identifier in context.IDENTIFIER())
            {
                symbolTable.Add(identifier.Symbol, type.Type);
            }
            return (Type.Empty, 0);
        }
        public override (Type Type, object Value) VisitPrintExpr([NotNull] project_grammarParser.PrintExprContext context)
        {
            var value = Visit(context.expr());
            if (value.Type != Type.Error)
            {
                Console.WriteLine(value.Value);
                return (Type.Empty, 0);
            }
            else
            {
                Errors.PrintAndClearErrors();
                return (Type.Error, 0);
            }

        }
        public override (Type Type, object Value) VisitPrimitiveType([NotNull] project_grammarParser.PrimitiveTypeContext context)
        {
            switch (context.type.Text)
            {
                case "int":
                    return (Type.Int, 0);
                case "float":
                    return (Type.Float, 0.0);
                case "string":
                    return (Type.String, "");
                case "bool":
                    return (Type.Boolean, false);
                default:
                    return (Type.Error, 0);
            }
        }
        public override (Type Type, object Value) VisitFloat([NotNull] project_grammarParser.FloatContext context)
        {
            return (Type.Float, float.Parse(context.FLOAT().GetText()));
        }
        public override (Type Type, object Value) VisitInt([NotNull] project_grammarParser.IntContext context)
        {
            return (Type.Int, int.Parse(context.INT().GetText()));
        }
        public override (Type Type, object Value) VisitString([NotNull] project_grammarParser.StringContext context)
        {
            return (Type.String, context.STRING().GetText().Replace("\"", String.Empty));
        }
        public override (Type Type, object Value) VisitBool([NotNull] project_grammarParser.BoolContext context)
        {
            var value = context.GetText();
            if (value.Equals("true"))
                return (Type.Boolean, true);
            else if (value.Equals("false"))
                return (Type.Boolean, false);
            else
                return (Type.Error, 0);
        }
        public override (Type Type, object Value) VisitId([NotNull] project_grammarParser.IdContext context)
        {
            //return value and type of the variable
            return symbolTable[context.IDENTIFIER().Symbol];
        }
        public override (Type Type, object Value) VisitParens([NotNull] project_grammarParser.ParensContext context)
        {
            return Visit(context.expr());
        }
        public override (Type Type, object Value) VisitBlockOfStatements([NotNull] project_grammarParser.BlockOfStatementsContext context)
        {
            (Type type, object value) last = (Type.Empty, 0);
            foreach (var statement in context.statement())
            {
                last = Visit(statement);
                if (last.type == Type.Error)
                    return (Type.Error, 0);
            }
            return last;
        }
        public override (Type Type, object Value) VisitUnaryMinus([NotNull] project_grammarParser.UnaryMinusContext context)
        {
            var operand = Visit(context.expr());
            if (operand.Type == Type.Error)
                return (Type.Error, 0);
            switch (operand.Type)
            {
                case Type.Int:
                    {
                        return (Type.Int, -(int)operand.Value);
                    }
                case Type.Float:
                    {
                        return (Type.Float, -ToFloat(operand.Value));
                    }
                default:
                    {
                        Errors.ReportError(context.SUB().Symbol, $"In expression '{context.SUB().GetText()}{operand.Value}' you are using wrong operands.");
                        Errors.PrintAndClearErrors();
                        return (Type.Error, 0);
                    }
            }
        }
        public override (Type Type, object Value) VisitAddSubCon([NotNull] project_grammarParser.AddSubConContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (left.Type == Type.Error || right.Type == Type.Error)
                return (Type.Error, 0);

            switch (context.op.Type)
            {
                case project_grammarParser.ADD:
                    {
                        if ((left.Type == Type.String || right.Type == Type.String) || (left.Type == Type.Boolean || right.Type == Type.Boolean))
                        {
                            Errors.ReportError(context.ADD().Symbol, $"In expression '{left.Value}{context.ADD().GetText()}{right.Value}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            return (Type.Error, 0);
                        }
                        if (left.Type == Type.Float || right.Type == Type.Float)
                            return (Type.Float, ToFloat(left.Value) + ToFloat(right.Value));
                        else
                            return (Type.Int, (int)left.Value + (int)right.Value);
                    }
                case project_grammarParser.SUB:
                    {
                        if ((left.Type == Type.String || right.Type == Type.String) || (left.Type == Type.Boolean || right.Type == Type.Boolean))
                        {
                            Errors.ReportError(context.SUB().Symbol, $"In expression '{left.Value}{context.SUB().GetText()}{right.Value}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            return (Type.Error, 0);
                        }
                        if (left.Type == Type.Float || right.Type == Type.Float)
                            return (Type.Float, ToFloat(left.Value) - ToFloat(right.Value));
                        else
                            return (Type.Int, (int)left.Value - (int)right.Value);
                    }
                case project_grammarParser.CON:
                    {
                        if (left.Type == Type.String && right.Type == Type.String)
                            return (Type.String, (left.Value as string) + (right.Value as string));
                        else
                        {
                            Errors.ReportError(context.CON().Symbol, $"In expression '{left.Value}{context.CON().GetText()}{right.Value}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            return (Type.Error, 0);
                        }
                    }
                default:
                    return (Type.Error, 0);
            }
        }
        public override (Type Type, object Value) VisitMulDivMod([NotNull] project_grammarParser.MulDivModContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);
            if (left.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);
            switch (context.op.Type)
            {
                case project_grammarParser.MUL:
                    {
                        if (left.Type == Type.String || right.Type == Type.String)
                        {
                            Errors.ReportError(context.MUL().Symbol, $"In expression '{left.Value}{context.MUL().GetText()}{right.Value}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            return (Type.Error, 0);
                        }
                        if (left.Type == Type.Float || right.Type == Type.Float)
                        {
                            return (Type.Float, ToFloat(left.Value) * ToFloat(right.Value));
                        }
                        else
                        {
                            return (Type.Int, (int)left.Value * (int)right.Value);
                        }
                    }
                case project_grammarParser.DIV:
                    {
                        if (left.Type == Type.String || right.Type == Type.String)
                        {
                            Errors.ReportError(context.DIV().Symbol, $"In expression '{left.Value}{context.DIV().GetText()}{right.Value}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            return (Type.Error, 0);
                        }

                        if (left.Type == Type.Float || right.Type == Type.Float)
                        {
                            if (ToFloat(right.Value) == 0.0)
                            {
                                Errors.ReportError(context.DIV().Symbol, $"In expression '{left.Value}{context.DIV().GetText()}{right.Value}' you are dividing by zero.");
                                Errors.PrintAndClearErrors();
                                return (Type.Error, 0);
                            }
                            else
                            {
                                return (Type.Float, ToFloat(left.Value) / ToFloat(right.Value));
                            }
                        }
                        else
                        {
                            if ((int)right.Value == 0)
                            {
                                Errors.ReportError(context.DIV().Symbol, $"In expression '{left.Value}{context.DIV().GetText()}{right.Value}' you are dividing by zero.");
                                Errors.PrintAndClearErrors();
                                return (Type.Error, 0);
                            }
                            else
                            {
                                return (Type.Int, (int)left.Value / (int)right.Value);
                            }
                        }
                    }
                case project_grammarParser.MOD:
                    {
                        if (left.Type == Type.Int && right.Type == Type.Int)
                        {
                            return (Type.Int, (int)left.Value % (int)right.Value);
                        }
                        else
                        {
                            Errors.ReportError(context.MOD().Symbol, $"In expression '{left.Value}{context.MOD().GetText()}{right.Value}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            return (Type.Error, 0);
                        }
                    }
                default:
                    {
                        return (Type.Error, 0);
                    }

            }
        }
        public override (Type Type, object Value) VisitAssignment([NotNull] project_grammarParser.AssignmentContext context)
        {
            var right = Visit(context.expr());
            var variable = symbolTable[context.IDENTIFIER().Symbol];
            if (variable.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);

            if (variable.Type == right.Type)
            {
                symbolTable[context.IDENTIFIER().Symbol] = right;
                return right;
            }

            if (variable.Type == Type.Float && right.Type == Type.Int)
            {
                var value = (Type.Float, ToFloat(right.Value));
                symbolTable[context.IDENTIFIER().Symbol] = value;
                return value;
            }

            if (variable.Type != right.Type)
            {
                Errors.ReportError(context.IDENTIFIER().Symbol, $"Variable '{context.IDENTIFIER().GetText()}' type is {variable.Type}, but the assigned value is {right.Type}.");
                Errors.PrintAndClearErrors();
                return (Type.Error, 0);
            }

            return (Type.Error, 0);
        }
        public override (Type Type, object Value) VisitNegation([NotNull] project_grammarParser.NegationContext context)
        {
            var operand = Visit(context.expr());
            if (operand.Type == Type.Error)
                return (Type.Error, 0);
            switch (operand.Type)
            {
                case Type.Boolean:
                    {
                        return (Type.Boolean, !(bool)operand.Value);
                    }
                default:
                    {
                        Errors.ReportError(context.NEG().Symbol, $"In expression '{context.NEG().GetText()}{operand.Value}' you are using wrong operands.");
                        Errors.PrintAndClearErrors();
                        return (Type.Error, 0);
                    }
            }
        }
        public override (Type Type, object Value) VisitLogicalAnd([NotNull] project_grammarParser.LogicalAndContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (left.Type == Type.Error || right.Type == Type.Error)
                return (Type.Error, 0);


            if (left.Type != Type.Boolean || right.Type != Type.Boolean)
            {
                Errors.ReportError(context.AND().Symbol, $"In expression '{left.Value}{context.AND().GetText()}{right.Value}' you are using wrong operands.");
                Errors.PrintAndClearErrors();
                return (Type.Error, 0);
            }

            return (Type.Boolean, (bool)left.Value && (bool)right.Value);
        }
        public override (Type Type, object Value) VisitLogicalOr([NotNull] project_grammarParser.LogicalOrContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (left.Type == Type.Error || right.Type == Type.Error)
                return (Type.Error, 0);


            if (left.Type != Type.Boolean || right.Type != Type.Boolean)
            {
                Errors.ReportError(context.OR().Symbol, $"In expression '{left.Value}{context.OR().GetText()}{right.Value}' you are using wrong operands.");
                Errors.PrintAndClearErrors();
                return (Type.Error, 0);
            }

            return (Type.Boolean, (bool)left.Value || (bool)right.Value);
        }
        public override (Type Type, object Value) VisitComparison([NotNull] project_grammarParser.ComparisonContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (left.Type == Type.Error || right.Type == Type.Error)
                return (Type.Error, 0);

            switch (context.op.Type)
            {
                case project_grammarParser.EQ:
                    {
                        if ((left.Type == Type.Boolean || right.Type == Type.Boolean))
                        {
                            Errors.ReportError(context.EQ().Symbol, $"In expression '{left.Value}{context.EQ().GetText()}{right.Value}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            return (Type.Error, 0);
                        }
                        if (left.Type == Type.Float || right.Type == Type.Float)
                            return (Type.Boolean, ToFloat(left.Value) == ToFloat(right.Value));
                        else if (left.Type == Type.Int && right.Type == Type.Int)
                            return (Type.Boolean, (int)left.Value == (int)right.Value);
                        else
                            return (Type.Boolean, (string)left.Value == (string)right.Value);
                    }
                case project_grammarParser.NEQ:
                    {
                        if ((left.Type == Type.Boolean || right.Type == Type.Boolean))
                        {
                            Errors.ReportError(context.NEQ().Symbol, $"In expression '{left.Value}{context.NEQ().GetText()}{right.Value}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            return (Type.Error, 0);
                        }
                        if (left.Type == Type.Float || right.Type == Type.Float)
                            return (Type.Boolean, ToFloat(left.Value) != ToFloat(right.Value));
                        else if (left.Type == Type.Int && right.Type == Type.Int)
                            return (Type.Boolean, (int)left.Value != (int)right.Value);
                        else
                            return (Type.Boolean, (string)left.Value != (string)right.Value);
                    }
                default:
                    return (Type.Error, 0);
            }
        }
        public override (Type Type, object Value) VisitRelation([NotNull] project_grammarParser.RelationContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (left.Type == Type.Error || right.Type == Type.Error)
                return (Type.Error, 0);

            switch (context.op.Type)
            {
                case project_grammarParser.LES:
                    {
                        if ((left.Type == Type.String || right.Type == Type.String) || (left.Type == Type.Boolean || right.Type == Type.Boolean))
                        {
                            Errors.ReportError(context.LES().Symbol, $"In expression '{left.Value}{context.LES().GetText()}{right.Value}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            return (Type.Error, 0);
                        }
                        if (left.Type == Type.Float || right.Type == Type.Float)
                            return (Type.Boolean, ToFloat(left.Value) < ToFloat(right.Value));
                        else
                            return (Type.Boolean, (int)left.Value < (int)right.Value);
                    }
                case project_grammarParser.GRE:
                    {
                        if ((left.Type == Type.String || right.Type == Type.String) || (left.Type == Type.Boolean || right.Type == Type.Boolean))
                        {
                            Errors.ReportError(context.LES().Symbol, $"In expression '{left.Value}{context.GRE().GetText()}{right.Value}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            return (Type.Error, 0);
                        }
                        if (left.Type == Type.Float || right.Type == Type.Float)
                            return (Type.Boolean, ToFloat(left.Value) > ToFloat(right.Value));
                        else
                            return (Type.Boolean, (int)left.Value > (int)right.Value);


                    }
                default:
                    return (Type.Error, 0);
            }
        }
        public override (Type Type, object Value) VisitIfElse([NotNull] project_grammarParser.IfElseContext context)
        {
            var condition = Visit(context.expr());
            if (condition.Type != Type.Boolean)
            {
                Errors.ReportError(context.IF().Symbol, $"Condition in IF statement must be BOOL");
                Errors.PrintAndClearErrors();
                return (Type.Error, 0);
            }
            if ((bool)condition.Value)
            {
                return Visit(context.pos);
            }
            else
            {
                if (context.neg != null)
                    return Visit(context.neg);
            }

            return (Type.Empty, 0);
        }
        public override (Type Type, object Value) VisitWhile([NotNull] project_grammarParser.WhileContext context)
        {
            var condition = Visit(context.expr());
            if (condition.Type != Type.Boolean)
            {
                Errors.ReportError(context.WHILE().Symbol, $"Condition in WHILE statement must be BOOL");
                Errors.PrintAndClearErrors();
                return (Type.Error, 0);
            }
            var c = condition;
            while ((bool)c.Value == true)
            {
                var value = Visit(context.statement());
                c = Visit(context.expr());
            }
            return (Type.Empty, 0);
        }
        public override (Type Type, object Value) VisitReadStatement([NotNull] project_grammarParser.ReadStatementContext context)
        {
            foreach (var identifier in context.IDENTIFIER())
            {
                var variable = symbolTable[identifier.Symbol];
                string value = Console.ReadLine();

                switch (variable.Type)
                {
                    case Type.Boolean:
                        {
                            if (value.Equals("true"))
                                symbolTable[identifier.Symbol] = (Type.Boolean, true);
                            else if (value.Equals("false"))
                                symbolTable[identifier.Symbol] = (Type.Boolean, false);
                            else
                            {
                                Errors.ReportError(context.READ().Symbol, $"You need to enter \"true\" or \"false\" value into boolean variable.");
                                Errors.PrintAndClearErrors();
                                return (Type.Error, 0);
                            }
                        }
                        break;
                    case Type.String:
                        {
                            symbolTable[identifier.Symbol] = (Type.String, value.Replace("\"", String.Empty));
                        }
                        break;
                    case Type.Int:
                        {
                            int output;
                            if (int.TryParse(value, out output))
                                symbolTable[identifier.Symbol] = (Type.Int, output);
                            else
                            {
                                Errors.ReportError(context.READ().Symbol, $"You need to enter integer value into integer variable.");
                                Errors.PrintAndClearErrors();
                                return (Type.Error, 0);
                            }

                        }
                        break;
                    case Type.Float:
                        {
                            float output;
                            if (float.TryParse(value, out output))
                                symbolTable[identifier.Symbol] = (Type.Float, output);
                            else
                            {
                                Errors.ReportError(context.READ().Symbol, $"You need to enter integer or float value into float variable.");
                                Errors.PrintAndClearErrors();
                                return (Type.Error, 0);
                            }
                        }
                        break;
                    default:
                        {
                            return (Type.Error, 0);
                        }
                }
            }

            return (Type.Empty, 0);
        }
        public override (Type Type, object Value) VisitWriteStatement([NotNull] project_grammarParser.WriteStatementContext context)
        {
            foreach (var expr in context.expr())
            {
                var valueOfExpr = Visit(expr);

                if (valueOfExpr.Type != Type.Error)
                    Console.Write(valueOfExpr.Value);
                //Console.Write(" ");
            }
            Console.Write("\n");

            return (Type.Empty, 0);
        }
        public override (Type Type, object Value) VisitEmptyStatement([NotNull] project_grammarParser.EmptyStatementContext context)
        {
            return (Type.Empty, 0);
        }
    }
}
