using Antlr4.Runtime.Misc;
using System;
using System.Text;
using PJP_Project;

namespace Project
{
    internal class CodeGeneratorVisitor : project_grammarBaseVisitor<string>
    {
        SymbolTable symbolTable = new SymbolTable();
        public override string VisitProgram([NotNull] project_grammarParser.ProgramContext context)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var statement in context.statement())
            {
                var code = Visit(statement);
                sb.Append(code);
            }
            return sb.ToString();
        }

        // PUSH TYPE VALUE
        public override string VisitInt([NotNull] project_grammarParser.IntContext context)
        {
            var value = Convert.ToInt32(context.INT().GetText(), 10);
            return $"PUSH I {value}\n";
        }
        public override string VisitString([NotNull] project_grammarParser.StringContext context)
        {
            var value = context.STRING().GetText();
            return $"PUSH S {value}\n";
        }
        public override string VisitBool([NotNull] project_grammarParser.BoolContext context)
        {
            //var value = context.BOOL().GetText();
            var value = context.GetText();
            return $"PUSH B {value}\n";
        }
        public override string VisitFloat([NotNull] project_grammarParser.FloatContext context)
        {
            var value = Convert.ToDouble(context.FLOAT().GetText());
            return $"PUSH F {value}\n";
        }
        public override string VisitId([NotNull] project_grammarParser.IdContext context)
        {
            return "load " + context.IDENTIFIER().GetText() + "\n";
        }

        public override string VisitParens([NotNull] project_grammarParser.ParensContext context)
        {
            return Visit(context.expr());
        }
        public override string VisitBlockOfStatements([NotNull] project_grammarParser.BlockOfStatementsContext context)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var statement in context.statement())
            {
                var code = Visit(statement);
                sb.Append(code);
            }
            return sb.ToString();
        }

        public override string VisitDeclaration([NotNull] project_grammarParser.DeclarationContext context)
        {
            var type = Visit(context.primitiveType());

            StringBuilder sb = new StringBuilder();
            foreach (var identifier in context.IDENTIFIER())
            {
                sb.Append(Visit(context.primitiveType()) + "save " + identifier.GetText() + "\n");
            }
            return sb.ToString();
        }

        public override string VisitPrimitiveType([NotNull] project_grammarParser.PrimitiveTypeContext context)
        {
            if (context.type.Text.Equals("int"))
                return $"push I 0\n";
            else if (context.type.Text.Equals("float"))
                return $"push F 0.0\n";
            else if (context.type.Text.Equals("string"))
                return $"push S \"\"\n";
            else
                return $"push B false\n";
        }
        bool first = true;
        bool end = false;
        int n = -5;
        public override string VisitAssignment([NotNull] project_grammarParser.AssignmentContext context)
        {
            var right = Visit(context.expr());
            string variable = context.IDENTIFIER().GetText();
            return right + "save " + variable + "\n" + "load " + variable + "\n";


        }
        public override string VisitPrintExpr([NotNull] project_grammarParser.PrintExprContext context)
        {
            return Visit(context.expr());
        }
        public override string VisitLogicalAnd([NotNull] project_grammarParser.LogicalAndContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            return left + right + "and\n";
        }
        public override string VisitLogicalOr([NotNull] project_grammarParser.LogicalOrContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            return left + right + "or\n";
        }
        public override string VisitComparison([NotNull] project_grammarParser.ComparisonContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (context.op.Type == project_grammarParser.EQ)
            {
                return left + right + "eq\n";
            }
            else
            {
                return left + right + "eq\n" + "not\n";
            }
        }
        public override string VisitRelation([NotNull] project_grammarParser.RelationContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (context.op.Type == project_grammarParser.GRE)
            {
                return left + right + "gt\n";
            }
            else
            {
                return left + right + "lt\n";
            }
        }
        public override string VisitAddSubCon([NotNull] project_grammarParser.AddSubConContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (context.op.Type == project_grammarParser.ADD)
            {
                return left + right + "add\n";
            }
            else if (context.op.Type == project_grammarParser.SUB)
            {
                return left + right + "sub\n";
            }
            else
            {
                return left + right + "concat\n";
            }
        }
        public override string VisitMulDivMod([NotNull] project_grammarParser.MulDivModContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (context.op.Type == project_grammarParser.MUL)
            {
                return left + right + "mul\n";
            }
            else if (context.op.Type == project_grammarParser.DIV)
            {
                return left + right + "div\n";
            }
            else
            {
                return left + right + "mod\n";
            }
        }
        public override string VisitUnaryMinus([NotNull] project_grammarParser.UnaryMinusContext context)
        {
            var operand = Visit(context.expr());

            return operand + "uminus\n";
        }
        public override string VisitNegation([NotNull] project_grammarParser.NegationContext context)
        {
            var operand = Visit(context.expr());

            return operand + "not\n";

        }
    }
}
