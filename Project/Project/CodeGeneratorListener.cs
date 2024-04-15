using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PJP_Project;
using System;
using System.IO;

namespace Project
{
    internal class CodeGeneratorListener : project_grammarBaseListener
    {
        ParseTreeProperty<string> code = new ParseTreeProperty<string>();
        ParseTreeProperty<(Type type, object value)> values = new ParseTreeProperty<(Type type, object value)>();
        SymbolTable symbolTable = new SymbolTable();

        int label = 0;
        private int GenerateUniqueLabel()
        {
            return label++;
        }

        private float ToFloat(object value)
        {
            if (value is int x) return (float)x;
            return (float)value;
        }

        /*  public override void ExitMul([NotNull] PLC_Lab7_exprParser.MulContext context)
          {
              var left = values.Get(context.expr()[0]);
              var right = values.Get(context.expr()[1]);
              if (context.op.Text.Equals("*"))
              {
                  values.Put(context, left * right);
              }
              else
              {
                  values.Put(context, left / right);
              }
          }
        */

        // DATA TYPES LITERALS
        public override void ExitId([NotNull] project_grammarParser.IdContext context)
        {
            code.Put(context, "load " + context.IDENTIFIER().GetText() + "\n");
            values.Put(context, symbolTable[context.IDENTIFIER().Symbol]);
        }
        public override void ExitBool([NotNull] project_grammarParser.BoolContext context)
        {
            var value = context.GetText();
            code.Put(context, $"push B {value}\n");
            if (value.Equals("true"))
                values.Put(context, (Type.Boolean, true));
            else
                values.Put(context, (Type.Boolean, false));
        }
        public override void ExitFloat([NotNull] project_grammarParser.FloatContext context)
        {
            var value = Convert.ToDouble(context.FLOAT().GetText());
            code.Put(context, $"push F {value}\n");
            values.Put(context, (Type.Float, float.Parse(context.FLOAT().GetText())));
        }
        public override void ExitInt([NotNull] project_grammarParser.IntContext context)
        {
            var value = Convert.ToInt32(context.INT().GetText(), 10);
            code.Put(context, $"push I {value}\n");
            values.Put(context, (Type.Int, int.Parse(context.INT().GetText())));
        }
        public override void ExitString([NotNull] project_grammarParser.StringContext context)
        {
            var value = context.STRING().GetText();
            code.Put(context, $"push S {value}\n");
            values.Put(context, (Type.String, context.STRING().GetText().Replace("\"", String.Empty)));
        }
        public override void ExitPrimitiveType([NotNull] project_grammarParser.PrimitiveTypeContext context)
        {
            if (context.type.Text.Equals("int"))
            {
                values.Put(context, (Type.Int, 0));
                code.Put(context, $"push I 0\n");
            }
            else if (context.type.Text.Equals("float"))
            {
                values.Put(context, (Type.Float, 0.0));
                code.Put(context, $"push F 0.0\n");
            }
            else if (context.type.Text.Equals("string"))
            {
                values.Put(context, (Type.String, ""));
                code.Put(context, $"push S \"\"\n");
            }
            else
            {
                values.Put(context, (Type.Boolean, false));
                code.Put(context, $"push B false\n");
            }
        }
        // BLOCKS
        public override void ExitParens([NotNull] project_grammarParser.ParensContext context)
        {
            code.Put(context, code.Get(context.expr()));
            values.Put(context, values.Get(context.expr()));
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
        public override void ExitProgram([NotNull] project_grammarParser.ProgramContext context)
        {
            string allText = "";
            foreach (var statement in context.statement())
            {
                var s = code.Get(statement);
                allText += s;
                Console.Write(s);
            }
            File.WriteAllText("output.txt", allText);
        }
        public override void ExitDeclaration([NotNull] project_grammarParser.DeclarationContext context)
        {
            var type = code.Get(context.primitiveType());
            var typeValue = values.Get(context.primitiveType());
            var s = "";
            foreach (var identifier in context.IDENTIFIER())
            {
                s = s + type;
                s = s + "save " + identifier.GetText() + "\n";
                symbolTable.Add(identifier.Symbol, typeValue.type);
            }
            code.Put(context, s);
        }
        public override void ExitPrintExpr([NotNull] project_grammarParser.PrintExprContext context)
        {
            code.Put(context, code.Get(context.expr()));
        }

        // OPERATORS
        bool first = true;
        string firstText = "";
        public override void ExitAssignment([NotNull] project_grammarParser.AssignmentContext context)
        {
            var right = code.Get(context.expr());
            var rightValue = values.Get(context.expr());
            var variableValue = symbolTable[context.IDENTIFIER().Symbol];
            string variable = context.IDENTIFIER().GetText();

            if (variableValue.Type == rightValue.type)
            {
                symbolTable[context.IDENTIFIER().Symbol] = rightValue;
                values.Put(context, rightValue);
                code.Put(context, right + "save " + variable + "\n" + "load " + variable + "\n");
            }
            else
            {
                var value = (Type.Float, ToFloat(rightValue.value));
                symbolTable[context.IDENTIFIER().Symbol] = value;
                values.Put(context, value);
                code.Put(context, right + "itof\n" + "save " + variable + "\n" + "load " + variable + "\n");
            }

            if (context.expr().GetText() == firstText && !first)
            {
                first = true;
                right = code.Get(context);
                code.Put(context, right + "pop\n");
            }

        }
        public override void EnterAssignment([NotNull] project_grammarParser.AssignmentContext context)
        {
            if (first)
            {
                first = false;
                firstText = context.expr().GetText();
            }
        }
        public override void ExitRelation([NotNull] project_grammarParser.RelationContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);
            var leftValue = values.Get(context.expr()[0]);
            var rightValue = values.Get(context.expr()[1]);

            switch (context.op.Type)
            {
                case project_grammarParser.LES:
                    {
                        if (leftValue.type == Type.Float || rightValue.type == Type.Float)
                        {
                            values.Put(context, (Type.Boolean, ToFloat(leftValue.value) < ToFloat(rightValue.value)));
                            if (leftValue.type == Type.Float)
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
                            values.Put(context, (Type.Boolean, (int)leftValue.value < (int)rightValue.value));
                            code.Put(context, left + right + "lt\n");
                        }
                    }
                    break;
                case project_grammarParser.GRE:
                    {
                        if (leftValue.type == Type.Float || rightValue.type == Type.Float)
                        {
                            values.Put(context, (Type.Boolean, ToFloat(leftValue.value) > ToFloat(rightValue.value)));
                            if (leftValue.type == Type.Float)
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
                            values.Put(context, (Type.Boolean, (int)leftValue.value > (int)rightValue.value));
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
            var leftValue = values.Get(context.expr()[0]);
            var rightValue = values.Get(context.expr()[1]);

            switch (context.op.Type)
            {
                case project_grammarParser.EQ:
                    {
                        if (leftValue.type == Type.Float || rightValue.type == Type.Float)
                        {
                            values.Put(context, (Type.Boolean, ToFloat(leftValue.value) == ToFloat(rightValue.value)));
                            if (leftValue.type == Type.Float)
                            {
                                code.Put(context, left + right + "itof\n" + "eq\n");
                            }
                            else
                            {
                                code.Put(context, left + "itof\n" + right + "eq\n");
                            }
                        }
                        else if (leftValue.type == Type.String)
                        {
                            values.Put(context, (Type.Boolean, (string)leftValue.value == (string)rightValue.value));
                            code.Put(context, left + right + "eq\n");
                        }
                        else
                        {
                            values.Put(context, (Type.Boolean, (int)leftValue.value == (int)rightValue.value));
                            code.Put(context, left + right + "eq\n");
                        }
                    }
                    break;
                case project_grammarParser.NEQ:
                    {
                        if (leftValue.type == Type.Float || rightValue.type == Type.Float)
                        {
                            values.Put(context, (Type.Boolean, ToFloat(leftValue.value) != ToFloat(rightValue.value)));
                            if (leftValue.type == Type.Float)
                            {
                                code.Put(context, left + right + "itof\n" + "eq\n" + "not\n");
                            }
                            else
                            {
                                code.Put(context, left + "itof\n" + right + "eq\n" + "not\n");
                            }
                        }
                        else if (leftValue.type == Type.String)
                        {
                            values.Put(context, (Type.Boolean, (string)leftValue.value != (string)rightValue.value));
                            code.Put(context, left + right + "eq\n" + "not\n");
                        }
                        else
                        {
                            values.Put(context, (Type.Boolean, (int)leftValue.value != (int)rightValue.value));
                            code.Put(context, left + right + "eq\n" + "not\n");
                        }
                    }
                    break;
            }
        }
        public override void ExitLogicalAnd([NotNull] project_grammarParser.LogicalAndContext context)
        {
            var leftValue = values.Get(context.expr()[0]);
            var rightValue = values.Get(context.expr()[1]);
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);

            values.Put(context, (Type.Boolean, (bool)leftValue.value && (bool)rightValue.value));
            code.Put(context, left + right + "and\n");

        }
        public override void ExitLogicalOr([NotNull] project_grammarParser.LogicalOrContext context)
        {
            var leftValue = values.Get(context.expr()[0]);
            var rightValue = values.Get(context.expr()[1]);
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);

            values.Put(context, (Type.Boolean, (bool)leftValue.value || (bool)rightValue.value));
            code.Put(context, left + right + "or\n");

        }
        public override void ExitAddSubCon([NotNull] project_grammarParser.AddSubConContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);
            var leftValue = values.Get(context.expr()[0]);
            var rightValue = values.Get(context.expr()[1]);

            switch (context.op.Type)
            {
                case project_grammarParser.ADD:
                    {
                        if (leftValue.type == Type.Float || rightValue.type == Type.Float)
                        {
                            values.Put(context, (Type.Float, ToFloat(leftValue.value) + ToFloat(rightValue.value)));
                            if (leftValue.type == Type.Float)
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
                            values.Put(context, (Type.Int, (int)leftValue.value + (int)rightValue.value));
                            code.Put(context, left + right + "add\n");
                        }
                    }
                    break;
                case project_grammarParser.SUB:
                    {
                        if (leftValue.type == Type.Float || rightValue.type == Type.Float)
                        {
                            values.Put(context, (Type.Float, ToFloat(leftValue.value) - ToFloat(rightValue.value)));
                            if (leftValue.type == Type.Float)
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
                            values.Put(context, (Type.Int, (int)leftValue.value - (int)rightValue.value));
                            code.Put(context, left + right + "sub\n");
                        }
                    }
                    break;
                case project_grammarParser.CON:
                    {

                        values.Put(context, (Type.String, (string)leftValue.value + (string)rightValue.value));
                        code.Put(context, left + right + "concat\n");

                    }
                    break;
            }
        }
        public override void ExitMulDivMod([NotNull] project_grammarParser.MulDivModContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);
            var leftValue = values.Get(context.expr()[0]);
            var rightValue = values.Get(context.expr()[1]);

