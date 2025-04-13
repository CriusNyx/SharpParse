using System.Data;
using SharpParse.Functional;
using SharpParse.Lexing;

namespace SharpParse.Parsing;

public class Parser
{
  public readonly IReadOnlyDictionary<string, ProductionSet> productionSets =
    new Dictionary<string, ProductionSet>();
  public readonly IReadOnlyDictionary<string, CustomParser> customParsers =
    new Dictionary<string, CustomParser>();

  /// <summary>
  /// Used to generate unexpected symbols for rules.
  ///
  /// The head of a particular production rule is the set of symbols that are expected to start it.
  /// </summary>
  /// <typeparam name="string"></typeparam>
  /// <typeparam name="string[]"></typeparam>
  /// <returns></returns>
  private static Dictionary<string, string[]> headCache = new Dictionary<string, string[]>();

  public Parser(ProductionSet[] productionSets, CustomParser[] customParsers = null!)
  {
    customParsers = customParsers ?? [];

    this.productionSets = productionSets.ToDictionary(x => x.name);
    this.customParsers = customParsers.ToDictionary(x => x.name);
  }

  public ASTNode? Parse(string rootSymbol, Lexon[] lexons)
  {
    lexons = lexons.Filter(x => x.isSemantic);
    var result = TryParse(rootSymbol, lexons);
    if (result is SuccessParseResult succ)
    {
      return succ.astNode;
    }
    else if (result is FailedParseResult failed)
    {
      throw new ParseException(failed);
    }
    else
      throw new NotImplementedException();
  }

  public ParseResult TryParse(string rootSymbol, Lexon[] lexons)
  {
    var parseResult = _Parse(rootSymbol, lexons);

    if (parseResult is SuccessParseResult succ)
    {
      if (succ.lexonsConsumed != lexons.Length)
      {
        return succ.hangingNode!;
      }
      return succ;
    }
    return parseResult;
  }

  public override string ToString()
  {
    return string.Join("\n", productionSets.Values.Select(x => x.ToString()));
  }

  private ParseResult _Parse(string rootSymbol, Lexon[] lexons)
  {
    var productionSet = productionSets.Safe(rootSymbol);
    if (productionSet != null)
    {
      var result = ParseProductionSet(productionSet, lexons, 0);
      return result;
    }
    throw new NotImplementedException($"Could not find root symbol {rootSymbol}");
  }

  ParseResult ParseProductionSet(ProductionSet? productionSet, Lexon[] lexons, int index)
  {
    if (productionSet == null)
    {
      throw new Exception("WTF? I don't think this should be possible.");
    }
    List<ParseResult> results = new List<ParseResult>();
    foreach (var rule in productionSet.rules)
    {
      var result = ParseProductionRule(rule, lexons, index);
      if (result is SuccessParseResult)
      {
        return result;
      }
      results.Add(result);
    }
    return FailedParseResult.Aggregate(
      results.ToArray().Map(x => x as FailedParseResult).FilterDefined()
    );
  }

  ParseResult ParseProductionRule(ProductionRule productionRule, Lexon[] lexons, int index)
  {
    int offset = 0;
    List<ASTNode> nodes = new List<ASTNode>();
    ParseResult result = null!;
    foreach (var symbol in productionRule.symbols)
    {
      result = ParseProductionSymbol(symbol, lexons, index + offset);
      if (result is SuccessParseResult succ)
      {
        nodes.Add(succ.astNode);
        offset += succ.lexonsConsumed;
      }
      else
      {
        return result;
      }
    }

    ParseResult? hangingNode = null;
    if (result is SuccessParseResult succ2)
    {
      hangingNode = succ2.hangingNode!;
    }

    return new SuccessParseResult(
      new ASTNode(productionRule.name, productionRule, nodes.ToArray(), []),
      offset,
      hangingNode
    );
  }

