using SharpParse.Functional;

namespace SharpParse.Lexing;

/// <summary>
/// Static lexer.
/// </summary>
public static class LexerStatic
{
  /// <summary>
  /// Scan a source code string and find all lexons.
  /// </summary>
  /// <param name="code"></param>
  /// <param name="rules"></param>
  /// <param name="startIndex"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Attempt to scan for a lexon and return it.
  /// </summary>
  /// <param name="code"></param>
  /// <param name="lexon"></param>
  /// <param name="remainingCode"></param>
  /// <param name="index"></param>
  /// <param name="rules"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Convert a set of lexons back into source code.
  /// </summary>
  /// <param name="lexons"></param>
  /// <param name="separator"></param>
  /// <returns></returns>
  internal static string LexonsToSource(Lexon[] lexons, string separator = "")
  {
    return string.Join(separator, lexons.Map(x => x.sourceCode));
  }

  /// <summary>
  /// Print all lexons
  /// </summary>
  /// <param name="lexons"></param>
  /// <typeparam name="LexonType"></typeparam>
  /// <returns></returns>
  internal static string PrintLexons(Lexon[] lexons)
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
