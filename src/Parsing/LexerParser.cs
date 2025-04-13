using SharpParse.Functional;
using SharpParse.Lexing;
using SharpParse.Parsing;

public class LexerParser
{
  public readonly LanguageGrammar grammar;
  public readonly Parser parser;

  public LexerParser(LanguageGrammar grammar, CustomParser[] customParsers)
  {
    this.grammar = grammar;
    parser = new Parser(grammar.productionSets, customParsers);
  }

  public ASTNode Parse(string sourceCode)
  {
    return parser.Parse(grammar.rootSymbol, Lex(sourceCode)).NotNull();
  }

  public Lexon[] Lex(string sourceCode, bool resumeAfterError)
  {
    if (resumeAfterError)
    {
      return LexWithErrors(sourceCode);
    }
    else
    {
      return Lex(sourceCode);
    }
  }

  public Lexon[] Lex(string sourceCode)
  {
    return LexerStatic.Lex(sourceCode, grammar.lexonRules);
  }

  public Lexon[] LexWithErrors(string code)
  {
    List<Lexon> list = new List<Lexon>();
    int index = 0;
    while (index < code.Length)
    {
      var lexons = LexerStatic.Lex(code, grammar.lexonRules, index);
      list.AddRange(lexons);
      var last = lexons.LastOrDefault();
      index = Math.Max(last?.end ?? 0 + 1, index + 1);
    }
    return list.ToArray();
  }
}
