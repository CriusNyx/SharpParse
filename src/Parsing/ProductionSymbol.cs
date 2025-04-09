namespace GenParse.Parsing;

public class ProductionSymbol<LexonType>
{
  public readonly string name;
  public string NameWithMod => modifier == null ? name : $"{name}{modifier}";
  public readonly LexonType lexonType;
  public readonly char? modifier;

  public bool isLexon => char.IsLower(name[0]);

  public ProductionSymbol(string name, LexonType lexonType, char? modifier)
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
