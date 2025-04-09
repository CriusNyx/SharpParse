using System.Text.RegularExpressions;
using GenParse.Functional;

namespace GenParse.Lexing;

public static class LexerStatic
{
  public static TLexon[] Lex<TLexon, TLexonType>(
    string code,
    (TLexonType ruleType, Regex regex)[] rules,
    Func<TLexonType, string, int, TLexon> lexonConstructor,
    int startIndex = 0
  )
  {
    int index = startIndex;
    if (index != 0)
    {
      code = code.Substring(index);
    }
    List<TLexon> lexons = new List<TLexon>();
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

  internal static string LexonsToSource<LexonType>(Lexon<LexonType>[] lexons, string separator = "")
  {
    return string.Join(separator, lexons.Map(x => x.sourceCode));
  }

  internal static string PrintLexons<LexonType>(Lexon<LexonType>[] lexons)
  {
    var identLength = lexons.Max(x => x.lexonType.ToString()!.Length) + 5;

    string GenerateLexonStrings(Lexon<LexonType> lexons)
    {
      var lexonString = lexons.lexonType.ToString()!.PadRight(identLength);
      var sourceString = lexons.sourceCode;
      return $"{lexonString} \"{sourceString}\"";
    }

    return string.Join('\n', lexons.Map(GenerateLexonStrings));
  }
}
