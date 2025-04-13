using System.Text.RegularExpressions;
using SharpParse.Functional;

namespace SharpParse.Lexing;

public static class LexerStatic
{
  public static Lexon[] Lex(string code, LexonRule[] rules, int startIndex = 0)
  {
    int index = startIndex;
    if (index != 0)
    {
      code = code.Substring(index);
    }
    List<Lexon> lexons = new List<Lexon>();
    while (TryLex(code, out var lexon, out code!, index, rules))
    {
      lexons.Add(lexon);
      index += lexon.sourceCode.Length;
    }

    return lexons.ToArray();
  }

  static bool TryLex(
    string code,
    out Lexon lexon,
    out string? remainingCode,
    int index,
    LexonRule[] rules
  )
  {
    foreach (var rule in rules)
    {
      var result = rule.regex.Match(code);
      if (result.Success)
      {
        var lexonType = rule.name;
        var lexicalString = code.Substring(0, result.Length);
        remainingCode = code.Substring(result.Length);
        lexon = new Lexon(lexonType, lexicalString, rule.isSemantic, index);
        return true;
      }
    }
    lexon = null!;
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
