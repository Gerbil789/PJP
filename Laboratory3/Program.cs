using Grammar;
using System;
using System.IO;
using System.Linq;

namespace Lab3
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                StreamReader r = new StreamReader(new FileStream("G1.TXT", FileMode.Open));

                GrammarReader inp = new GrammarReader(r);
                var grammar = inp.Read();
                grammar.dump();

                GrammarOps gr = new GrammarOps(grammar);

                // First step, computes nonterminals that can be rewritten as empty word
                Console.WriteLine("EmptyNonterminals");
                foreach (Nonterminal nt in gr.EmptyNonterminals)
                {
                    Console.Write(nt.Name + " ");
                }
                Console.WriteLine();

                Console.WriteLine("FirstSet");
                foreach (var nt in gr.FirstSet)
                {
                    Console.Write(nt.Key.Name + "->");
                    foreach (var nt2 in nt.Value.Where(x => x.GetType() == typeof(Terminal)))
                    {
                        Console.Write(nt2.Name + ", ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();

                Console.WriteLine("FirstSetRules");
                foreach (var kv in gr.FirstSetRules)
                {
                    Console.WriteLine(kv.Key);
                    foreach (var v in kv.Value)
                        Console.Write(v.Name + ", ");
                    Console.WriteLine();
                }
                Console.WriteLine();

                Console.WriteLine("FollowSet");
                foreach (var nt in gr.FollowSet)
                {
                    Console.Write(nt.Key.Name + "->");
                    foreach (var nt2 in nt.Value.Where(x => x.GetType() == typeof(Terminal)))
                    {
                        Console.Write(nt2.Name + ", ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            catch (GrammarException e)
            {
                Console.WriteLine($"{e.LineNumber}: Error -  {e.Message}");
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}