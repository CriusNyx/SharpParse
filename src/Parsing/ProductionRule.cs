using GenParse.Functional;

namespace GenParse.Parsing;

public class ProductionRule<LexonType>
{
  public readonly string name;
  public readonly ProductionSymbol<LexonType>[] symbols;

  public ProductionRule(string name, ProductionSymbol<LexonType>[] symbols)
  {
    this.name = name;
    this.symbols = symbols;
  }

  public override string ToString()
  {
    return $"{name} = {string.Join(" ", symbols.Map(x => x.ToString()))}";
  }
}
