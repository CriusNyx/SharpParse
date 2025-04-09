using SharpParse.Functional;

namespace SharpParse.Parsing;

public class ProductionRule
{
  public readonly string name;
  public readonly ProductionSymbol[] symbols;

  public ProductionRule(string name, ProductionSymbol[] symbols)
  {
    this.name = name;
    this.symbols = symbols;
  }

  public override string ToString()
  {
    return $"{name} = {string.Join(" ", symbols.Map(x => x.ToString()))}";
  }
}
