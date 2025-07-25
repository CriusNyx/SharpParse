using SharpParse.Functional;
using SharpParse.Parsing;

public static class ASTNodeExtensions
{
  /// <summary>
  /// Construct the source code from an AST node.
  /// </summary>
  /// <param name="astNode"></param>
  /// <returns></returns>
  public static string SourceCode(this ASTNode astNode)
  {
    return string.Join("", astNode.lexons.Map(x => x.sourceCode));
  }
}
