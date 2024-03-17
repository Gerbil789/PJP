using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laboratory2
{
  public enum TokenType
  {
    IDENTIFIER,
    NUMBER,
    OPERATOR,
    DELIMITER,
    KEYWORD,
    UNKNOWN
  }


  public class Token
  {
    public TokenType Type { get; }
    public string Value { get; }

    public Token(TokenType type, string value = null)
    {
      Type = type;
      Value = value;
    }

    public override string ToString()
    {
      return $"{Type}:{Value}";
    }
  }
}
