using SharpParse.Functional;
using SharpParse.Parsing;

public static class ASTNodeExtensions
{
  public static string SourceCode(this ASTNode astNode)
  {
    return string.Join("", astNode.lexons.Map(x => x.sourceCode));
  }
}
