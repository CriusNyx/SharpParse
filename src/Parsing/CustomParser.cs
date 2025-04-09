using SharpParse.Lexing;
using SharpParse.Parsing;

public interface CustomParser
{
  public string name { get; }
  public ParseResult? Parse(Parser parser, Lexon[] lexons, int index);
}
