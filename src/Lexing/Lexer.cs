using System.Text.RegularExpressions;
using GenParse.Lexing;

public abstract class Lexer { }

public class Lexer<TLexonType, TLexon> : Lexer
{
  (TLexonType ruleType, Regex regex)[] lexonRules;
  Func<TLexonType, string, int, TLexon> lexonConstructor;

  public Lexer(
    (TLexonType ruleType, Regex regex)[] lexonRules,
    Func<TLexonType, string, int, TLexon> lexonConstructor
  )
  {
    this.lexonRules = lexonRules;
    this.lexonConstructor = lexonConstructor;
  }

  public TLexon[] Lex(string code, int startIndex = 0)
  {
    return LexerStatic.Lex(code, lexonRules, lexonConstructor, startIndex);
  }
}
