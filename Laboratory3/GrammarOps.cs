using Grammar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab3
{
	public class GrammarOps
	{
		public GrammarOps(IGrammar g)
		{
			this.g = g;
			compute_empty();
      compute_first();
      compute_first_rules();
      compute_follow();
    }

		public ISet<Nonterminal> EmptyNonterminals { get; } = new HashSet<Nonterminal>();
    public Dictionary<Nonterminal, HashSet<Symbol>> FirstSet { get; } = new Dictionary<Nonterminal, HashSet<Symbol>>();
    public Dictionary<Rule, HashSet<Symbol>> FirstSetRules { get; } = new Dictionary<Rule, HashSet<Symbol>>();
    public Dictionary<Nonterminal, HashSet<Symbol>> FollowSet { get; } = new Dictionary<Nonterminal, HashSet<Symbol>>();
    private void compute_empty()
		{
      foreach (var rule in g.Rules)
      {
        if (rule.RHS.Count == 0) EmptyNonterminals.Add(rule.LHS);
      }
      int count;
      do
      {
        count = EmptyNonterminals.Count;
        foreach (var rule in g.Rules)
          if (rule.RHS.All(x => x is Nonterminal && EmptyNonterminals.Contains(x))) EmptyNonterminals.Add(rule.LHS);
      } while (count != EmptyNonterminals.Count);

    }

    private void compute_first()
    {
      foreach (var rule in g.Rules)
      {
        FirstSet[rule.LHS] = new HashSet<Symbol>();
      }

      foreach (var rule in g.Rules)
      {
        if (rule.RHS.Count == 0)
          continue;
        Stack<Rule> stack = new Stack<Rule>();
        var right = rule.RHS[0];
        FirstSet[rule.LHS].Add(right);
        if(EmptyNonterminals.Contains(right) && rule.RHS.Count > 1)
          FirstSet[rule.LHS].Add(rule.RHS[1]);
      }

      bool change;
      do
      {
        change = false;
        foreach (var kv in FirstSet)
        {
          var nts = kv.Value.Where(x => x.GetType() == typeof(Nonterminal));
          List<Symbol> additionalSymbols = new List<Symbol>();
          foreach (var nt in nts)
          {
            foreach(var x in FirstSet[(Nonterminal)nt])
            {
              if (!kv.Value.Contains(x))
              {
                additionalSymbols.Add(x);
                change = true;
              }
            }
          }
          
          foreach(Symbol s in additionalSymbols)
            kv.Value.Add(s);
        }
      } while (change);
    }

    private void compute_first_rules()
    {
      Console.WriteLine("Starto");
      foreach (var rule in g.Rules)
      {
        if(rule.RHS.Count == 0)
        {
          FirstSetRules[rule] = new HashSet<Symbol>();
          continue;
        }
         
        var r = rule.RHS[0];
        if (r.GetType() == typeof(Nonterminal))
          FirstSetRules[rule] = new HashSet<Symbol>(FirstSet[(Nonterminal)r].Where(x => x.GetType() == typeof(Terminal)));
        else
          FirstSetRules[rule] = new HashSet<Symbol> { r };
      }
    }

    private void compute_follow()
    {
      foreach (var rule in g.Rules)
        if(!FollowSet.ContainsKey(rule.LHS))
          FollowSet[rule.LHS] = new HashSet<Symbol>();
      FollowSet[g.StartingNonterminal].Add(new Terminal("$"));

      foreach (var kv in FollowSet)
      {
        var relevantRules = g.Rules.Where(x => x.RHS.Contains(kv.Key)).ToList();
        foreach (var rule in relevantRules)
        {
          int index = rule.RHS.IndexOf(kv.Key);

          if (rule.RHS.Count - 1 == index)
            kv.Value.Add(rule.LHS);
          else
          {
            var r = rule.RHS[index + 1];
            if(r.GetType() == typeof(Terminal))
              kv.Value.Add(r);
            else
            {
              foreach (var t in FirstSet[r as Nonterminal])
                kv.Value.Add(t);
            }
          }
        }
      }


      bool change;
      do
      {
        change = false;
        foreach (var kv in FollowSet)
        {
          var nts = kv.Value.Where(x => x.GetType() == typeof(Nonterminal));
          List<Symbol> additionalSymbols = new List<Symbol>();
          foreach (var nt in nts)
          {
            foreach (var x in FollowSet[(Nonterminal)nt])
            {
              if (!kv.Value.Contains(x))
              {
                additionalSymbols.Add(x);
                change = true;
              }
            }
          }

          foreach (Symbol s in additionalSymbols)
            kv.Value.Add(s);
        }
      } while (change);

    }

  private IGrammar g;
	}
}
