using SharpParse.Functional;
using SharpParse.Lexing;
using SharpParse.Parsing;

/// <summary>
/// Contains a lexer and parser for a particular programming language.
/// </summary>
public class LexerParser
{
  /// <summary>
  /// Grammar for the language.
  /// </summary>
  public readonly LanguageGrammar grammar;

  /// <summary>
  /// The parser that is able to parse this grammar.
  /// </summary>
  public readonly Parser parser;

  public LexerParser(LanguageGrammar grammar, CustomParser[] customParsers)
  {
    this.grammar = grammar;
    parser = new Parser(grammar.productionSets, customParsers);
  }

  /// <summary>
  /// Parse the source code and return the abstract syntax tree.
  /// </summary>
  /// <param name="sourceCode"></param>
  /// <returns></returns>
  public ASTNode Parse(string sourceCode)
  {
    return parser.Parse(grammar.rootSymbol, Lex(sourceCode)).NotNull();
  }

  /// <summary>
  /// Lex the source code and return the lexons.
  /// </summary>
  /// <param name="sourceCode"></param>
  /// <param name="resumeAfterError"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Lex the source code and return the lexons.
  /// </summary>
  /// <param name="sourceCode"></param>
  /// <returns></returns> <summary>
  ///
  /// </summary>
  /// <param name="sourceCode"></param>
  /// <returns></returns>
  public Lexon[] Lex(string sourceCode)
  {
    return LexerStatic.Lex(sourceCode, grammar.lexonRules);
  }

  /// <summary>
  /// Lex the source code and return the lexons. Continue even if errors are found.
  /// </summary>
  /// <param name="code"></param>
  /// <returns></returns>
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
