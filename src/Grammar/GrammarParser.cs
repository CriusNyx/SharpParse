using System.Text.RegularExpressions;
using SharpParse.Functional;
using SharpParse.Lexing;
using SharpParse.Parsing;
using RuleNames = SharpParse.GrammarFileGrammar.RuleNames;

namespace SharpParse.Grammar;

/// <summary>
/// The types that are valid for grammar nodes.
/// </summary>
public static class GrammarLexonType
{
  public const string comment = "comment";
  public const string whitespace = "whitespace";
  public const string name = "name";
  public const string regex = "regex";
  public const string equalSign = "equalSign";
  public const string semicolon = "semicolon";
  public const string modifierCharacter = "modifierCharacter";
}

/// <summary>
/// Custom parser for parsing grammar files.
/// </summary>
public class GrammarParser
{
  /// <summary>
  /// Parse the specified grammar file and return a set of production rules for the grammar.
  /// </summary>
  /// <param name="sourceFiles"></param>
  /// <param name="stringToLexon"></param>
  /// <returns></returns>
  [Obsolete("Use ParseGrammar(string)")]
  public static ProductionSet[] ParseGrammar(
    string[] sourceFiles,
    Func<string, string> stringToLexon
  )
  {
    var rules = sourceFiles.FlatMap((x) => Parse(x, stringToLexon));
    return ProductionSet.FromProductionRules(rules);
  }

  /// <summary>
  /// Parse a grammar and return a set of production rules.
  /// </summary>
  /// <param name="code"></param>
  /// <param name="stringToLexon"></param>
  /// <returns></returns>
  [Obsolete("Use ParseGrammar(string)")]
  internal static ProductionRule[] Parse(string code, Func<string, string> stringToLexon)
  {
    var lexons = LexerStatic
      .Lex(code, GrammarLexonRules.LexonRules)
      .Filter(lexon => lexon.isSemantic);
    var queue = new Queue<Lexon>(lexons);

    return GenerateRuleParser(queue).UntilNull().ToArray();
  }

  /// <summary>
  /// Parse a grammar specification and return a LanguageGrammar for the specification.
  /// </summary>
  /// <param name="code"></param>
  /// <returns></returns>
  public static LanguageGrammar ParseGrammar(string code)
  {
    // Lex source code
    var lexons = LexerStatic.Lex(code, GrammarLexonRules.LexonRules);

    // Generate a parser for source code
    var parser = GenerateGrammarParser();

    // Parse the grammar specifiction and get the root node.
    var grammarNode = parser.Parse("Grammar", lexons.Filter(x => x.isSemantic))!;

    return TransformASTToGrammar(grammarNode);
  }

  /// <summary>
  /// Transform an AST for a grammar specification to a language grammar
  /// </summary>
  /// <param name="grammarNode"></param>
  /// <returns></returns>
  private static LanguageGrammar TransformASTToGrammar(ASTNode grammarNode)
  {
    // Get the set of grammar statements
    var grammarStarNode = grammarNode.Match("GrammarStatement*");

    // Construct lists for outputs
    List<ProductionRule> productionRules = new List<ProductionRule>();
    List<LexonRule> lexonRules = new List<LexonRule>();

    // Iterate through each statement and extract the relevant specifications.
    foreach (var child in grammarStarNode!.children)
    {
      if (TryGetProductionRule(child, out var productionRule))
      {
        productionRules.Add(productionRule);
      }
      if (TryGetLexonRule(child, out var lexonRule))
      {
        lexonRules.Add(lexonRule);
      }
    }

    // Find the root symbol for the specified grammar.
    var rootSymbol = productionRules
      .FirstOrDefault(x => x.rootSymbol)
      .NotNull("Language must have a root symbol defined.")
      .name;

    /// <summary>
    /// Construct output and return it.
    /// </summary>
    /// <param name="rootSymbol"></param>
    /// <param name="lexonRules.ToArray()"></param>
    /// <param name="ProductionSet.FromProductionRules(productionRules"></param>
    /// <returns></returns>
    return new LanguageGrammar(
      rootSymbol,
      lexonRules.ToArray(),
      ProductionSet.FromProductionRules(productionRules)
    );
  }

  private static Parser GenerateGrammarParser()
  {
    var grammarProductionSets = ProductionSet.FromProductionRules(
      GrammarFileGrammar.ProductionRules
    );

    var parser = new Parser(grammarProductionSets);
    return parser;
  }

