using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var fileName = "input1.txt";
            Console.WriteLine("Parsing: " + fileName);
            var inputFile = new StreamReader("Inputs/" + fileName);

            AntlrInputStream input = new(inputFile);
            Project_GrammarLexer lexer = new(input);
            CommonTokenStream tokens = new(lexer);
            Project_GrammarParser parser = new(tokens);

            parser.AddErrorListener(new VerboseListener());

            IParseTree tree = parser.program();

            if (parser.NumberOfSyntaxErrors == 0)
            {
                //Console.WriteLine(tree.ToStringTree(parser));

                new EvalVisitor().Visit(tree);
            }
        }
    }
}