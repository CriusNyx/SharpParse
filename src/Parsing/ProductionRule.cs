using SharpParse.Functional;

namespace SharpParse.Parsing;

public class ProductionRule
{
  public readonly bool rootSymbol;
  public readonly string name;
  public readonly ProductionSymbol[] symbols;

  public ProductionRule(string name, ProductionSymbol[] symbols, bool rootSymbol = false)
  {
    this.name = name;
    this.symbols = symbols;
    this.rootSymbol = rootSymbol;
  }

  public ProductionRule(string name, params string[] symbols)
  {
    this.name = name;
    this.symbols = symbols.Map(ProductionSymbol.Infer);
  }

  public override string ToString()
  {
    return $"{name} = {string.Join(" ", symbols.Map(x => x.ToString()))}";
  }
}
