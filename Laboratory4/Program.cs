using System;
using System.Collections.Generic;
using System.Linq;

class LL1GrammarChecker
{
    static Dictionary<char, HashSet<char>> FIRST;
    static Dictionary<char, HashSet<char>> FOLLOW;
    static Dictionary<char, List<string>> productions;

    static void Main(string[] args)
    {
        string input;
        //input = Console.ReadLine();

        input = "{Input grammar}" +
               "A : b C | B d;" +
               "B : C C | b A;" +
               "C : c C | {e};";

        ParseGrammar(input);

    }

    static void ParseGrammar(string input)
    {
        FIRST = new Dictionary<char, HashSet<char>>();
        FOLLOW = new Dictionary<char, HashSet<char>>();
        productions = new Dictionary<char, List<string>>();

        //split the input into lines
        string[] lines = input.Split(';');
        //remove empty lines
        lines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

        foreach (string line in lines)
        {
            string[] parts = line.Trim().Split(':');
            char nonTerminal = parts[0].Trim()[0];
            string[] rules = parts[1].Trim().Split('|').Select(r => r.Trim()).ToArray();

            if (!productions.ContainsKey(nonTerminal))
                productions[nonTerminal] = new List<string>();

            foreach (string rule in rules)
                productions[nonTerminal].Add(rule);
        }
    }



    static bool IsTerminal(char symbol)
    {
        return char.IsLower(symbol) || symbol == '$';
    }

   

}

