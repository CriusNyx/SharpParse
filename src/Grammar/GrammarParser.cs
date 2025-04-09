using System.Text.RegularExpressions;
using SharpParse.Functional;
using SharpParse.Lexing;
using SharpParse.Parsing;

namespace SharpParse.Grammar;

public enum GrammarLexonType
{
  comment,
  whitespace,
  name,
  equalSign,
  semicolon,
  modifierCharacter,
}

public class GrammarParser
{
  public static readonly (GrammarLexonType lexonType, string rule)[] RulesDef =
  [
    (GrammarLexonType.comment, @"^//.*\n"),
    (GrammarLexonType.whitespace, @"^\s+"),
    (GrammarLexonType.name, @"^\w+"),
    (GrammarLexonType.equalSign, @"^="),
    (GrammarLexonType.semicolon, @"^;"),
    (GrammarLexonType.modifierCharacter, @"[*|?]"),
  ];

  private static readonly (GrammarLexonType lexonType, Regex regex)[] Rules = RulesDef.Map(
    (rule) => (rule.lexonType, new Regex(rule.rule))
  );

  public static ProductionSet<LanguageLexonType>[] ParseGrammar<LanguageLexonType>(
    string[] sourceFiles,
    Func<string, LanguageLexonType> stringToLexon
  )
  {
    var rules = sourceFiles.FlatMap((x) => Parse(x, stringToLexon));
    Dictionary<string, List<ProductionRule<LanguageLexonType>>> dict =
      new Dictionary<string, List<ProductionRule<LanguageLexonType>>>();

    foreach (var rule in rules)
    {
      dict.AddOrGet(rule.name, () => new List<ProductionRule<LanguageLexonType>>()).Add(rule);
    }

    return dict.Select(
        (pair) => new ProductionSet<LanguageLexonType>(pair.Key, pair.Value.ToArray())
      )
      .ToArray();
  }

  internal static ProductionRule<LanguageLexonType>[] Parse<LanguageLexonType>(
    string code,
    Func<string, LanguageLexonType> stringToLexon
  )
  {
    var lexons = LexerStatic
      .Lex(code, Rules, (lexonType, code, index) => new GrammarLexon(lexonType, code))
      .Filter(lexon => lexon.IsSemantic);
    var queue = new Queue<GrammarLexon>(lexons);

    return GenerateRuleParser(queue, stringToLexon).UntilNull().ToArray();
  }

  static Func<ProductionRule<LanguageLexonType>> GenerateRuleParser<LanguageLexonType>(
    Queue<GrammarLexon> queue,
    Func<string, LanguageLexonType> stringToLexon
  )
  {
    return () => ParseRule(ref queue, stringToLexon);
  }

  static ProductionRule<LanguageLexonType> ParseRule<LanguageLexonType>(
    ref Queue<GrammarLexon> queue,
    Func<string, LanguageLexonType> stringToLexon
  )
  {
    string? name = null;
    bool rulesSection = false;
    string? ruleSymbol = null;
    char? modifier = null;

    List<ProductionSymbol<LanguageLexonType>> symbols =
      new List<ProductionSymbol<LanguageLexonType>>();

    while (queue.TryDequeue(out var value))
    {
      switch (value.lexonType)
      {
        case GrammarLexonType.name:
          if (name != null && !rulesSection)
          {
            throw new Exception($"Already parsed a name for this rule {name} {value.code}");
          }
          else if (name != null)
          {
            if (ruleSymbol != null)
            {
              symbols.Add(
                new ProductionSymbol<LanguageLexonType>(
                  ruleSymbol,
                  stringToLexon(ruleSymbol),
                  modifier
                )
              );
              modifier = null;
            }
            ruleSymbol = value.code;
          }
          else
          {
            name = value.code;
          }
          break;
        case GrammarLexonType.equalSign:
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
          modifier = value.code[0];
          break;
        case GrammarLexonType.semicolon:
          if (name == null)
          {
            throw new Exception($"Rule with no name");
          }
          if (ruleSymbol != null)
          {
            symbols.Add(
              new ProductionSymbol<LanguageLexonType>(
                ruleSymbol,
                stringToLexon(ruleSymbol),
                modifier
              )
            );
          }
          return new ProductionRule<LanguageLexonType>(name!, symbols.ToArray());
      }
    }
    return null!;
  }
}

class GrammarLexon
{
  public readonly GrammarLexonType lexonType;
  public readonly string code;

  public bool IsSemantic =>
    lexonType != GrammarLexonType.comment && lexonType != GrammarLexonType.whitespace;

  public GrammarLexon(GrammarLexonType lexonType, string code)
  {
    this.lexonType = lexonType;
    this.code = code;
  }
}