            switch (context.op.Type)
            {
                case project_grammarParser.MUL:
                    {
                        if (leftValue.type == Type.Float || rightValue.type == Type.Float)
                        {
                            values.Put(context, (Type.Float, ToFloat(leftValue.value) * ToFloat(rightValue.value)));
                            if (leftValue.type == Type.Float)
                            {
                                code.Put(context, left + right + "itof\n" + "mul\n");
                            }
                            else
                            {
                                code.Put(context, left + "itof\n" + right + "mul\n");
                            }
                        }
                        else
                        {
                            values.Put(context, (Type.Int, (int)leftValue.value * (int)rightValue.value));
                            code.Put(context, left + right + "mul\n");
                        }
                    }
                    break;
                case project_grammarParser.DIV:
                    {
                        if (leftValue.type == Type.Float || rightValue.type == Type.Float)
                        {
                            values.Put(context, (Type.Float, ToFloat(leftValue.value) / ToFloat(rightValue.value)));
                            if (leftValue.type == Type.Float)
                            {
                                code.Put(context, left + right + "itof\n" + "div\n");
                            }
                            else
                            {
                                code.Put(context, left + "itof\n" + right + "div\n");
                            }
                        }
                        else
                        {
                            values.Put(context, (Type.Int, (int)leftValue.value / (int)rightValue.value));
                            code.Put(context, left + right + "div\n");
                        }
                    }
                    break;
                case project_grammarParser.MOD:
                    {

                        values.Put(context, (Type.Int, (int)leftValue.value % (int)rightValue.value));
                        code.Put(context, left + right + "mod\n");

                    }
                    break;
            }
        }
        public override void ExitUnaryMinus([NotNull] project_grammarParser.UnaryMinusContext context)
        {
            var operandValue = values.Get(context.expr());
            var operand = code.Get(context.expr());

            switch (operandValue.type)
            {
                case Type.Int:
                    {
                        values.Put(context, (Type.Int, -(int)operandValue.value));
                        code.Put(context, operand + "uminus\n");
                    }
                    break;
                case Type.Float:
                    {
                        values.Put(context, (Type.Float, -ToFloat(operandValue.value)));
                        code.Put(context, operand + "uminus\n");
                    }
                    break;
            }
        }
        public override void ExitNegation([NotNull] project_grammarParser.NegationContext context)
        {
            var operandValue = values.Get(context.expr());
            var operand = code.Get(context.expr());


            values.Put(context, (Type.Boolean, !(bool)operandValue.value));
            code.Put(context, operand + "not\n");

        }
        // READ WRITE
        public override void ExitWriteStatement([NotNull] project_grammarParser.WriteStatementContext context)
        {
            string s = "";
            int count = 0;
            foreach (var expr in context.expr())
            {
                var valueOfExpr = values.Get(expr);
                var exprCode = code.Get(expr);
                s = s + exprCode;
                count++;
                //Console.Write(" ");
            }
            //Console.Write("\n");
            code.Put(context, s + "print " + count.ToString() + "\n");
        }
        public override void ExitReadStatement([NotNull] project_grammarParser.ReadStatementContext context)
        {
            string s = "";
            foreach (var identifier in context.IDENTIFIER())
            {
                var variableValue = symbolTable[identifier.Symbol];

                switch (variableValue.Type)
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
        // IF, WHILE
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
            //var conditionValue = values.Get(context.expr());

            var body = "";
            var startLabel = GenerateUniqueLabel();
            var endLabel = GenerateUniqueLabel();

            body += code.Get(context.statement());
            body += "jmp " + startLabel.ToString() + "\n";
            /*while ((bool)conditionValue.value)
            {
                body += code.Get(context.statement());

                conditionValue = values.Get(context.expr());
            }*/
            code.Put(context, "label " + startLabel.ToString() + "\n" + condition + "fjmp " + endLabel.ToString() + "\n" + body + "label " + endLabel.ToString() + "\n");
        }
    }
}
