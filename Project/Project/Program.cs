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
        static string fileName = "input4.txt";
        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var inputFile = new StreamReader(fileName);
            AntlrInputStream input = new AntlrInputStream(inputFile);
            project_grammarLexer lexer = new(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            project_grammarParser parser = new(tokens);
            parser.AddErrorListener(new VerboseListener());


            Console.WriteLine("----------------SYNTAX ERRORS--------------");
            IParseTree tree = parser.program();
           
            if (parser.NumberOfSyntaxErrors != 0)
                return;
            else
                Console.WriteLine("No syntax errors");


            Console.WriteLine("\n----------------TYPE ERRORS--------------");
            if (new EvalVisitor().Visit(tree) == Type.Error) 
                return;
            else
                Console.WriteLine("No type errors");


            Console.WriteLine("\n----------------MACHINE CODE--------------");
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(new CodeGeneratorListener(), tree);

        }
    }
}