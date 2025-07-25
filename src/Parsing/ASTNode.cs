using SharpParse.Functional;
using SharpParse.Lexing;

namespace SharpParse.Parsing;

/// <summary>
/// An abstract syntax tree node.
/// </summary>
public class ASTNode
{
  /// <summary>
  /// The name of the production rule used to parse the node.
  /// </summary>
  public readonly string name;

  /// <summary>
  /// The production rule used to parse the node.
  /// </summary>
  public readonly ProductionRule? productionRule;

  /// <summary>
  /// The children of this node.
  /// </summary>
  public readonly ASTNode[] children;

  /// <summary>
  /// The set of lexons that comprise this node.
  /// </summary>
  public readonly Lexon[] lexons;

  public ASTNode(string name, ProductionRule? productionRule, ASTNode[] children, Lexon[] lexons)
  {
    this.name = name;
    this.productionRule = productionRule;
    this.children = children;
    this.lexons = lexons;
  }

  /// <summary>
  /// Print the program source code for this node.
  /// </summary>
  /// <returns></returns>
  public string PrintProgram()
  {
    var lines = PrintProgramPrivate();
    var leftLen = lines.Max((line) => line.treeString.Length);
    var padLen = leftLen + 3;
    return string.Join(
      "\n",
      lines.Map((line) => $"{line.treeString.PadRight(padLen)} {line.sourceString}")
    );
  }

  /// <summary>
  /// Internal method to print program.
  /// </summary>
  /// <param name="treeString"></param>
  /// <param name="indentLength"></param>
  /// <returns></returns>
  private (string treeString, string sourceString)[] PrintProgramPrivate(int indentLength = 0)
  {
    string indent = "";
    for (int i = 0; i < indentLength - 1; i++)
    {
      indent += " ";
    }
    if (indentLength > 0)
    {
      indent += " ";
    }
    return
    [
      (indent + name, LexerStatic.LexonsToSource(lexons, " ")),
      .. children.FlatMap((child) => child.PrintProgramPrivate(indentLength + 1)),
    ];
  }

  /// <summary>
  /// Return a representation of the AST node for the program.
  /// </summary>
  /// <returns></returns>
  public override string ToString()
  {
    return $"ASTNode {name}";
  }

  /// <summary>
  /// Compute the position of the node in the source code.
  /// </summary>
  /// <param name="start"></param>
  /// <param name="CalculatePosition("></param>
  /// <returns></returns>
  public (int start, int length) CalculatePosition()
  {
    if (lexons.Length > 0)
    {
      var start = lexons.First().index;
      var end = lexons.Last().end;
      return (start, end - start);
    }
    else if (children.Length > 0)
    {
      var start = children.First().CalculatePosition().start;
      var endPos = children.Map(x => x.CalculatePosition()).MaxBy(x => x.start + x.length);
      var end = endPos.start + endPos.length;
      return (start, end - start);
    }
    else
      return (0, 0);
  }
}
