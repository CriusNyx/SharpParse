namespace SharpParse.Parsing;

/// <summary>
/// The context for a current parsing job.
/// </summary>
public class ParseContext
{
  /// <summary>
  /// A list of production sets for the language.
  /// </summary>
  public readonly IReadOnlyDictionary<string, ProductionSet> productionSets;

  /// <summary>
  /// /// A list of custom parsers for the language.
  /// </summary>
  /// <typeparam name="string"></typeparam>
  /// <typeparam name="CustomParser"></typeparam>
  /// <returns></returns>
  public readonly IReadOnlyDictionary<string, CustomParser> customParsers =
    new Dictionary<string, CustomParser>();

  /// <summary>
  ///
  /// </summary>
  /// <param name="productionSets"></param>
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
