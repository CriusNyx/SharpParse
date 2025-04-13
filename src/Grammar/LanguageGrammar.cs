using SharpParse.Functional;
using SharpParse.Parsing;

public class LanguageGrammar
{
  public readonly string rootSymbol;
  public readonly LexonRule[] lexonRules;
  public readonly ProductionSet[] productionSets;

  public LanguageGrammar(string rootSymbol, LexonRule[] lexonRules, ProductionSet[] productionSets)
  {
    this.rootSymbol = rootSymbol;
    this.lexonRules = lexonRules;
    this.productionSets = productionSets;
  }

  public override string ToString()
  {
    return $"Root Symbol: {rootSymbol}\n\n{string.Join("\n", lexonRules.Map(x => x.ToString()))}\n\n{string.Join("\n", productionSets.Map(x => x.ToString()))}";
  }
}
