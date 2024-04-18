using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace PJP_Project
{
    public class Program
    {
        static string fileName = "input1.txt";
        public static void Main(string[] args)
        {

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var inputFile = new StreamReader(fileName);
            AntlrInputStream input = new AntlrInputStream(inputFile);
            project_grammarLexer lexer = new(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            project_grammarParser parser = new(tokens);
            parser.AddErrorListener(new VerboseListener());


            Console.WriteLine("----------------SYNTAX ERRORS------------");
            IParseTree tree = parser.program();

            if (parser.NumberOfSyntaxErrors != 0)
                return;
            else
                Console.WriteLine("Ok!");


            Console.WriteLine("\n----------------TYPE ERRORS--------------");
            var typeCheck = new TypeCheckListener();
            var walker = new ParseTreeWalker();
            walker.Walk(typeCheck, tree);

            if (Errors.NumberOfErrors != 0)
            {
                Errors.PrintAndClearErrors();
                return;
            }
            else
                Console.WriteLine("Ok!");


            Console.WriteLine("\n----------------MACHINE CODE-------------");

            var generator = new CodeGeneratorListener(typeCheck.types, typeCheck.symbolTable);
            walker.Walk(generator, tree);

            if (Errors.NumberOfErrors != 0)
            {
                Errors.PrintAndClearErrors();
                return;
            }
            else
                Console.WriteLine("Ok!");


            Console.WriteLine("\n----------------INTERPRETER--------------");
            try
            {
                var intertpreter = new Interpreter("output.txt");
                intertpreter.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine("\n----------------RUNTIME ERROR------------\n");
                Console.WriteLine(e.Message);
                Console.WriteLine("\n-----------------------------------------");
            }

        }
    }
}