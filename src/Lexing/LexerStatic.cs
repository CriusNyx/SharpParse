using System.Text.RegularExpressions;
using SharpParse.Functional;

namespace SharpParse.Lexing;

public static class LexerStatic
{
  public static Lexon[] Lex(
    string code,
    (string ruleType, Regex regex)[] rules,
    Func<string, string, int, Lexon> lexonConstructor,
    int startIndex = 0
  )
  {
    int index = startIndex;
    if (index != 0)
    {
      code = code.Substring(index);
    }
    List<Lexon> lexons = new List<Lexon>();
    while (TryLex(code, out var lexonType, out var lexicalString, out code!, rules))
    {
      lexons.Add(lexonConstructor(lexonType!, lexicalString!, index));
      index += lexicalString!.Length;
    }

    return lexons.ToArray();
  }

  static bool TryLex<T>(
    string code,
    out T? lexonType,
    out string? lexicalString,
    out string? remainingCode,
    (T ruleType, Regex regex)[] rules
  )
  {
    foreach (var rule in rules)
    {
      var result = rule.regex.Match(code);
      if (result.Success)
      {
        lexonType = rule.ruleType;
        lexicalString = code.Substring(0, result.Length);
        remainingCode = code.Substring(result.Length);
        return true;
      }
    }

    lexonType = default;
    lexicalString = null;
    remainingCode = null;
    return false;
  }

  internal static string LexonsToSource(Lexon[] lexons, string separator = "")
  {
    return string.Join(separator, lexons.Map(x => x.sourceCode));
  }

  internal static string PrintLexons<LexonType>(Lexon[] lexons)
  {
    var identLength = lexons.Max(x => x.lexonType.ToString()!.Length) + 5;

    string GenerateLexonStrings(Lexon lexons)
    {
      var lexonString = lexons.lexonType.ToString()!.PadRight(identLength);
      var sourceString = lexons.sourceCode;
      return $"{lexonString} \"{sourceString}\"";
    }

    return string.Join('\n', lexons.Map(GenerateLexonStrings));
  }
}
