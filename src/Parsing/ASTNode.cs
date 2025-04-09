using GenParse.Functional;
using GenParse.Lexing;

namespace GenParse.Parsing;

public class ASTNode<LexonType>
{
  public readonly string name;
  public readonly ProductionRule<LexonType>? productionRule;
  public readonly ASTNode<LexonType>[] children;
  public readonly Lexon<LexonType>[] lexons;

  public ASTNode(
    string name,
    ProductionRule<LexonType>? productionRule,
    ASTNode<LexonType>[] children,
    Lexon<LexonType>[] lexons
  )
  {
    this.name = name;
    this.productionRule = productionRule;
    this.children = children;
    this.lexons = lexons;
  }

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

  public override string ToString()
  {
    return $"ASTNode {name}";
  }

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
