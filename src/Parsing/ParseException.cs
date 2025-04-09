using SharpParse.Functional;
using SharpParse.Parsing;

public class ParseException(FailedParseResult failedParseResult)
  : Exception(GenerateMessage(failedParseResult))
{
  public readonly FailedParseResult failedParseResult = failedParseResult;

  private static string GenerateMessage(FailedParseResult failedParseResult)
  {
    return $"Unexpected symbol {failedParseResult.offendingLexon?.sourceCode ?? "eof"}. Expected {string.Join(", ", failedParseResult.expectedLexons.Map(x => x!.ToString()))}";
  }
}
