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

  public static ProductionSet[] FromProductionRules(IEnumerable<ProductionRule> rules)
  {
    Dictionary<string, List<ProductionRule>> dict = new Dictionary<string, List<ProductionRule>>();

    foreach (var rule in rules)
    {
      dict.AddOrGet(rule.name, () => new List<ProductionRule>()).Add(rule);
    }

    return dict.Select((pair) => new ProductionSet(pair.Key, pair.Value.ToArray())).ToArray();
  }
}
