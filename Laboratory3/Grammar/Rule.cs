using System.Collections.Generic;
using System.Text;

namespace Grammar
{

	public class Rule
	{

		public Rule(Nonterminal lhs)
		{
			this.LHS = lhs;
		}

		public Nonterminal LHS { get; init; }

		public IList<Symbol> RHS { get; } = new List<Symbol>();

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(LHS.Name);
			sb.Append(" -> ");
			foreach (Symbol sym in RHS)
			{
				sb.Append(sym.Name);
				sb.Append(", ");
			}
      return sb.ToString();
    }
  }
}