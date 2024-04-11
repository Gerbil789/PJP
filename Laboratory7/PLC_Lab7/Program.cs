
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace PLC_Lab7
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var fileName = "input.txt";
            Console.WriteLine("Parsing: " + fileName);
            var inputFile = new StreamReader(fileName);
            AntlrInputStream input = new AntlrInputStream(inputFile);

            // Create a lexer to tokenize the input
            PLC_Lab7_exprLexer lexer = new PLC_Lab7_exprLexer(input);

            // Create a token stream from the lexer
            CommonTokenStream tokens = new CommonTokenStream(lexer);

            // Create a parser to parse the token stream
            PLC_Lab7_exprParser parser = new PLC_Lab7_exprParser(tokens);

            // Add a listener to handle parse errors
            parser.AddErrorListener(new VerboseListener());

            // Parse the input and get the parse tree
            IParseTree tree = parser.prog();

            // If there are no syntax errors, proceed with tree traversal
            if (parser.NumberOfSyntaxErrors == 0)
            {
                //Console.WriteLine(tree.ToStringTree(parser));
                ParseTreeWalker walker = new ParseTreeWalker();
                walker.Walk(new EvalListener(), tree);

                new EvalVisitor().Visit(tree);
            }
        }
    }
}