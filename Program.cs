using System;
using System.Collections.Generic;

class Program
{
  static void Main(string[] args)
  {
    int N = int.Parse(Console.ReadLine());
    for (int i = 0; i < N; i++)
    {
      string expression = Console.ReadLine();
      try
      {
        int result = EvaluateExpression(expression);
        Console.WriteLine(result);
      }
      catch (Exception)
      {
        Console.WriteLine("ERROR");
      }
    }
  }

  static int EvaluateExpression(string expression)
  {
    expression = expression.Replace(" ", ""); // Remove whitespace
    Queue<char> tokens = new Queue<char>(expression);
    return EvaluateAdditionSubtraction(tokens);
  }

  static int EvaluateAdditionSubtraction(Queue<char> tokens)
  {
    int result = EvaluateMultiplicationDivision(tokens);
    while (tokens.Count > 0)
    {
      char op = tokens.Peek();
      if (op == '+' || op == '-')
      {
        tokens.Dequeue();
        int nextOperand = EvaluateMultiplicationDivision(tokens);
        if (op == '+')
          result += nextOperand;
        else
          result -= nextOperand;
      }
      else
      {
        break;
      }
    }
    return result;
  }

  static int EvaluateMultiplicationDivision(Queue<char> tokens)
  {
    int result = EvaluateOperand(tokens);
    while (tokens.Count > 0)
    {
      char op = tokens.Peek();
      if (op == '*' || op == '/')
      {
        tokens.Dequeue();
        int nextOperand = EvaluateOperand(tokens);
        if (op == '*')
          result *= nextOperand;
        else
          result /= nextOperand;
      }
      else
      {
        break;
      }
    }
    return result;
  }

  static int EvaluateOperand(Queue<char> tokens)
  {
    if (tokens.Count == 0)
      throw new ArgumentException("Expression ended unexpectedly.");

    char c = tokens.Dequeue();
    if (c == '(')
    {
      int result = EvaluateAdditionSubtraction(tokens);
      if (tokens.Count == 0 || tokens.Dequeue() != ')')
        throw new ArgumentException("Closing parenthesis expected.");
      return result;
    }
    else if (char.IsDigit(c))
    {
      return c - '0';
    }
    else
    {
      throw new ArgumentException("Invalid character in expression.");
    }
  }
}
