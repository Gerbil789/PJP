using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;

namespace PLC_Lab8
{
  public class EvalVisitor : PLC_Lab8_exprBaseVisitor<(Type Type, object Value)>
  {
    public override (Type Type, object Value) VisitProgram([NotNull] PLC_Lab8_exprParser.ProgramContext context)
    {
      foreach (var statementContext in context.statement())
      {
        Visit(statementContext);
      }
      return (Type.Error, 0);
    }

    public override (Type, object) VisitType([NotNull] PLC_Lab8_exprParser.TypeContext context)
    {
      string typeText = context.GetText();
      switch (typeText)
      {
        case "int":
        case "float":
        case "bool":
        case "string":
          return (Type.Valid, typeText);
        default:
          Console.WriteLine($"Syntax Error: Invalid type '{typeText}'");
          return (Type.Error, null);
      }
    }

    public override (Type, object) VisitBinaryExpr([NotNull] PLC_Lab8_exprParser.BinaryExprContext context)
    {
      var left = Visit(context.expression(0));
      var right = Visit(context.expression(1));

      if (left.Type == Type.Error || right.Type == Type.Error)
      {
        Console.WriteLine("Syntax Error: Invalid operands in binary expression");
        return (Type.Error, null);
      }

      return (Type.Valid, null);
    }

    public override (Type, object) VisitUnaryMinusExpr([NotNull] PLC_Lab8_exprParser.UnaryMinusExprContext context)
    {
      var expr = Visit(context.expression());

      if (expr.Type == Type.Error)
      {
        Console.WriteLine("Syntax Error: Invalid operand in unary minus expression");
        return (Type.Error, null);
      }

      // Check for other syntax errors as needed

      return (Type.Valid, null);
    }

    public override (Type, object) VisitEmptyCommand([NotNull] PLC_Lab8_exprParser.EmptyCommandContext context)
    {
      // No syntax error in an empty command
      return (Type.Valid, null);
    }

    public override (Type, object) VisitDeclarationStatement([NotNull] PLC_Lab8_exprParser.DeclarationStatementContext context)
    {
      string type = context.type().GetText();
      foreach (var identifier in context.ID())
      {
        string variableName = identifier.GetText();

        // Check for valid variable names
        if (!IsValidVariableName(variableName))
        {
          Console.WriteLine($"Syntax Error: Invalid variable name '{variableName}'");
          return (Type.Error, null);
        }

        // Check for duplicate variable declarations
        if (VariableAlreadyDeclared(variableName))
        {
          Console.WriteLine($"Syntax Error: Variable '{variableName}' already declared");
          return (Type.Error, null);
        }

        // Other checks as needed

        // If no syntax error is found, proceed to the next variable
      }

      // No syntax error in declaration statement
      return (Type.Valid, null);
    }

    private HashSet<string> declaredVariables = new HashSet<string>();

    private bool VariableAlreadyDeclared(string variableName)
    {
      return declaredVariables.Contains(variableName);
    }

    private bool IsValidVariableName(string variableName)
    {
      // Check if the variable name starts with a letter
      if (!char.IsLetter(variableName[0]))
      {
        return false;
      }

      // Check if all characters are letters or digits
      for (int i = 1; i < variableName.Length; i++)
      {
        if (!char.IsLetterOrDigit(variableName[i]))
        {
          return false;
        }
      }

      // Variable name is valid
      return true;
    }

    public override (Type, object) VisitExpressionStatement([NotNull] PLC_Lab8_exprParser.ExpressionStatementContext context)
    {
      // Validate the expression
      var exprResult = Visit(context.expression());

      // Check for syntax error in the expression
      if (exprResult.Type == Type.Error)
      {
        Console.WriteLine("Syntax Error: Invalid expression");
        return (Type.Error, null);
      }

      // No syntax error in expression statement
      return (Type.Valid, null);
    }

    public override (Type, object) VisitReadStatement([NotNull] PLC_Lab8_exprParser.ReadStatementContext context)
    {
      foreach (var identifier in context.ID())
      {
        string variableName = identifier.GetText();

        // Check for valid variable names
        if (!IsValidVariableName(variableName))
        {
          Console.WriteLine($"Syntax Error: Invalid variable name '{variableName}'");
          return (Type.Error, null);
        }

        // Check if the variable is declared
        if (!VariableAlreadyDeclared(variableName))
        {
          Console.WriteLine($"Syntax Error: Variable '{variableName}' not declared");
          return (Type.Error, null);
        }

        // Other checks as needed

        // If no syntax error is found, proceed to the next variable
      }

      // No syntax error in read statement
      return (Type.Valid, null);
    }

