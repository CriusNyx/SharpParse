using GenParse.Functional;
using GenParse.Lexing;

namespace GenParse.Parsing;

public abstract class ParseResult<LexonType> { }

public class SuccessParseResult<LexonType>(
  ASTNode<LexonType> astNode,
  int lexonsConsumed,
  ParseResult<LexonType>? hangingNode
) : ParseResult<LexonType>
{
  public readonly ASTNode<LexonType> astNode = astNode;
  public readonly int lexonsConsumed = lexonsConsumed;

  /// <summary>
  /// If the parser has not consumed all lexons track the last parse result scanned.
  /// </summary>
  public readonly ParseResult<LexonType>? hangingNode = hangingNode;
}

public class FailedParseResult<LexonType>(
  Lexon<LexonType>? offendingLexon,
  LexonType[] expectedLexons
) : ParseResult<LexonType>
{
  public readonly Lexon<LexonType>? offendingLexon = offendingLexon;
  public readonly LexonType[] expectedLexons = expectedLexons;

  public static FailedParseResult<LexonType> Aggregate(FailedParseResult<LexonType>[] failedResults)
  {
    Lexon<LexonType>? lastLexon = null;
    if (!failedResults.Any(x => x == null))
    {
      lastLexon = failedResults.Map(x => x.offendingLexon).MaxBy(x => x?.index);
    }
    var relevantResults = failedResults.Filter(x => x.offendingLexon == lastLexon);
    var expected = relevantResults.FlatMap(x => x.expectedLexons).Distinct().ToArray();
    return new FailedParseResult<LexonType>(lastLexon, expected);
  }

  public string ErrorMessage()
  {
    return $"Unexpected symbol {offendingLexon}, expected {string.Join(", ", expectedLexons)}";
  }
}