  ParseResult ParseProductionSymbol(ProductionSymbol symbol, Lexon[] lexons, int index)
  {
    if (symbol.modifier != null)
    {
      switch (symbol.modifier)
      {
        case '*':
        {
          //result = symbol.ParseStar(context, lexons, index);
          return ParseStar(symbol, lexons, index);
        }
        case '?':
        {
          return ParseQuestion(symbol, lexons, index);
        }
        default:
          throw new NotImplementedException();
      }
    }
    else
    {
      return ParseSingle(symbol, lexons, index);
    }
  }

  SuccessParseResult ParseStar(ProductionSymbol symbol, Lexon[] lexons, int index)
  {
    int offset = 0;
    ParseResult? node;
    var output = new List<ASTNode>();
    do
    {
      node = ParseSingle(symbol, lexons, index + offset);
      if (node is SuccessParseResult succ)
      {
        offset += succ.lexonsConsumed;
        output.Add(succ.astNode);
      }
    } while (node is SuccessParseResult);
    return new SuccessParseResult(
      new ASTNode($"{symbol.name}{symbol.modifier}", null, output.ToArray(), []),
      offset,
      node
    );
  }

  SuccessParseResult ParseQuestion(ProductionSymbol symbol, Lexon[] lexons, int index)
  {
    var result = ParseSingle(symbol, lexons, index);
    var succ = result as SuccessParseResult;
    ASTNode[] children = [];
    if (succ != null)
    {
      children = [succ.astNode];
    }
    return new SuccessParseResult(
      new ASTNode(symbol.NameWithMod, null, children, []),
      succ?.lexonsConsumed ?? 0,
      succ == null ? result : null
    );
  }

  ParseResult ParseSingle(ProductionSymbol symbol, Lexon[] lexons, int index)
  {
    if (symbol.isLexon)
    {
      if (lexons.TryGet(index, out var lexon))
      {
        if (Equals(lexon.lexonType, symbol.lexonType))
        {
          return new SuccessParseResult(
            new ASTNode(symbol.name, null, [], lexons[index..(index + 1)]),
            1,
            null
          );
        }
        else
        {
          return new FailedParseResult(lexon, [symbol.lexonType]);
        }
      }
      return new FailedParseResult(null, [symbol.lexonType]);
    }
    else
    {
      if (customParsers.TryGetValue(symbol.name, out var customParser))
      {
        var result = customParser.Parse(this, lexons, index);
        if (result != null)
        {
          return result;
        }
      }
      var productionSet = productionSets.Safe(symbol.name);
      if (productionSet != null)
      {
        return ParseProductionSet(productionSet, lexons, index);
      }
      else
      {
        throw new Exception("Should this be possible?");
      }
    }
  }

  public string[] ComputeHead(string grammarElementName)
  {
    if (headCache.TryGetValue(grammarElementName, out var result))
    {
      return result;
    }
    var productionSet = productionSets.Safe(grammarElementName);

    string[] output;

    if (productionSet == null)
    {
      output = [];
    }
    else
    {
      output = ComputeHeadForSet(productionSet);
    }
    headCache[grammarElementName] = output;
    return output;
  }

  private string[] ComputeHeadForSet(ProductionSet productionSet)
  {
    return productionSet.rules.FlatMap(ComputeHeadForRule).Distinct().ToArray();
  }

  private string[] ComputeHeadForRule(ProductionRule productionRule)
  {
    if (productionRule.symbols.Length == 0)
    {
      return [];
    }
    List<string> output = new List<string>();
    for (int i = 0; i < productionRule.symbols.Length; i++)
    {
      var sym = productionRule.symbols[i];
      switch (sym.modifier)
      {
        case '*':
        case '?':
          output.AddRange(ComputeHeadForSymbol(sym));
          break;
        default:
          output.AddRange(ComputeHeadForSymbol(sym));
          return output.ToArray();
      }
    }
    return output.ToArray();
  }

  private string[] ComputeHeadForSymbol(ProductionSymbol productionSymbol)
  {
    if (productionSymbol.isLexon)
    {
      return [productionSymbol.lexonType];
    }
    else
    {
      return ComputeHead(productionSymbol.name);
    }
  }
}