    public override (Type, object) VisitWriteStatement([NotNull] PLC_Lab8_exprParser.WriteStatementContext context)
    {
      foreach (var exprContext in context.expression())
      {
        var exprResult = Visit(exprContext);

        // Check for syntax error in the expression
        if (exprResult.Type == Type.Error)
        {
          Console.WriteLine("Syntax Error: Invalid expression in write statement");
          return (Type.Error, null);
        }

        // Other checks as needed
      }

      // No syntax error in write statement
      return (Type.Valid, null);
    }

    public override (Type, object) VisitBlock([NotNull] PLC_Lab8_exprParser.BlockContext context)
    {
      foreach (var statementContext in context.statement())
      {
        var statementResult = Visit(statementContext);

        // Check for syntax error in the statement
        if (statementResult.Type == Type.Error)
        {
          Console.WriteLine("Syntax Error: Invalid statement in block");
          return (Type.Error, null);
        }

        // Other checks as needed
      }

      // No syntax error in block
      return (Type.Valid, null);
    }

    //public override (Type, object) VisitIfStatement([NotNull] PLC_Lab8_exprParser.IfStatementContext context)
    //{
    //  // Validate the condition expression
    //  var conditionResult = Visit(context.expression());

    //  // Check for syntax error in the condition expression
    //  if (conditionResult.Type == Type.Error)
    //  {
    //    Console.WriteLine("Syntax Error: Invalid condition expression in if statement");
    //    return (Type.Error, null);
    //  }

    //  // Validate the if block
    //  var ifBlockResult = Visit(context.statement(0));

    //  // Check for syntax error in the if block
    //  if (ifBlockResult.Type == Type.Error)
    //  {
    //    Console.WriteLine("Syntax Error: Invalid if block in if statement");
    //    return (Type.Error, null);
    //  }

    //  // Validate the else block if it exists
    //  if (context.Else != null)
    //  {
    //    var elseBlockResult = Visit(context.statement(1));

    //    // Check for syntax error in the else block
    //    if (elseBlockResult.Type == Type.Error)
    //    {
    //      Console.WriteLine("Syntax Error: Invalid else block in if statement");
    //      return (Type.Error, null);
    //    }
    //  }

    //  // No syntax error in if statement
    //  return (Type.Valid, null);
    //}

    public override (Type, object) VisitWhileStatement([NotNull] PLC_Lab8_exprParser.WhileStatementContext context)
    {
      // Validate the condition expression
      var conditionResult = Visit(context.expression());

      // Check for syntax error in the condition expression
      if (conditionResult.Type == Type.Error)
      {
        Console.WriteLine("Syntax Error: Invalid condition expression in while statement");
        return (Type.Error, null);
      }

      // Validate the while block
      var whileBlockResult = Visit(context.statement());

      // Check for syntax error in the while block
      if (whileBlockResult.Type == Type.Error)
      {
        Console.WriteLine("Syntax Error: Invalid while block in while statement");
        return (Type.Error, null);
      }

      // No syntax error in while statement
      return (Type.Valid, null);
    }

    public override (Type, object) VisitDoWhileStatement([NotNull] PLC_Lab8_exprParser.DoWhileStatementContext context)
    {
      // Validate the condition expression
      var conditionResult = Visit(context.expression());

      // Check for syntax error in the condition expression
      if (conditionResult.Type == Type.Error)
      {
        Console.WriteLine("Syntax Error: Invalid condition expression in do-while statement");
        return (Type.Error, null);
      }

      // Validate the do-while block
      var doWhileBlockResult = Visit(context.statement());

      // Check for syntax error in the do-while block
      if (doWhileBlockResult.Type == Type.Error)
      {
        Console.WriteLine("Syntax Error: Invalid do-while block in do-while statement");
        return (Type.Error, null);
      }

      // No syntax error in do-while statement
      return (Type.Valid, null);
    }

   




  }
}
