using System.Text.RegularExpressions;

namespace SharpParse.Parsing;

public class ProductionSymbol
{
  public readonly string name;
  public string NameWithMod => modifier == null ? name : $"{name}{modifier}";
  public readonly string lexonType;
  public readonly char? modifier;

  public bool isLexon => char.IsLower(name[0]);

  public ProductionSymbol(string name, string lexonType, char? modifier)
  {
    this.name = name;
    this.lexonType = lexonType;
    this.modifier = modifier;
  }

  public override string ToString()
  {
    return NameWithMod;
  }

  internal static ProductionSymbol Infer(string symbol)
  {
    if (string.IsNullOrEmpty(symbol))
    {
      throw new ArgumentException("Cannot infer a symbol from an empty string.");
    }
    char lastChar = symbol[symbol.Length - 1];
    char? modifierCharacter = null;
    if (Regex.IsMatch(lastChar.ToString(), GrammarLexonRules.modifierCharacterRegex))
    {
      modifierCharacter = lastChar;
      symbol = symbol.Substring(0, symbol.Length - 1);
    }
    var firstChar = symbol[0];
    return new ProductionSymbol(
      symbol,
      char.IsLower(firstChar) ? symbol : null!,
      modifierCharacter
    );
  }
}
