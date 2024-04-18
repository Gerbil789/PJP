using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.IO;

namespace PJP_Project
{
    internal class CodeGeneratorListener : project_grammarBaseListener
    {
        public ParseTreeProperty<string> code = new();
        private SymbolTable symbolTable;
        private ParseTreeProperty<Type> types;

        private bool first = true; //check if it is the first assignment
        private string firstText = ""; //store the first assignment
        private int label = 0; //unique label generator

        public CodeGeneratorListener(ParseTreeProperty<Type> types, SymbolTable symbolTable)
        {
            this.types = types;
            this.symbolTable = symbolTable;
        }

        private int GenerateUniqueLabel()
        {
            return label++;
        }

        public override void EnterAssignment([NotNull] project_grammarParser.AssignmentContext context)
        {
            if (first)
            {
                first = false;
                firstText = context.expr().GetText();
            }
        }
        public override void ExitId([NotNull] project_grammarParser.IdContext context)
        {
            code.Put(context, $"load {context.IDENTIFIER().GetText()}\n");
        }
        public override void ExitBool([NotNull] project_grammarParser.BoolContext context)
        {
            var value = context.GetText();
            code.Put(context, $"push B {value}\n");
        }
        public override void ExitFloat([NotNull] project_grammarParser.FloatContext context)
        {
            var value = Convert.ToDouble(context.FLOAT().GetText());
            code.Put(context, $"push F {value:0.0#####}\n");
        }
        public override void ExitInt([NotNull] project_grammarParser.IntContext context)
        {
            var value = Convert.ToInt32(context.INT().GetText(), 10);
            code.Put(context, $"push I {value}\n");
        }
        public override void ExitString([NotNull] project_grammarParser.StringContext context)
        {
            var value = context.STRING().GetText();
            code.Put(context, $"push S {value}\n");
        }
        public override void ExitPrimitiveType([NotNull] project_grammarParser.PrimitiveTypeContext context)
        {
            code.Put(context, context.type.Text switch
            {
                "int" => $"push I 0\n",
                "float" => $"push F 0.0\n",
                "string" => $"push S \"\"\n",
                _ => $"push B false\n"
            });
        }
        public override void ExitParens([NotNull] project_grammarParser.ParensContext context)
        {
            code.Put(context, code.Get(context.expr()));
        }
        public override void ExitBlockOfStatements([NotNull] project_grammarParser.BlockOfStatementsContext context)
        {
            string s = "";
            foreach (var statement in context.statement())
            {
                s = s + code.Get(statement);
            }
            code.Put(context, s);
        }
        public override void ExitDeclaration([NotNull] project_grammarParser.DeclarationContext context)
        {
            var type = code.Get(context.primitiveType());
            var s = string.Empty;
            foreach (var identifier in context.IDENTIFIER())
            {
                s = s + type;
                s = s + "save " + identifier.GetText() + "\n";
            }
            code.Put(context, s);
        }
        public override void ExitPrintExpr([NotNull] project_grammarParser.PrintExprContext context)
        {
            code.Put(context, code.Get(context.expr()));
        }
        public override void ExitAssignment([NotNull] project_grammarParser.AssignmentContext context)
        {
            var right = code.Get(context.expr());
            var leftType = symbolTable[context.IDENTIFIER().Symbol];
            var rightType = types.Get(context.expr());
            string variable = context.IDENTIFIER().GetText();

            if (leftType == Type.Float && rightType == Type.Int)
            {
                code.Put(context, $"{right}itof\n" + $"save {variable}\n" + $"load {variable}\n");
                
            }
            else
            {
                code.Put(context, $"{right}save {variable} \n" + $"load {variable}\n");
            }

            if (context.expr().GetText() == firstText && !first)
            {
                first = true;
                right = code.Get(context);
                code.Put(context, right + "pop\n");
            }
        }

        public override void ExitRelation([NotNull] project_grammarParser.RelationContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);

            var leftType = types.Get(context.expr()[0]);
            var rightType = types.Get(context.expr()[1]);

