using Antlr4.Runtime.Misc;
using System;
using PJP_Project;

namespace Project
{
  public class EvalVisitor : project_grammarBaseVisitor<Type>
  {
    SymbolTable symbolTable = new SymbolTable();
    private float ToFloat(object value)
    {
      if (value is int x) return (float)x;
      return (float)value;
    }
    public override Type VisitProgram([NotNull] project_grammarParser.ProgramContext context)
    {
      foreach (var statement in context.statement())
      {
        if (Visit(statement) == Type.Error)
          //save to parse 
          return (Type.Error);
      }
      return (Type.Empty);
    }
    public override Type VisitDeclaration([NotNull] project_grammarParser.DeclarationContext context)
    {
      var type = Visit(context.primitiveType());
      if (type == Type.Error)
        return (Type.Error);

      foreach (var identifier in context.IDENTIFIER())
      {
        symbolTable.Add(identifier.Symbol, type);
      }
      return (Type.Empty);
    }
    public override Type VisitPrintExpr([NotNull] project_grammarParser.PrintExprContext context)
    {
      var type = Visit(context.expr());
      if (type != Type.Error)
      {
        Console.WriteLine(type);
        return (Type.Empty);
      }
      else
      {
        Errors.PrintAndClearErrors();
        return (Type.Error);
      }
    }
    public override Type VisitPrimitiveType([NotNull] project_grammarParser.PrimitiveTypeContext context)
    {
      switch (context.type.Text)
      {
        case "int":
          return (Type.Int);
        case "float":
          return (Type.Float);
        case "string":
          return (Type.String);
        case "bool":
          return (Type.Boolean);
        default:
          return (Type.Error);
      }
    }
    public override Type VisitFloat([NotNull] project_grammarParser.FloatContext context)
    {
      return Type.Float;
    }
    public override Type VisitInt([NotNull] project_grammarParser.IntContext context)
    {
      return Type.Int;
    }
    public override Type VisitString([NotNull] project_grammarParser.StringContext context)
    {
      return Type.String;
    }
    public override Type VisitBool([NotNull] project_grammarParser.BoolContext context)
    {
      return Type.Boolean;
    }
    public override Type VisitId([NotNull] project_grammarParser.IdContext context)
    {
      //return type of the variable
      return symbolTable[context.IDENTIFIER().Symbol];
    }
    public override Type VisitParens([NotNull] project_grammarParser.ParensContext context)
    {
      return Visit(context.expr());
    }
    public override Type VisitBlockOfStatements([NotNull] project_grammarParser.BlockOfStatementsContext context)
    {
      Type last = Type.Empty;
      foreach (var statement in context.statement())
      {
        last = Visit(statement);
        if (last == Type.Error)
          return (Type.Error);
      }
      return last;
    }
    public override Type VisitUnaryMinus([NotNull] project_grammarParser.UnaryMinusContext context)
    {
      var type = Visit(context.expr());
      if (type == Type.Int || type == Type.Float)
      {
        return type;
      }
      else
      {
        return Type.Error;
      }


    }
    public override Type VisitAddSubCon([NotNull] project_grammarParser.AddSubConContext context)
    {
      var left = Visit(context.expr()[0]);
      var right = Visit(context.expr()[1]);

      if (left == Type.Error || right == Type.Error)
        return (Type.Error);

      switch (context.op.Type)
      {
        case project_grammarParser.ADD:
          {
            if ((left == Type.String || right == Type.String) || (left == Type.Boolean || right == Type.Boolean))
            {
              Errors.ReportError(context.ADD().Symbol, $"In expression '{left}{context.ADD().GetText()}{right}' you are using wrong operands.");
              Errors.PrintAndClearErrors();
              return (Type.Error);
            }
            if (left == Type.Float || right == Type.Float)
              return (Type.Float);
            else
              return (Type.Int);
          }
        case project_grammarParser.SUB:
          {
            if ((left == Type.String || right == Type.String) || (left == Type.Boolean || right == Type.Boolean))
            {
              Errors.ReportError(context.SUB().Symbol, $"In expression '{left}{context.SUB().GetText()}{right}' you are using wrong operands.");
              Errors.PrintAndClearErrors();
              return (Type.Error);
            }
            if (left == Type.Float || right == Type.Float)
              return (Type.Float);
            else
              return (Type.Int);
          }
        case project_grammarParser.CON:
          {
            if (left == Type.String && right == Type.String)
              return (Type.String);
            else
            {
              Errors.ReportError(context.CON().Symbol, $"In expression '{left}{context.CON().GetText()}{right}' you are using wrong operands.");
              Errors.PrintAndClearErrors();
              return (Type.Error);
            }
          }
        default:
          return (Type.Error);
      }
    }
    public override Type VisitMulDivMod([NotNull] project_grammarParser.MulDivModContext context)
    {
      var left = Visit(context.expr()[0]);
      var right = Visit(context.expr()[1]);
      if (left == Type.Error || right == Type.Error) return (Type.Error);
      switch (context.op.Type)
      {
        case project_grammarParser.MUL:
          {
            if (left == Type.String || right == Type.String)
            {
              Errors.ReportError(context.MUL().Symbol, $"In expression '{left}{context.MUL().GetText()}{right}' you are using wrong operands.");
              Errors.PrintAndClearErrors();
              return (Type.Error);
            }
            if (left == Type.Float || right == Type.Float)
            {
              return (Type.Float);
            }
            else
            {
              return (Type.Int);
            }
          }
        case project_grammarParser.DIV:
          {
            if (left == Type.String || right == Type.String)
            {
              Errors.ReportError(context.DIV().Symbol, $"In expression '{left}{context.DIV().GetText()}{right}' you are using wrong operands.");
              Errors.PrintAndClearErrors();
              return (Type.Error);
            }

            if (left == Type.Float || right == Type.Float)
            {
              if (ToFloat(right) == 0.0)
              {
                Errors.ReportError(context.DIV().Symbol, $"In expression '{left}{context.DIV().GetText()}{right}' you are dividing by zero.");
                Errors.PrintAndClearErrors();
                return (Type.Error);
              }
              else
              {
                return (Type.Float);
              }
            }
            else
            {
             return Type.Int;
              
            }
          }
        case project_grammarParser.MOD:
          {
            if (left == Type.Int && right == Type.Int)
            {
              return (Type.Int);
            }
            else
            {
              Errors.ReportError(context.MOD().Symbol, $"In expression '{left}{context.MOD().GetText()}{right}' you are using wrong operands.");
              Errors.PrintAndClearErrors();
              return (Type.Error);
            }
          }
        default:
          {
            return (Type.Error);
          }

      }
    }
    public override Type VisitAssignment([NotNull] project_grammarParser.AssignmentContext context)
    {
      var right = Visit(context.expr());
      var variable = symbolTable[context.IDENTIFIER().Symbol];
      if (variable == Type.Error || right == Type.Error) return (Type.Error);

      if (variable == right)
      {
        symbolTable[context.IDENTIFIER().Symbol] = right;
        return right;
      }

      if (variable == Type.Float && right == Type.Int)
      {
        //var value = (Type.Float, ToFloat(right));
        //symbolTable[context.IDENTIFIER().Symbol] = value;
        return variable;
      }

      if (variable != right)
      {
        Errors.ReportError(context.IDENTIFIER().Symbol, $"Variable '{context.IDENTIFIER().GetText()}' type is {variable}, but the assigned value is {right}.");
        Errors.PrintAndClearErrors();
        return (Type.Error);
      }

      return (Type.Error);
    }
    public override Type VisitNegation([NotNull] project_grammarParser.NegationContext context)
    {
      var operand = Visit(context.expr());
      if (operand == Type.Error)
        return (Type.Error);
      switch (operand)
      {
        case Type.Boolean:
          {
            return (Type.Boolean);
          }
        default:
          {
            Errors.ReportError(context.NEG().Symbol, $"In expression '{context.NEG().GetText()}{operand}' you are using wrong operands.");
            Errors.PrintAndClearErrors();
            return (Type.Error);
          }
      }
    }
    public override Type VisitLogicalAnd([NotNull] project_grammarParser.LogicalAndContext context)
    {
      var left = Visit(context.expr()[0]);
      var right = Visit(context.expr()[1]);

      if (left == Type.Error || right == Type.Error)
        return (Type.Error);


      if (left != Type.Boolean || right != Type.Boolean)
      {
        Errors.ReportError(context.AND().Symbol, $"In expression '{left}{context.AND().GetText()}{right}' you are using wrong operands.");
        Errors.PrintAndClearErrors();
        return (Type.Error);
      }

      return (Type.Boolean);
    }
    public override Type VisitLogicalOr([NotNull] project_grammarParser.LogicalOrContext context)
    {
      var left = Visit(context.expr()[0]);
      var right = Visit(context.expr()[1]);

      if (left == Type.Error || right == Type.Error)
        return (Type.Error);


      if (left != Type.Boolean || right != Type.Boolean)
      {
        Errors.ReportError(context.OR().Symbol, $"In expression '{left} {context.OR().GetText()} {right}' you are using wrong operands.");
        Errors.PrintAndClearErrors();
        return (Type.Error);
      }

      return (Type.Boolean);
    }
    public override Type VisitComparison([NotNull] project_grammarParser.ComparisonContext context)
    {
      var left = Visit(context.expr()[0]);
      var right = Visit(context.expr()[1]);

      if (left == Type.Error || right == Type.Error)
        return (Type.Error);

      switch (context.op.Type)
      {
        case project_grammarParser.EQ:
          {
            if ((left == Type.Boolean || right == Type.Boolean))
            {
              Errors.ReportError(context.EQ().Symbol, $"In expression '{left}   {context.EQ().GetText()}   {right}' you are using wrong operands.");
              Errors.PrintAndClearErrors();
              return (Type.Error);
            }
            if (left == Type.Float || right == Type.Float)
              return (Type.Boolean);
            else if (left == Type.Int && right == Type.Int)
              return (Type.Boolean);
            else
              return (Type.Boolean);
          }
        case project_grammarParser.NEQ:
          {
            if ((left == Type.Boolean || right == Type.Boolean))
            {
              Errors.ReportError(context.NEQ().Symbol, $"In expression '{left}{context.NEQ().GetText()}{right}' you are using wrong operands.");
              Errors.PrintAndClearErrors();
              return (Type.Error);
            }
            if (left == Type.Float || right == Type.Float)
              return (Type.Boolean);
            else if (left == Type.Int && right == Type.Int)
              return (Type.Boolean);
            else
              return (Type.Boolean);
          }
        default:
          return (Type.Error);
      }
    }
    public override Type VisitRelation([NotNull] project_grammarParser.RelationContext context)
    {
      var left = Visit(context.expr()[0]);
      var right = Visit(context.expr()[1]);

      if (left == Type.Error || right == Type.Error)
        return (Type.Error);

      switch (context.op.Type)
      {
        case project_grammarParser.LES:
          {
            if ((left == Type.String || right == Type.String) || (left == Type.Boolean || right == Type.Boolean))
            {
              Errors.ReportError(context.LES().Symbol, $"In expression '{left}{context.LES().GetText()}{right}' you are using wrong operands.");
              Errors.PrintAndClearErrors();
              return (Type.Error);
            }
            if (left == Type.Float || right == Type.Float)
              return (Type.Boolean);
            else
              return (Type.Boolean);
          }
        case project_grammarParser.GRE:
          {
            if ((left == Type.String || right == Type.String) || (left == Type.Boolean || right == Type.Boolean))
            {
              Errors.ReportError(context.LES().Symbol, $"In expression '{left} {context.GRE().GetText()} {right}' you are using wrong operands.");
              Errors.PrintAndClearErrors();
              return (Type.Error);
            }
            if (left == Type.Float || right == Type.Float)
              return (Type.Boolean);
            else
              return (Type.Boolean);


          }
        default:
          return (Type.Error);
      }
    }
    public override Type VisitIfElse([NotNull] project_grammarParser.IfElseContext context)
    {
      var condition = Visit(context.expr());
      if (condition != Type.Boolean)
      {
        Errors.ReportError(context.IF().Symbol, $"Condition in IF statement must be BOOL");
        Errors.PrintAndClearErrors();
        return (Type.Error);
      }
      //if ((bool)condition)
      //{
      //  return Visit(context.pos);
      //}
      //else
      //{
      //  if (context.neg != null)
      //    return Visit(context.neg);
      //}

      return (Type.Empty);
    }
    public override Type VisitWhile([NotNull] project_grammarParser.WhileContext context)
    {
      var condition = Visit(context.expr());
      if (condition != Type.Boolean)
      {
        Errors.ReportError(context.WHILE().Symbol, $"Condition in WHILE statement must be BOOL");
        Errors.PrintAndClearErrors();
        return (Type.Error);
      }
      //var c = condition;
      //while ((bool)c == true)
      //{
      //  var value = Visit(context.statement());
      //  c = Visit(context.expr());
      //}
      return (Type.Empty);
    }
    public override Type VisitReadStatement([NotNull] project_grammarParser.ReadStatementContext context)
    {
      foreach (var identifier in context.IDENTIFIER())
      {
        var variable = symbolTable[identifier.Symbol];
        string value = Console.ReadLine();

        switch (variable)
        {
          case Type.Boolean:
            {
              if (value.Equals("true"))
                symbolTable[identifier.Symbol] = (Type.Boolean);
              else if (value.Equals("false"))
                symbolTable[identifier.Symbol] = (Type.Boolean);
              else
              {
                Errors.ReportError(context.READ().Symbol, $"You need to enter \"true\" or \"false\" value into boolean variable.");
                Errors.PrintAndClearErrors();
                return (Type.Error);
              }
            }
            break;
          case Type.String:
            {
              symbolTable[identifier.Symbol] = (Type.String);
            }
            break;
          case Type.Int:
            {
              int output;
              if (int.TryParse(value, out output))
                symbolTable[identifier.Symbol] = (Type.Int);
              else
              {
                Errors.ReportError(context.READ().Symbol, $"You need to enter integer value into integer variable.");
                Errors.PrintAndClearErrors();
                return (Type.Error);
              }

            }
            break;
          case Type.Float:
            {
              float output;
              if (float.TryParse(value, out output))
                symbolTable[identifier.Symbol] = (Type.Float);
              else
              {
                Errors.ReportError(context.READ().Symbol, $"You need to enter integer or float value into float variable.");
                Errors.PrintAndClearErrors();
                return (Type.Error);
              }
            }
            break;
          default:
            {
              return (Type.Error);
            }
        }
      }

      return (Type.Empty);
    }
    public override Type VisitWriteStatement([NotNull] project_grammarParser.WriteStatementContext context)
    {
      foreach (var expr in context.expr())
      {
        var valueOfExpr = Visit(expr);

        if (valueOfExpr != Type.Error)
          Console.Write(valueOfExpr);
      }
      Console.Write("\n");

      return (Type.Empty);
    }
    public override Type VisitEmptyStatement([NotNull] project_grammarParser.EmptyStatementContext context)
    {
      return (Type.Empty);
    }
  }
}
