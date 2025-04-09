using GenParse.Functional;
using GenParse.Parsing;

public static class ASTNodeExtensions
{
  public static string SourceCode<LexonType>(this ASTNode<LexonType> astNode)
  {
    return string.Join("", astNode.lexons.Map(x => x.sourceCode));
  }
}
