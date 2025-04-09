using GenParse.Functional;
using GenParse.Parsing;

public class ParseException<LexonType>(FailedParseResult<LexonType> failedParseResult)
  : Exception(GenerateMessage(failedParseResult))
{
  public readonly FailedParseResult<LexonType> failedParseResult = failedParseResult;

  private static string GenerateMessage(FailedParseResult<LexonType> failedParseResult)
  {
    return $"Unexpected symbol {failedParseResult.offendingLexon?.sourceCode ?? "eof"}. Expected {string.Join(", ", failedParseResult.expectedLexons.Map(x => x!.ToString()))}";
  }
}
