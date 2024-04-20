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
        private string firstText = string.Empty; //store the first assignment
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
            var value = int.Parse(context.INT().GetText());
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
                "int" => "push I 0\n",
                "float" => "push F 0.0\n",
                "string" => "push S \"\"\n",
                _ => "push B false\n"
            });
        }
        public override void ExitParens([NotNull] project_grammarParser.ParensContext context)
        {
            code.Put(context, code.Get(context.expr()));
        }
        public override void ExitBlockOfStatements([NotNull] project_grammarParser.BlockOfStatementsContext context)
        {
            var s = string.Empty;
            foreach (var statement in context.statement())
            {
                s += code.Get(statement);
            }
            code.Put(context, s);
        }
        public override void ExitDeclaration([NotNull] project_grammarParser.DeclarationContext context)
        {
            var type = code.Get(context.primitiveType());
            var s = string.Empty;
            foreach (var identifier in context.IDENTIFIER())
            {
                s += $"{type}save {identifier.GetText()}\n";
            }
            code.Put(context, s);
        }
        public override void ExitPrintExpr([NotNull] project_grammarParser.PrintExprContext context)
        {
            code.Put(context, code.Get(context.expr()));
        }
        public override void ExitAssignment([NotNull] project_grammarParser.AssignmentContext context)
        {
            var left = context.IDENTIFIER().GetText();
            var right = code.Get(context.expr());

            var leftType = symbolTable[context.IDENTIFIER().Symbol];
            var rightType = types.Get(context.expr());

            var n = (leftType == Type.Float && rightType == Type.Int) ? "itof\n" : string.Empty;

            code.Put(context, $"{right}{n}save {left} \nload {left}\n");

            if (context.expr().GetText() == firstText && !first)
            {
                first = true;
                right = code.Get(context);
                code.Put(context, $"{right}pop\n");
            }
        }
        public override void ExitRelation([NotNull] project_grammarParser.RelationContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);

            var leftType = types.Get(context.expr()[0]);
            var rightType = types.Get(context.expr()[1]);

            var op = context.op.Type switch
            {
                project_grammarParser.LES => "lt",
                project_grammarParser.GRE => "gt",
                _ => throw new Exception("Invalid operator")
            };

            if (leftType == rightType)
            {
                code.Put(context, $"{left}{right}{op}\n");
                return;
            }

            if (leftType == Type.Float)
            {
                code.Put(context, $"{left}{right}itof\n{op}\n");
            }
            else
            {
                code.Put(context, $"{left}itof\n{right}{op}\n");
            }
        }
        public override void ExitComparison([NotNull] project_grammarParser.ComparisonContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);

            var leftType = types.Get(context.expr()[0]);
            var rightType = types.Get(context.expr()[1]);

            var not = context.op.Type == project_grammarParser.NEQ ? "not\n" : string.Empty;

            if (leftType == Type.Float || rightType == Type.Float)
            {
                if (leftType == Type.Float)
                {
                    code.Put(context, $"{left}{right}itof\neq F\n{not}");
                }
                else
                {
                    code.Put(context, $"{left}itof\n{right}eq F\n{not}");
                }
            }
            else if (leftType == Type.String)
            {
                code.Put(context, $"{left}{right}eq S\n{not}");
            }
            else
            {
                code.Put(context, $"{left}{right}eq I\n{not}");
            }
        }
        public override void ExitLogicalAnd([NotNull] project_grammarParser.LogicalAndContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);
            code.Put(context, $"{left}{right}and\n");
        }
        public override void ExitLogicalOr([NotNull] project_grammarParser.LogicalOrContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);
            code.Put(context, $"{left}{right}or\n");
        }
        public override void ExitAddSubCon([NotNull] project_grammarParser.AddSubConContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);

            var leftType = types.Get(context.expr()[0]);
            var rightType = types.Get(context.expr()[1]);

            if (context.op.Type == project_grammarParser.CON)
            {
                code.Put(context, $"{left}{right}concat\n");
                return;
            }

            var op = context.op.Type switch
            {
                project_grammarParser.ADD => "add",
                project_grammarParser.SUB => "sub",
                _ => throw new Exception("Invalid operator")
            };

            if (leftType == rightType)
            {
                code.Put(context, $"{left}{right}{op}\n");
                return;
            }

            if (leftType == Type.Float)
            {
                code.Put(context, $"{left}{right}itof\n{op}\n");
            }
            else
            {
                code.Put(context, $"{left}itof\n{right}{op}\n");
            }
        }
        public override void ExitMulDivMod([NotNull] project_grammarParser.MulDivModContext context)
        {
            var left = code.Get(context.expr()[0]);
            var right = code.Get(context.expr()[1]);

            var leftType = types.Get(context.expr()[0]);
            var rightType = types.Get(context.expr()[1]);

            if(context.op.Type == project_grammarParser.MOD)
            {
                code.Put(context, $"{left}{right}mod\n");
                return;
            }

            var op = context.op.Type switch
            {
                project_grammarParser.MUL => "mul",
                project_grammarParser.DIV => "div",
                _ => throw new Exception("Invalid operator")
            };

            if (leftType == rightType)
            {
                code.Put(context, $"{left}{right}{op}\n");
                return;
            }

            if (leftType == Type.Float)
            {
                code.Put(context, $"{left}{right}itof\n{op}\n");
            }
            else
            {
                code.Put(context, $"{left}itof\n{right}{op}\n");
            }
        }
        public override void ExitUnaryMinus([NotNull] project_grammarParser.UnaryMinusContext context)
        {
            var operand = code.Get(context.expr());
            code.Put(context, $"{operand}uminus\n");
        }
        public override void ExitNegation([NotNull] project_grammarParser.NegationContext context)
        {
            var operand = code.Get(context.expr());
            code.Put(context, $"{operand}not\n");
        }
        public override void ExitWriteStatement([NotNull] project_grammarParser.WriteStatementContext context)
        {
            var s = string.Empty;
            int count = 0;
            foreach (var expr in context.expr())
            {
                var exprCode = code.Get(expr);
                s += exprCode;
                count++;
            }
            code.Put(context, $"{s}print {count}\n");
        }
        public override void ExitReadStatement([NotNull] project_grammarParser.ReadStatementContext context)
        {
            var s = string.Empty;
            foreach (var identifier in context.IDENTIFIER())
            {
                var type = symbolTable[identifier.Symbol];

                var id = type switch
                {
                    Type.Boolean => 'B',
                    Type.Int => 'I',
                    Type.Float => 'F',
                    Type.String => 'S',
                    _ => throw new Exception("Invalid type")
                };

                s += $"read {id}\nsave {identifier.Symbol.Text}\n";
            }
            code.Put(context, s);
        }
        public override void ExitIfElse([NotNull] project_grammarParser.IfElseContext context)
        {
            var condition = code.Get(context.expr());

            string positiveBranch = string.Empty;
            string negativeBranch = string.Empty;
            int fjumpLabel = GenerateUniqueLabel();
            int positiveEnd = GenerateUniqueLabel();
            negativeBranch += (context.neg == null) ? "" : code.Get(context.neg);
            positiveBranch += code.Get(context.pos);
            code.Put(context, $"{condition}fjmp {fjumpLabel}\n{positiveBranch}jmp {positiveEnd}\nlabel {fjumpLabel}\n{negativeBranch}label {positiveEnd}\n");
        }
        public override void ExitWhile([NotNull] project_grammarParser.WhileContext context)
        {
            var condition = code.Get(context.expr());
            var startLabel = GenerateUniqueLabel();
            var endLabel = GenerateUniqueLabel();

            code.Put(context, $"label {startLabel}\n" +                     // set start label
                $"{condition}fjmp {endLabel}\n" +                           // if condition is false jump to end label
                $"{code.Get(context.statement())}jmp {startLabel}\n" +      // execute statement and jump to start label
                $"label {endLabel}\n");                                     // set end label
        }
        public override void ExitDoWhile([NotNull] project_grammarParser.DoWhileContext context)
        {
            var condition = code.Get(context.expr());
            var startLabel = GenerateUniqueLabel();
            var endLabel = GenerateUniqueLabel();

            code.Put(context, $"label {startLabel}\n" +     // set start label
                $"{code.Get(context.statement())}" +        // execute statement
                $"{condition}fjmp {endLabel}\n" +           // if condition is false jump to end label
                $"jmp {startLabel}\n" +                     // jump back to start label
                $"label {endLabel}\n");                     // set end label
        }
        public override void ExitFor([NotNull] project_grammarParser.ForContext context)
        {
            var initialization = code.Get(context.expr()[0]);
            var condition = code.Get(context.expr()[1]);
            var update = code.Get(context.expr()[2]);
            var startLabel = GenerateUniqueLabel();
            var endLabel = GenerateUniqueLabel();

            code.Put(context, $"{initialization}" +                     // execute initialization
                $"label {startLabel}\n" +                               // set start label
                $"{condition}fjmp {endLabel}\n" +                       // if condition is false jump to end label
                $"{code.Get(context.statement())}" +                    // execute loop body
                $"{update}jmp {startLabel}\n" +                         // jump back to start label for update
                $"label {endLabel}\n");                                 // end label
        }
        public override void ExitProgram([NotNull] project_grammarParser.ProgramContext context)
        {
            var text = string.Empty;
            foreach (var statement in context.statement())
            {
                var s = code.Get(statement);
                text += s;
                //Console.Write(s);
            }
            File.WriteAllText("output.txt", text);
        }
    }
}
