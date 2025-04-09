using GenParse.Functional;

namespace GenParse.Parsing;

public class ProductionSet<LexonType>
{
  public string name;
  public readonly ProductionRule<LexonType>[] rules;

  public ProductionSet(string name, ProductionRule<LexonType>[] rules)
  {
    this.name = name;
    this.rules = rules;
  }

  public override string ToString()
  {
    return string.Join("\n", rules.Map(x => x.ToString()));
  }
}
