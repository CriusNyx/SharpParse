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
}
