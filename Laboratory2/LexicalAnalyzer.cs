using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Laboratory2;

namespace Laboratory2
{
  public class LexicalAnalyzer
  {


    private static Dictionary<char, TokenType> OperatorTypes = new Dictionary<char, TokenType>()
    {
        { '+', TokenType.OPERATOR },
        { '-', TokenType.OPERATOR },
        { '*', TokenType.OPERATOR },
        { '/', TokenType.OPERATOR }
    };

    private static Dictionary<string, TokenType> KeywordTypes = new Dictionary<string, TokenType>()
    {
        { "div", TokenType.KEYWORD },
        { "mod", TokenType.KEYWORD }
    };

    private static Dictionary<char, TokenType> DelimiterTypes = new Dictionary<char, TokenType>()
    {
        { '(', TokenType.DELIMITER },
        { ')', TokenType.DELIMITER },
        { ';', TokenType.DELIMITER }
    };

    public static List<Token> Analyze(List<string> lines)
    {
      List<Token> tokens = new List<Token>();

      foreach(var line in lines)
      {
        tokens.AddRange(AnalyzeLine(line));
      }

      return tokens;
    }

    private static List<Token> AnalyzeLine(string line)
    {
      List<Token> tokens = new List<Token>();
      var currentToken = new Token(TokenType.UNKNOWN);
      var currentValue = string.Empty;

      foreach (char c in line)
      {
        if (char.IsWhiteSpace(c))
        {
          if (currentToken.Type != TokenType.UNKNOWN)
          {
            tokens.Add(new Token(currentToken.Type, currentValue));
            currentToken = new Token(TokenType.UNKNOWN);
            currentValue = string.Empty;
          }
          continue;
        }
        else if (OperatorTypes.ContainsKey(c))
        {
          if (currentToken.Type != TokenType.UNKNOWN)
          {
            tokens.Add(new Token(currentToken.Type, currentValue));
            currentToken = new Token(TokenType.UNKNOWN);
            currentValue = string.Empty;
          }
          tokens.Add(new Token(OperatorTypes[c], c.ToString()));
        }
        else if (DelimiterTypes.ContainsKey(c))
        {
          if (currentToken.Type != TokenType.UNKNOWN)
          {
            tokens.Add(new Token(currentToken.Type, currentValue));
            currentToken = new Token(TokenType.UNKNOWN);
            currentValue = string.Empty;
          }
          tokens.Add(new Token(DelimiterTypes[c], c.ToString()));
        }
        else
        {
          currentValue += c;
          if (KeywordTypes.ContainsKey(currentValue))
          {
            if (currentToken.Type != TokenType.UNKNOWN)
            {
              tokens.Add(new Token(currentToken.Type, currentValue));
              currentToken = new Token(TokenType.UNKNOWN);
              currentValue = string.Empty;
            }
            tokens.Add(new Token(KeywordTypes[currentValue], currentValue));
            currentValue = string.Empty;
          }
          else if (OperatorTypes.ContainsKey(c))
          {
            if (currentToken.Type != TokenType.UNKNOWN)
            {
              tokens.Add(new Token(currentToken.Type, currentValue));
              currentToken = new Token(TokenType.UNKNOWN);
              currentValue = string.Empty;
            }
            tokens.Add(new Token(OperatorTypes[c], c.ToString()));
          }
          else if (DelimiterTypes.ContainsKey(c))
          {
            if (currentToken.Type != TokenType.UNKNOWN)
            {
              tokens.Add(new Token(currentToken.Type, currentValue));
              currentToken = new Token(TokenType.UNKNOWN);
              currentValue = string.Empty;
            }
            tokens.Add(new Token(DelimiterTypes[c], c.ToString()));
          }
        }

      }

      return tokens;
    }
  }
}
