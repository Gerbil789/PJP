using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace PJP_Project
{
    public class TypeCheckListener : project_grammarBaseListener
    {
        public SymbolTable symbolTable = new SymbolTable();
        public ParseTreeProperty<Type> types { get; } = new ParseTreeProperty<Type>();

        public override void ExitDeclaration([NotNull] project_grammarParser.DeclarationContext context)
        {
            var type = types.Get(context.primitiveType());
            foreach (var identifier in context.IDENTIFIER())
            {
                symbolTable.Add(identifier.Symbol, type);
            }
        }
        public override void ExitPrimitiveType([NotNull] project_grammarParser.PrimitiveTypeContext context)
        {
            var type = context.type.Text switch
            {
                "int" => Type.Int,
                "float" => Type.Float,
                "string" => Type.String,
                "bool" => Type.Boolean,
                _ => Type.Error,
            };

            types.Put(context, type);
        }
        public override void ExitPrintExpr([NotNull] project_grammarParser.PrintExprContext context)
        {
            var type = types.Get(context.expr());
            if (type == Type.Error)
            {
                types.Put(context, Type.Error);
            }
            else
            {
                types.Put(context, Type.Empty);
            }
        }
        public override void ExitFloat([NotNull] project_grammarParser.FloatContext context)
        {
            types.Put(context, Type.Float);
        }
        public override void ExitInt([NotNull] project_grammarParser.IntContext context)
        {
            types.Put(context, Type.Int);
        }
        public override void ExitString([NotNull] project_grammarParser.StringContext context)
        {
            types.Put(context, Type.String);
        }
        public override void ExitBool([NotNull] project_grammarParser.BoolContext context)
        {
            types.Put(context, Type.Boolean);
        }
        public override void ExitEmptyStatement([NotNull] project_grammarParser.EmptyStatementContext context)
        {
            types.Put(context, Type.Empty);
        }
        public override void ExitId([NotNull] project_grammarParser.IdContext context)
        {
            types.Put(context, symbolTable[context.IDENTIFIER().Symbol]);
        }
        public override void ExitParens([NotNull] project_grammarParser.ParensContext context)
        {
            types.Put(context, types.Get(context.expr()));
        }
        public override void ExitBlockOfStatements([NotNull] project_grammarParser.BlockOfStatementsContext context)
        {
            var last = Type.Empty;
            foreach (var statement in context.statement())
            {
                last = types.Get(statement);
            }
            types.Put(context, last);
        }
        public override void ExitUnaryMinus([NotNull] project_grammarParser.UnaryMinusContext context)
        {
            var type = types.Get(context.expr());
            types.Put(context, type);
        }
        public override void ExitAddSubCon([NotNull] project_grammarParser.AddSubConContext context)
        {
            var left = types.Get(context.expr()[0]);
            var right = types.Get(context.expr()[1]);
            if (left == Type.Error || right == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            switch (context.op.Type)
            {
                case project_grammarParser.ADD:
                    {
                        if ((left == Type.String || right == Type.String) || (left == Type.Boolean || right == Type.Boolean))
                        {
                            Errors.ReportError(context.ADD().Symbol, $"In expression '{left}{context.ADD().GetText()}{right}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            types.Put(context, Type.Error);
                            return;
                        }
                        if (left == Type.Float || right == Type.Float)
                            types.Put(context, Type.Float);
                        else
                            types.Put(context, Type.Int);
                        break;
                    }
                case project_grammarParser.SUB:
                    {
                        if ((left == Type.String || right == Type.String) || (left == Type.Boolean || right == Type.Boolean))
                        {
                            Errors.ReportError(context.SUB().Symbol, $"In expression '{left}{context.SUB().GetText()}{right}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            types.Put(context, Type.Error);
                            return;
                        }
                        if (left == Type.Float || right == Type.Float)
                            types.Put(context, Type.Float);
                        else
                            types.Put(context, Type.Int);
                        break;
                    }
                case project_grammarParser.CON:
                    {
                        if (left == Type.String && right == Type.String)
                            types.Put(context, Type.String);
                        else
                        {
                            Errors.ReportError(context.CON().Symbol, $"In expression '{left}{context.CON().GetText()}{right}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            types.Put(context, Type.Error);
                            return;
                        }
                        break;
                    }
                default:
                    types.Put(context, Type.Error);
                    return;
            }


        }
        public override void ExitMulDivMod([NotNull] project_grammarParser.MulDivModContext context)
        {
            var left = types.Get(context.expr()[0]);
            var right = types.Get(context.expr()[1]);
            if (left == Type.Error || right == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            switch (context.op.Type)
            {
                case project_grammarParser.MUL:
                    {
                        if (left == Type.String || right == Type.String)
                        {
                            Errors.ReportError(context.MUL().Symbol, $"In expression '{left}{context.MUL().GetText()}{right}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            types.Put(context, Type.Error);
                            return;
                        }
                        if (left == Type.Float || right == Type.Float)
                        {
                            types.Put(context, Type.Float);
                        }
                        else
                        {
                            types.Put(context, Type.Int);
                        }
                        break;
                    }
                case project_grammarParser.DIV:
                    {
                        if (left == Type.String || right == Type.String)
                        {
                            Errors.ReportError(context.DIV().Symbol, $"In expression '{left}{context.DIV().GetText()}{right}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            types.Put(context, Type.Error);
                            return;
                        }

                        if (left == Type.Float || right == Type.Float)
                        {
                            types.Put(context, Type.Float);
                        }
                        else
                        {
                            types.Put(context, Type.Int);

                        }
                        break;
                    }
                case project_grammarParser.MOD:
                    {
                        if (left == Type.Int && right == Type.Int)
                        {
                            types.Put(context, Type.Int);
                        }
                        else
                        {
                            Errors.ReportError(context.MOD().Symbol, $"In expression '{left}{context.MOD().GetText()}{right}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            types.Put(context, Type.Error);
                            return;
                        }
                        break;
                    }
                default:
                    {
                        types.Put(context, Type.Error);
                        return;
                    }
            }
        }
        public override void ExitAssignment([NotNull] project_grammarParser.AssignmentContext context)
        {
            var rightType = types.Get(context.expr());
            var leftType = symbolTable[context.IDENTIFIER().Symbol];

            if (leftType == Type.Error || rightType == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            if (leftType == rightType)
            {
                symbolTable[context.IDENTIFIER().Symbol] = rightType;
                types.Put(context.expr(), rightType);
                return;
            }

            if (leftType == Type.Float && rightType == Type.Int)
            {
                symbolTable[context.IDENTIFIER().Symbol] = Type.Float;
                types.Put(context.expr(), Type.Int);
                return;
            }

            Errors.ReportError(context.IDENTIFIER().Symbol, $"Variable '{context.IDENTIFIER().GetText()}' type is {leftType}, but the assigned value is {rightType}.");
            Errors.PrintAndClearErrors();
            types.Put(context, Type.Error);
        }
        public override void ExitNegation([NotNull] project_grammarParser.NegationContext context)
        {
            var type = types.Get(context.expr());
            if (type == Type.Boolean)
            {
                types.Put(context, Type.Boolean);
            }
            else
            {
                Errors.ReportError(context.NEG().Symbol, $"In expression '{context.NEG().GetText()}{type}' you are using wrong operands.");
                Errors.PrintAndClearErrors();
                types.Put(context, Type.Error);
            }
        }
        public override void ExitComparison([NotNull] project_grammarParser.ComparisonContext context)
        {
            var left = types.Get(context.expr()[0]);
            var right = types.Get(context.expr()[1]);
            if (left == Type.Error || right == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            switch (context.op.Type)
            {
                case project_grammarParser.EQ:
                    {
                        if ((left == Type.Boolean || right == Type.Boolean))
                        {
                            Errors.ReportError(context.EQ().Symbol, $"In expression '{left}   {context.EQ().GetText()}   {right}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            types.Put(context, Type.Error);
                            return;
                        }
                        if (left == Type.Float || right == Type.Float)
                            types.Put(context, Type.Boolean);
                        else if (left == Type.Int && right == Type.Int)
                            types.Put(context, Type.Boolean);
                        else
                            types.Put(context, Type.Boolean);
                        break;
                    }
                case project_grammarParser.NEQ:
                    {
                        if ((left == Type.Boolean || right == Type.Boolean))
                        {
                            Errors.ReportError(context.NEQ().Symbol, $"In expression '{left}{context.NEQ().GetText()}{right}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            types.Put(context, Type.Error);
                            return;
                        }
                        if (left == Type.Float || right == Type.Float)
                            types.Put(context, Type.Boolean);
                        else if (left == Type.Int && right == Type.Int)
                            types.Put(context, Type.Boolean);
                        else
                            types.Put(context, Type.Boolean);
                        break;  
                    }
                default:
                    types.Put(context, Type.Error);
                    return;
            }
        }
        public override void ExitLogicalAnd([NotNull] project_grammarParser.LogicalAndContext context)
        {
            var left = types.Get(context.expr()[0]);
            var right = types.Get(context.expr()[1]);
            if (left == Type.Error || right == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            if (left != Type.Boolean || right != Type.Boolean)
            {
                Errors.ReportError(context.AND().Symbol, $"In expression '{left}{context.AND().GetText()}{right}' you are using wrong operands.");
                Errors.PrintAndClearErrors();
                types.Put(context, Type.Error);
                return;
            }

            types.Put(context, Type.Boolean);
        }
        public override void ExitLogicalOr([NotNull] project_grammarParser.LogicalOrContext context)
        {
            var left = types.Get(context.expr()[0]);
            var right = types.Get(context.expr()[1]);
            if (left == Type.Error || right == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            if (left != Type.Boolean || right != Type.Boolean)
            {
                Errors.ReportError(context.OR().Symbol, $"In expression '{left}{context.OR().GetText()}{right}' you are using wrong operands.");
                Errors.PrintAndClearErrors();
                types.Put(context, Type.Error);
                return;
            }

            types.Put(context, Type.Boolean);
        }
        public override void ExitRelation([NotNull] project_grammarParser.RelationContext context)
        {
            var left = types.Get(context.expr()[0]);
            var right = types.Get(context.expr()[1]);
            if (left == Type.Error || right == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            switch (context.op.Type)
            {
                case project_grammarParser.LES:
                    {
                        if ((left == Type.String || right == Type.String) || (left == Type.Boolean || right == Type.Boolean))
                        {
                            Errors.ReportError(context.LES().Symbol, $"In expression '{left}{context.LES().GetText()}{right}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            types.Put(context, Type.Error);
                            return;
                        }
                        if (left == Type.Float || right == Type.Float)
                            types.Put(context, Type.Boolean);
                        else
                            types.Put(context, Type.Boolean);
                        break;
                    }
                case project_grammarParser.GRE:
                    {
                        if ((left == Type.String || right == Type.String) || (left == Type.Boolean || right == Type.Boolean))
                        {
                            Errors.ReportError(context.LES().Symbol, $"In expression '{left} {context.GRE().GetText()} {right}' you are using wrong operands.");
                            Errors.PrintAndClearErrors();
                            types.Put(context, Type.Error);
                            return;
                        }
                        if (left == Type.Float || right == Type.Float)
                            types.Put(context, Type.Boolean);
                        else
                            types.Put(context, Type.Boolean);
                        break;
                    }
                default:
                    types.Put(context, Type.Error);
                    return;
            }
        }
        public override void ExitIfElse([NotNull] project_grammarParser.IfElseContext context)
        {
            var type = types.Get(context.expr());
            if (type != Type.Boolean)
            {
                Errors.ReportError(context.IF().Symbol, $"In expression '{context.IF().GetText()}({type})' you are using wrong operands.");
                Errors.PrintAndClearErrors();
                types.Put(context, Type.Error);
                return;
            }

            
            if (types.Get(context.pos) == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            //if there is else statement
            if (context.neg != null)
            {
                if (types.Get(context.neg) == Type.Error)
                {
                    types.Put(context, Type.Error);
                    return;
                }
            }

            types.Put(context, Type.Empty);
        }
        public override void ExitReadStatement([NotNull] project_grammarParser.ReadStatementContext context)
        {
            foreach (var identifier in context.IDENTIFIER())
            {
                var type = symbolTable[identifier.Symbol];

                if (type == Type.Error)
                {
                    Errors.ReportError(identifier.Symbol, $"Variable '{identifier.GetText()}' is not declared.");
                    Errors.PrintAndClearErrors();
                    types.Put(context, Type.Error);
                    return;
                }

                symbolTable[identifier.Symbol] = type;
                //types.Put(context, type);
            }
            types.Put(context, Type.Empty);
        }
        public override void ExitWriteStatement([NotNull] project_grammarParser.WriteStatementContext context)
        {

            foreach (var expr in context.expr())
            {
                var type = types.Get(expr);
                if (type == Type.Error)
                {
                    types.Put(context, Type.Error);
                    return;
                }
            }
            types.Put(context, Type.Empty);
        }
        public override void ExitWhile([NotNull] project_grammarParser.WhileContext context)
        {
            var type = types.Get(context.expr());
            if (type != Type.Boolean)
            {
                Errors.ReportError(context.WHILE().Symbol, $"In expression '{context.WHILE().GetText()}({type})' you are using wrong operands.");
                Errors.PrintAndClearErrors();
                types.Put(context, Type.Error);
                return;
            }

            var whileType = types.Get(context.statement());
            if (whileType == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            types.Put(context, Type.Empty);
        }
        public override void ExitDoWhile([NotNull] project_grammarParser.DoWhileContext context)
        {
            var type = types.Get(context.expr());
            if (type != Type.Boolean)
            {
                Errors.ReportError(context.DO().Symbol, $"In expression '{context.DO().GetText()}({type})' you are using wrong operands.");
                Errors.PrintAndClearErrors();
                types.Put(context, Type.Error);
                return;
            }

            var doWhileType = types.Get(context.statement());
            if (doWhileType == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            types.Put(context, Type.Empty);
        }
        public override void ExitFor([NotNull] project_grammarParser.ForContext context)
        {
            //inizialization
            if (types.Get(context.expr()[0]) == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            //condition
            if (types.Get(context.expr()[1]) == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            //update
            if (types.Get(context.expr()[2]) == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            //body
            if (types.Get(context.statement()) == Type.Error)
            {
                types.Put(context, Type.Error);
                return;
            }

            types.Put(context, Type.Empty);
        }
    }
}
