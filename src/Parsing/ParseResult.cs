using SharpParse.Functional;
using SharpParse.Lexing;

namespace SharpParse.Parsing;

public abstract class ParseResult { }

public class SuccessParseResult(ASTNode astNode, int lexonsConsumed, ParseResult? hangingNode)
  : ParseResult
{
  public readonly ASTNode astNode = astNode;
  public readonly int lexonsConsumed = lexonsConsumed;

  /// <summary>
  /// If the parser has not consumed all lexons track the last parse result scanned.
  /// </summary>
  public readonly ParseResult? hangingNode = hangingNode;
}

public class FailedParseResult(Lexon? offendingLexon, string[] expectedLexons) : ParseResult
{
  public readonly Lexon? offendingLexon = offendingLexon;
  public readonly string[] expectedLexons = expectedLexons;

  public static FailedParseResult Aggregate(FailedParseResult[] failedResults)
  {
    Lexon? lastLexon = null;
    if (!failedResults.Any(x => x == null))
    {
      lastLexon = failedResults.Map(x => x.offendingLexon).MaxBy(x => x?.index);
    }
    var relevantResults = failedResults.Filter(x => x.offendingLexon == lastLexon);
    var expected = relevantResults.FlatMap(x => x.expectedLexons).Distinct().ToArray();
    return new FailedParseResult(lastLexon, expected);
  }

  public string ErrorMessage()
  {
    return $"Unexpected symbol {offendingLexon}, expected {string.Join(", ", expectedLexons)}";
  }
}
