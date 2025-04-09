namespace SharpParse.Parsing;

public class ParseContext
{
  public readonly IReadOnlyDictionary<string, ProductionSet> productionSets;
  public readonly IReadOnlyDictionary<string, CustomParser> customParsers =
    new Dictionary<string, CustomParser>();

  public ParseContext(IReadOnlyDictionary<string, ProductionSet> productionSets)
  {
    this.productionSets = productionSets;
  }

  public override string ToString()
  {
    return "Grammar:\n\n"
      + string.Join("\n\n", productionSets.Select((pair) => $"{pair.Key}:\n{pair.Value}"));
  }
}
