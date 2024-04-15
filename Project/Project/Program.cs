using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using PJP_Project;

namespace Project
{
    public class Program
    {
        static string fileName = "input3.txt";
        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Console.WriteLine($"Parsing: {fileName}");
            var inputFile = new StreamReader(fileName);
            AntlrInputStream input = new AntlrInputStream(inputFile);
            project_grammarLexer lexer = new(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            project_grammarParser parser = new(tokens);
            parser.AddErrorListener(new VerboseListener());
            IParseTree tree = parser.program();

            if (parser.NumberOfSyntaxErrors != 0)
            {
                Console.WriteLine("Syntax errors found. Exiting program.");
                return;
            }

            var typeCheck = new EvalVisitor().Visit(tree);
            if (typeCheck.Type == Type.Error)
            {
                Console.WriteLine("Type errors found. Exiting program.");
                return;
            }


            Console.WriteLine("\n----------------CODE GENERATION--------------\n");
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(new CodeGeneratorListener(), tree);

            Console.WriteLine("\n------------------INTERPRETER----------------\n");
            Interpreter interpreter = new Interpreter("output.txt");
            interpreter.Run();
        }
    }
}