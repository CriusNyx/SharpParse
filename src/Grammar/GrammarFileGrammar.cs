using System.Text.RegularExpressions;
using SharpParse.Functional;
using SharpParse.Parsing;

namespace SharpParse;

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

public static class GrammarFileGrammar
{
  public static class RuleNames
  {
    public const string Grammar = "Grammar";
    public const string GrammarStatement = "GrammarStatement";
    public const string LexonRule = "LexonRule";
    public const string ProductionRule = "ProductionRule";
    public const string ProductionSymbol = "ProductionSymbol";
  }

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
