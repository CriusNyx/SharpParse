using SharpParse.Lexing;
using SharpParse.Parsing;

/// <summary>
/// Custom parser for elements that can't be trivially parsed using conventional parser.
/// </summary>
public interface CustomParser
{
  public string name { get; }
  public ParseResult? Parse(Parser parser, Lexon[] lexons, int index);
}
