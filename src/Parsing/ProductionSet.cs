using SharpParse.Functional;

namespace SharpParse.Parsing;

public class ProductionSet
{
  public string name;
  public readonly ProductionRule[] rules;

  public ProductionSet(string name, ProductionRule[] rules)
  {
    this.name = name;
    this.rules = rules;
  }

  public override string ToString()
  {
    return string.Join("\n", rules.Map(x => x.ToString()));
  }
}
