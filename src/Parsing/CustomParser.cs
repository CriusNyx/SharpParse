using GenParse.Lexing;
using GenParse.Parsing;

public interface CustomParser<LexonType>
{
  public string name { get; }
  public ParseResult<LexonType>? Parse(
    Parser<LexonType> parser,
    Lexon<LexonType>[] lexons,
    int index
  );
}