  /// <summary>
  /// Try to get a production rule for a specific AST node.
  /// </summary>
  /// <param name="astNode"></param>
  /// <param name="productionRule"></param>
  /// <returns></returns>
  private static bool TryGetProductionRule(ASTNode astNode, out ProductionRule productionRule)
  {
    if (astNode.TryMatch(RuleNames.ProductionRule, out var productionRuleNode))
    {
      List<ProductionSymbol> productionSymbols = new List<ProductionSymbol>();
      var (rootMarker, nameNode, symbolsNode) = productionRuleNode.Match(
        ("exclamation?", "name", "ProductionSymbol*")
      );
      bool isRoot = rootMarker.NotNull().children.Length != 0;

      foreach (var symbol in symbolsNode?.children!)
      {
        var (symbolNameNode, modifierCharacterNode) = symbol.Match(("name", "modifierCharacter?"));
        var symName = symbolNameNode?.SourceCode();
        var modSource = modifierCharacterNode.NotNull().SourceCode();
        char? modChar = null;
        if (modSource.Length != 0)
        {
          modChar = modSource[0];
        }

        productionSymbols.Add(new ProductionSymbol(symName.NotNull(), null!, modChar));
      }

      productionRule = new ProductionRule(
        nameNode.NotNull().SourceCode(),
        productionSymbols.ToArray(),
        isRoot
      );

      return true;
    }

    productionRule = null!;
    return false;
  }

  /// <summary>
  /// Try to get a lexon rule for a particular ASTNode.
  /// </summary>
  /// <param name="astNode"></param>
  /// <param name="lexonRule"></param>
  /// <returns></returns>
  private static bool TryGetLexonRule(ASTNode astNode, out LexonRule lexonRule)
  {
    if (astNode.TryMatch(RuleNames.LexonRule, out var lexonNode))
    {
      var (nameNode, modCharNode, regexNode) = lexonNode.Match(
        ("name", "modifierCharacter?", "regex")
      );

      var name = nameNode?.SourceCode() ?? "";
      var regexSource = regexNode?.SourceCode();
      // Remove the leading and trailing slash on the regex.
      var regex = regexSource?.Substring(1, regexSource.Length - 2) ?? "";

      bool isSemantic = !(
        modCharNode.NotNull().TryMatch("modifierCharacter", out var modCharChildNode)
        && modCharChildNode.SourceCode() == "?"
      );

      lexonRule = new LexonRule(name, isSemantic, new Regex($"^{regex}"));
      return true;
    }
    else
    {
      lexonRule = null!;
      return false;
    }
  }

  /// <summary>
  /// Generate the parser for parsing rules.
  /// </summary>
  /// <param name="queue"></param>
  /// <param name="stringToLexon"></param>
  /// <returns></returns>
  static Func<ProductionRule?> GenerateRuleParser(Queue<Lexon> queue)
  {
    return () => ParseRule(ref queue);
  }

  /// <summary>
  /// Attempt to parse and return a production rule.
  /// </summary>
  /// <param name="queue"></param>
  /// <returns></returns>
  static ProductionRule? ParseRule(ref Queue<Lexon> queue)
  {
    string? name = null;
    bool rulesSection = false;
    string? ruleSymbol = null;
    char? modifier = null;

    List<ProductionSymbol> symbols = new List<ProductionSymbol>();

    while (queue.TryDequeue(out var value))
    {
      switch (value.lexonType)
      {
        case GrammarLexonType.name:
          if (name != null && !rulesSection)
          {
            throw new Exception($"Already parsed a name for this rule {name} {value.sourceCode}");
          }
          else if (name != null)
          {
            if (ruleSymbol != null)
            {
              symbols.Add(new ProductionSymbol(ruleSymbol, ruleSymbol, modifier));
              modifier = null;
            }
            ruleSymbol = value.sourceCode;
          }
          else
          {
            name = value.sourceCode;
          }
          break;
        case GrammarLexonType.equalSign:
          if (rulesSection)
          {
            throw new Exception($"More then one definition for rule {name}");
          }
          rulesSection = true;
          break;
        case GrammarLexonType.modifierCharacter:
          if (modifier != null)
          {
            throw new Exception($"Multiple modifier characters for symbol {ruleSymbol}");
          }
          modifier = value.sourceCode[0];
          break;
        case GrammarLexonType.semicolon:
          if (name == null)
          {
            throw new Exception($"Rule with no name");
          }
          if (ruleSymbol != null)
          {
            symbols.Add(new ProductionSymbol(ruleSymbol, ruleSymbol, modifier));
          }
          return new ProductionRule(name!, symbols.ToArray());
      }
    }
    return null!;
  }
}
