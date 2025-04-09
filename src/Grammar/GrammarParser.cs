using System.Text.RegularExpressions;
using SharpParse.Functional;
using SharpParse.Lexing;
using SharpParse.Parsing;

namespace SharpParse.Grammar;

public static class GrammarLexonType
{
  public const string comment = "comment";
  public const string whitespace = "whitespace";
  public const string name = "name";
  public const string equalSign = "equalSign";
  public const string semicolon = "semicolon";
  public const string modifierCharacter = "modifierCharacter";
}

public class GrammarParser
{
  public static readonly (string lexonType, string rule)[] RulesDef =
  [
    (GrammarLexonType.comment, @"^//.*\n"),
    (GrammarLexonType.whitespace, @"^\s+"),
    (GrammarLexonType.name, @"^\w+"),
    (GrammarLexonType.equalSign, @"^="),
    (GrammarLexonType.semicolon, @"^;"),
    (GrammarLexonType.modifierCharacter, @"[*|?]"),
  ];

  private static readonly (string lexonType, Regex regex)[] Rules = RulesDef.Map(
    (rule) => (rule.lexonType, new Regex(rule.rule))
  );

  public static ProductionSet[] ParseGrammar(
    string[] sourceFiles,
    Func<string, string> stringToLexon
  )
  {
    var rules = sourceFiles.FlatMap((x) => Parse(x, stringToLexon));
    Dictionary<string, List<ProductionRule>> dict = new Dictionary<string, List<ProductionRule>>();

    foreach (var rule in rules)
    {
      dict.AddOrGet(rule.name, () => new List<ProductionRule>()).Add(rule);
    }

    return dict.Select((pair) => new ProductionSet(pair.Key, pair.Value.ToArray())).ToArray();
  }

  internal static ProductionRule[] Parse(string code, Func<string, string> stringToLexon)
  {
    var lexons = LexerStatic
      .Lex(
        code,
        Rules,
        (lexonType, code, index) =>
          new Lexon(lexonType, code, lexonType != GrammarLexonType.whitespace, index)
      )
      .Filter(lexon => lexon.isSemantic);
    var queue = new Queue<Lexon>(lexons);

    return GenerateRuleParser(queue, stringToLexon).UntilNull().ToArray();
  }

  static Func<ProductionRule> GenerateRuleParser(
    Queue<Lexon> queue,
    Func<string, string> stringToLexon
  )
  {
    return () => ParseRule(ref queue, stringToLexon);
  }

  static ProductionRule ParseRule(ref Queue<Lexon> queue, Func<string, string> stringToLexon)
  {
    string? name = null;
    bool rulesSection = false;
    string? ruleSymbol = null;
    char? modifier = null;

    List<ProductionSymbol> symbols = new List<ProductionSymbol>();

    while (queue.TryDequeue(out var value))
    {
      switch (value.lexonType)
      {
        case GrammarLexonType.name:
          if (name != null && !rulesSection)
          {
            throw new Exception($"Already parsed a name for this rule {name} {value.sourceCode}");
          }
          else if (name != null)
          {
            if (ruleSymbol != null)
            {
              symbols.Add(new ProductionSymbol(ruleSymbol, stringToLexon(ruleSymbol), modifier));
              modifier = null;
            }
            ruleSymbol = value.sourceCode;
          }
          else
          {
            name = value.sourceCode;
          }
          break;
        case "equalSign":
          if (rulesSection)
          {
            throw new Exception($"More then one definition for rule {name}");
          }
          rulesSection = true;
          break;
        case GrammarLexonType.modifierCharacter:
          if (modifier != null)
          {
            throw new Exception($"Multiple modifier characters for symbol {ruleSymbol}");
          }
          modifier = value.sourceCode[0];
          break;
        case GrammarLexonType.semicolon:
          if (name == null)
          {
            throw new Exception($"Rule with no name");
          }
          if (ruleSymbol != null)
          {
            symbols.Add(new ProductionSymbol(ruleSymbol, stringToLexon(ruleSymbol), modifier));
          }
          return new ProductionRule(name!, symbols.ToArray());
      }
    }
    return null!;
  }
}