            switch (context.op.Type)
            {
                case project_grammarParser.LES:
                    {
                        if (leftType == Type.Float || rightType == Type.Float)
                        {
                            if (leftType == Type.Float)
                            {
                                code.Put(context, left + right + "itof\n" + "lt\n");
                            }
                            else
                            {
                                code.Put(context, left + "itof\n" + right + "lt\n");
                            }
                        }
                        else
                        {
                            code.Put(context, left + right + "lt\n");
                        }
                    }
                    break;
                case project_grammarParser.GRE:
                    {
                        if (leftType == Type.Float || rightType == Type.Float)
                        {
                            if (leftType == Type.Float)
                            {
                                code.Put(context, left + right + "itof\n" + "gt\n");
                            }
                            else
                            {
                                code.Put(context, left + "itof\n" + right + "gt\n");
                            }
                        }
                        else
                        {
                            code.Put(context, left + right + "gt\n");
                        }
                    }
                    break;
            }
        }
        public override void ExitComparison([NotNull] project_grammarParser.ComparisonContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);

            var leftType = types.Get(context.expr()[0]);
            var rightType = types.Get(context.expr()[1]);

            switch (context.op.Type)
            {
                case project_grammarParser.EQ:
                    {
                        if (leftType == Type.Float || rightType == Type.Float)
                        {
                            if (leftType == Type.Float)
                            {
                                code.Put(context, left + right + "itof\n" + "eq F\n");
                            }
                            else
                            {
                                code.Put(context, left + "itof\n" + right + "eq F\n");
                            }
                        }
                        else if (leftType == Type.String)
                        {
                            code.Put(context, left + right + "eq S\n");
                        }
                        else
                        {
                            code.Put(context, left + right + "eq I\n");
                        }
                    }
                    break;
                case project_grammarParser.NEQ:
                    {
                        if (leftType == Type.Float || rightType == Type.Float)
                        {
                            if (leftType == Type.Float)
                            {
                                code.Put(context, left + right + "itof\n" + "eq\n" + "not\n");
                            }
                            else
                            {
                                code.Put(context, left + "itof\n" + right + "eq\n" + "not\n");
                            }
                        }
                        else if (leftType == Type.String)
                        {
                            code.Put(context, left + right + "eq\n" + "not\n");
                        }
                        else
                        {
                            code.Put(context, left + right + "eq\n" + "not\n");
                        }
                    }
                    break;
            }
        }
        public override void ExitLogicalAnd([NotNull] project_grammarParser.LogicalAndContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);

            code.Put(context, left + right + "and\n");
        }
        public override void ExitLogicalOr([NotNull] project_grammarParser.LogicalOrContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);

            code.Put(context, left + right + "or\n");
        }
        public override void ExitAddSubCon([NotNull] project_grammarParser.AddSubConContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);

            var leftType = types.Get(context.expr()[0]);
            var rightType = types.Get(context.expr()[1]);

            switch (context.op.Type)
            {
                case project_grammarParser.ADD:
                    {
                        if (types.Get(context) == Type.Float)
                        {

                            if (leftType == Type.Float)
                            {
                                code.Put(context, left + right + "itof\n" + "add\n");
                            }
                            else
                            {
                                code.Put(context, left + "itof\n" + right + "add\n");
                            }
                        }
                        else
                        {
                            code.Put(context, left + right + "add\n");
                        }
                    }
                    break;
                case project_grammarParser.SUB:
                    {
                        if (leftType == Type.Float || rightType == Type.Float)
                        {
                            if (leftType == Type.Float)
                            {
                                code.Put(context, left + right + "itof\n" + "sub\n");
                            }
                            else
                            {
                                code.Put(context, left + "itof\n" + right + "sub\n");
                            }
                        }
                        else
                        {
                            code.Put(context, left + right + "sub\n");
                        }
                    }
                    break;
                case project_grammarParser.CON:
                    {
                        code.Put(context, left + right + "concat\n");

                    }
                    break;
            }
        }
        public override void ExitMulDivMod([NotNull] project_grammarParser.MulDivModContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);

            var leftType = types.Get(context.expr()[0]);
            var rightType = types.Get(context.expr()[1]);

            switch (context.op.Type)
            {
                case project_grammarParser.MUL:
                    {
                        if((leftType == Type.Int && rightType == Type.Int) || (leftType == Type.Float && rightType == Type.Float))
                        {
                            code.Put(context, left + right + "mul\n");
                            break;
                        }

                        if (leftType != Type.Float || rightType != Type.Float)
                        {
                            if (leftType == Type.Float)
                            {
                                code.Put(context, left + right + "itof\n" + "mul\n");
                            }
                            else
                            {
                                code.Put(context, left + "itof\n" + right + "mul\n");
                            }
                        }
                    }
                    break;
                case project_grammarParser.DIV:
                    {
                        if ((leftType == Type.Int && rightType == Type.Int) || (leftType == Type.Float && rightType == Type.Float))
                        {
                            code.Put(context, left + right + "div\n");
                            break;
                        }

                        if (leftType != Type.Float || rightType != Type.Float)
                        {
                            if (leftType == Type.Float)
                            {
                                code.Put(context, left + right + "itof\n" + "div\n");
                            }
                            else
                            {
                                code.Put(context, left + "itof\n" + right + "div\n");
                            }
                        }
                    }
                    break;
                case project_grammarParser.MOD:
                    {

                        code.Put(context, left + right + "mod\n");

                    }
                    break;
            }
        }
        public override void ExitUnaryMinus([NotNull] project_grammarParser.UnaryMinusContext context)
        {
            var operand = code.Get(context.expr());

            switch (types.Get(context))
            {
                case Type.Int:
                    {
                        code.Put(context, operand + "uminus\n");
                    }
                    break;
                case Type.Float:
                    {
                        code.Put(context, operand + "uminus\n");
                    }
                    break;
            }
        }
        public override void ExitNegation([NotNull] project_grammarParser.NegationContext context)
        {
            var operand = code.Get(context.expr());

            code.Put(context, operand + "not\n");
        }
        public override void ExitWriteStatement([NotNull] project_grammarParser.WriteStatementContext context)
        {
            var s = string.Empty;
            int count = 0;
            foreach (var expr in context.expr())
            {
                var exprCode = code.Get(expr);
                s = s + exprCode;
                count++;
            }
            code.Put(context, s + "print " + count.ToString() + "\n");
        }
        public override void ExitReadStatement([NotNull] project_grammarParser.ReadStatementContext context)
        {
            string s = "";
            foreach (var identifier in context.IDENTIFIER())
            {
                var variableValue = symbolTable[identifier.Symbol];

                switch (variableValue)
                {
                    case Type.Boolean:
                        {
                            s = s + "read B\n";
                        }
                        break;
                    case Type.Int:
                        {
                            s = s + "read I\n";
                        }
                        break;
                    case Type.Float:
                        {
                            s = s + "read F\n";
                        }
                        break;
                    case Type.String:
                        {
                            s = s + "read S\n";
                        }
                        break;
                }
                s = s + "save " + identifier.Symbol.Text + "\n";
            }
            code.Put(context, s);
        }
        public override void ExitIfElse([NotNull] project_grammarParser.IfElseContext context)
        {
            var condition = code.Get(context.expr());

            string positiveBranch = "";
            string negativeBranch = "";
            int fjumpLabel = GenerateUniqueLabel();
            int positiveEnd = GenerateUniqueLabel();
            negativeBranch += (context.neg == null) ? "" : code.Get(context.neg);
            positiveBranch += code.Get(context.pos);
            code.Put(context, condition + "fjmp " + fjumpLabel.ToString() + "\n" + positiveBranch + "jmp " + positiveEnd.ToString() + "\n" + "label " + fjumpLabel.ToString() + "\n" + negativeBranch + "label " + positiveEnd.ToString() + "\n");
        }
        public override void ExitWhile([NotNull] project_grammarParser.WhileContext context)
        {
            var condition = code.Get(context.expr());

            var body = "";
            var startLabel = GenerateUniqueLabel();
            var endLabel = GenerateUniqueLabel();

            body += code.Get(context.statement());
            body += "jmp " + startLabel.ToString() + "\n";
            code.Put(context, "label " + startLabel.ToString() + "\n" + condition + "fjmp " + endLabel.ToString() + "\n" + body + "label " + endLabel.ToString() + "\n");
        }

        public override void ExitProgram([NotNull] project_grammarParser.ProgramContext context)
        {
            var allText = string.Empty;
            foreach (var statement in context.statement())
            {
                var s = code.Get(statement);
                allText += s;
                //Console.Write(s);
            }
            File.WriteAllText("output.txt", allText);
        }
    }
}
