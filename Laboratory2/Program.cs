// Laboratory 2 : Lexical analyzer
// RUB0031
// 27.2.2024


using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Laboratory2;

class Program
{
  static void Main(string[] args)
  {
    var input = ReadInput();

    List<string> cleanedLines = new List<string>();

    string[] lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

    foreach (string line in lines)
    {
      string cleanedLine = Regex.Replace(line, @"//.*|[\s\t\n\r]", "");
      if (!string.IsNullOrWhiteSpace(cleanedLine))
      {
        cleanedLines.Add(cleanedLine);
      }
    }

    var tokens = LexicalAnalyzer.Analyze(cleanedLines);

    foreach (var token in tokens)
    {
      Console.WriteLine(token);
    }

    Console.ReadLine();

  }

  static string ReadInput()
  {
    StringBuilder inputBuilder = new StringBuilder();
    string line;

    while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
    {
      inputBuilder.AppendLine(line);
    }

    string input = inputBuilder.ToString();
    return input;
  }




}
