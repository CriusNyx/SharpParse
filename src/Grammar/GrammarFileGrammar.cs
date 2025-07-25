using System.Text.RegularExpressions;
using SharpParse.Parsing;

namespace SharpParse;

/// <summary>
/// Set of lexons used to lex grammar specifications.
/// </summary>
public static class GrammarLexonType
{
  public const string comment = "comment";
  public const string exclamation = "exclamation";
  public const string whitespace = "whitespace";
  public const string name = "name";
  public const string regex = "regex";
  public const string equalSign = "equalSign";
  public const string semicolon = "semicolon";
  public const string modifierCharacter = "modifierCharacter";
}

/// <summary>
/// Set of lexon rules for parsing grammar specifications.
/// </summary>
public static class GrammarLexonRules
{
  public const string modifierCharacterRegex = @"^[\*|\?]";

  /// <summary>
  /// Specification for grammar lexons.
  /// </summary>
  /// <returns></returns>
  public static readonly LexonRule[] LexonRules =
  [
    new LexonRule(GrammarLexonType.comment, false, new Regex(@"^#.*\n")),
    new LexonRule(GrammarLexonType.whitespace, false, new Regex(@"^\s+")),
    new LexonRule(GrammarLexonType.regex, true, new Regex(@"^/.*/(?=[;|\s])")),
    new LexonRule(GrammarLexonType.name, true, new Regex(@"^\w+")),
    new LexonRule(GrammarLexonType.equalSign, true, new Regex(@"^=")),
    new LexonRule(GrammarLexonType.semicolon, true, new Regex(@"^;")),
    new LexonRule(GrammarLexonType.exclamation, true, new Regex(@"^!")),
    new LexonRule(GrammarLexonType.modifierCharacter, true, new Regex(@"^[*|?]")),
  ];
}

/// <summary>
/// Grammar specification for grammar files.
/// The grammar for grammar files is the same as the following.
///
/// Grammar = GrammarStatement*;
/// GrammarStatement = LexonRule | Production Rule;
/// LexonRule = name modifierCharacter? equalSign regex semicolon;
/// ProductionRule = name equalSign ProductionSymbol;
/// ProductionSymbol = name modifierCharacter*;
/// </summary>
public static class GrammarFileGrammar
{
  /// <summary>
  /// Rule for grammar specification grammar.
  /// </summary>
  public static class RuleNames
  {
    public const string Grammar = "Grammar";
    public const string GrammarStatement = "GrammarStatement";
    public const string LexonRule = "LexonRule";
    public const string ProductionRule = "ProductionRule";
    public const string ProductionSymbol = "ProductionSymbol";
  }

  /// <summary>
  /// Production rules for grammar files.
  /// </summary>
  /// <returns></returns>
  public static ProductionRule[] ProductionRules =
  [
    new ProductionRule(RuleNames.Grammar, "GrammarStatement*"),
    new ProductionRule(RuleNames.GrammarStatement, "LexonRule"),
    new ProductionRule(RuleNames.GrammarStatement, "ProductionRule"),
    new ProductionRule(
      RuleNames.LexonRule,
      "name",
      "modifierCharacter?",
      "equalSign",
      "regex",
      "semicolon"
    ),
    new ProductionRule(
      RuleNames.ProductionRule,
      "exclamation?",
      "name",
      "equalSign",
      "ProductionSymbol*",
      "semicolon"
    ),
    new ProductionRule(RuleNames.ProductionSymbol, "name", "modifierCharacter?"),
  ];
}
