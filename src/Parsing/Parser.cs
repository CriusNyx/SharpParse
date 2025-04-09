using System.Data;
using GenParse.Functional;
using GenParse.Lexing;

namespace GenParse.Parsing;

public class Parser<LexonType>
{
  public readonly IReadOnlyDictionary<string, ProductionSet<LexonType>> productionSets =
    new Dictionary<string, ProductionSet<LexonType>>();
  public readonly IReadOnlyDictionary<string, CustomParser<LexonType>> customParsers =
    new Dictionary<string, CustomParser<LexonType>>();

  private static Dictionary<string, LexonType[]> headCache = new Dictionary<string, LexonType[]>();

  public Parser(
    ProductionSet<LexonType>[] productionSets,
    CustomParser<LexonType>[] customParsers = null!
  )
  {
    customParsers = customParsers ?? [];

    this.productionSets = productionSets.ToDictionary(x => x.name);
    this.customParsers = customParsers.ToDictionary(x => x.name);
  }

  public ASTNode<LexonType>? Parse(string rootSymbol, Lexon<LexonType>[] lexons)
  {
    var result = TryParse(rootSymbol, lexons);
    if (result is SuccessParseResult<LexonType> succ)
    {
      return succ.astNode;
    }
    else if (result is FailedParseResult<LexonType> failed)
    {
      throw new ParseException<LexonType>(failed);
    }
    else
      throw new NotImplementedException();
  }

  public ParseResult<LexonType> TryParse(string rootSymbol, Lexon<LexonType>[] lexons)
  {
    var parseResult = _Parse(rootSymbol, lexons);

    if (parseResult is SuccessParseResult<LexonType> succ)
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

  private ParseResult<LexonType> _Parse(string rootSymbol, Lexon<LexonType>[] lexons)
  {
    var productionSet = productionSets.Safe(rootSymbol);
    if (productionSet != null)
    {
      var result = ParseProductionSet(productionSet, lexons, 0);
      return result;
    }
    throw new NotImplementedException($"Could not find root symbol {rootSymbol}");
  }

  ParseResult<LexonType> ParseProductionSet(
    ProductionSet<LexonType>? productionSet,
    Lexon<LexonType>[] lexons,
    int index
  )
  {
    if (productionSet == null)
    {
      throw new Exception("WTF? I don't think this should be possible.");
    }
    List<ParseResult<LexonType>> results = new List<ParseResult<LexonType>>();
    foreach (var rule in productionSet.rules)
    {
      var result = ParseProductionRule(rule, lexons, index);
      if (result is SuccessParseResult<LexonType>)
      {
        return result;
      }
      results.Add(result);
    }
    return FailedParseResult<LexonType>.Aggregate(
      results.ToArray().Map(x => x as FailedParseResult<LexonType>).FilterDefined()
    );
  }

  ParseResult<LexonType> ParseProductionRule(
    ProductionRule<LexonType> productionRule,
    Lexon<LexonType>[] lexons,
    int index
  )
  {
    int offset = 0;
    List<ASTNode<LexonType>> nodes = new List<ASTNode<LexonType>>();
    ParseResult<LexonType> result = null!;
    foreach (var symbol in productionRule.symbols)
    {
      result = ParseProductionSymbol(symbol, lexons, index + offset);
      if (result is SuccessParseResult<LexonType> succ)
      {
        nodes.Add(succ.astNode);
        offset += succ.lexonsConsumed;
      }
      else
      {
        return result;
      }
    }

    ParseResult<LexonType>? hangingNode = null;
    if (result is SuccessParseResult<LexonType> succ2)
    {
      hangingNode = succ2.hangingNode!;
    }

    return new SuccessParseResult<LexonType>(
      new ASTNode<LexonType>(productionRule.name, productionRule, nodes.ToArray(), []),
      offset,
      hangingNode
    );
  }

  ParseResult<LexonType> ParseProductionSymbol(
    ProductionSymbol<LexonType> symbol,
    Lexon<LexonType>[] lexons,
    int index
  )
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

  SuccessParseResult<LexonType> ParseStar(
    ProductionSymbol<LexonType> symbol,
    Lexon<LexonType>[] lexons,
    int index
  )
  {
    int offset = 0;
    ParseResult<LexonType>? node;
    var output = new List<ASTNode<LexonType>>();
    do
    {
      node = ParseSingle(symbol, lexons, index + offset);
      if (node is SuccessParseResult<LexonType> succ)
      {
        offset += succ.lexonsConsumed;
        output.Add(succ.astNode);
      }
    } while (node is SuccessParseResult<LexonType>);
    return new SuccessParseResult<LexonType>(
      new ASTNode<LexonType>($"{symbol.name}{symbol.modifier}", null, output.ToArray(), []),
      offset,
      node
    );
  }

  SuccessParseResult<LexonType> ParseQuestion(
    ProductionSymbol<LexonType> symbol,
    Lexon<LexonType>[] lexons,
    int index
  )
  {
    var result = ParseSingle(symbol, lexons, index);
    var succ = result as SuccessParseResult<LexonType>;
    ASTNode<LexonType>[] children = [];
    if (succ != null)
    {
      children = [succ.astNode];
    }
    return new SuccessParseResult<LexonType>(
      new ASTNode<LexonType>(symbol.NameWithMod, null, children, []),
      succ?.lexonsConsumed ?? 0,
      succ == null ? result : null
    );
  }

  ParseResult<LexonType> ParseSingle(
    ProductionSymbol<LexonType> symbol,
    Lexon<LexonType>[] lexons,
    int index
  )
  {
    if (symbol.isLexon)
    {
      if (lexons.TryGet(index, out var lexon))
      {
        if (Equals(lexon.lexonType, symbol.lexonType))
        {
          return new SuccessParseResult<LexonType>(
            new ASTNode<LexonType>(symbol.name, null, [], lexons[index..(index + 1)]),
            1,
            null
          );
        }
        else
        {
          return new FailedParseResult<LexonType>(lexon, [symbol.lexonType]);
        }
      }
      return new FailedParseResult<LexonType>(null, [symbol.lexonType]);
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

  public LexonType[] ComputeHead(string grammarElementName)
  {
    if (headCache.TryGetValue(grammarElementName, out var result))
    {
      return result;
    }
    var productionSet = productionSets.Safe(grammarElementName);

    LexonType[] output;

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

  private LexonType[] ComputeHeadForSet(ProductionSet<LexonType> productionSet)
  {
    return productionSet.rules.FlatMap(ComputeHeadForRule).Distinct().ToArray();
  }

  private LexonType[] ComputeHeadForRule(ProductionRule<LexonType> productionRule)
  {
    if (productionRule.symbols.Length == 0)
    {
      return [];
    }
    List<LexonType> output = new List<LexonType>();
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

  private LexonType[] ComputeHeadForSymbol(ProductionSymbol<LexonType> productionSymbol)
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
