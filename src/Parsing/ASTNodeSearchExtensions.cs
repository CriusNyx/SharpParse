using SharpParse.Functional;

namespace SharpParse.Parsing;

public static class ASTNodeSearchExtensions
{
  public static ASTNode? MatchPath(this ASTNode? node, string search)
  {
    return MatchPath(node, search.Split('.'));
  }

  public static ASTNode? MatchPath(this ASTNode? node, string[] search)
  {
    return search.Reduce(node, (search, node) => node.Match(search));
  }

  public static ASTNode? Match(this ASTNode? node, string search)
  {
    if (TryMatch(node, search, out var output))
    {
      return output;
    }
    return null;
  }

  public static (ASTNode?, ASTNode?) Match(this ASTNode? node, (string, string) search)
  {
    if (TryMatch(node, search, out var output))
    {
      return output;
    }
    return (null, null);
  }

  public static (ASTNode?, ASTNode?, ASTNode?) Match(
    this ASTNode? node,
    (string, string, string) search
  )
  {
    if (TryMatch(node, search, out var output))
    {
      return output;
    }
    return (null, null, null);
  }

  public static bool TryMatch(this ASTNode? node, string search, out ASTNode result)
  {
    var output = TryMatch(node, [search], out var arr);
    result = arr[0];
    return output;
  }

  public static bool TryMatch(
    this ASTNode? node,
    (string, string) search,
    out (ASTNode, ASTNode) nodes
  )
  {
    var output = TryMatch(node, [search.Item1, search.Item2], out var result);
    nodes = (result[0], result[1]);
    return output;
  }

  public static bool TryMatch(
    this ASTNode? node,
    (string, string, string) search,
    out (ASTNode, ASTNode, ASTNode) nodes
  )
  {
    var output = TryMatch(node, [search.Item1, search.Item2, search.Item3], out var result);
    nodes = (result[0], result[1], result[2]);
    return output;
  }

  public static bool TryMatch(
    this ASTNode? node,
    (string, string, string, string) search,
    out (ASTNode, ASTNode, ASTNode, ASTNode) nodes
  )
  {
    var output = TryMatch(node, [search.Item1, search.Item2, search.Item3], out var result);
    nodes = (result[0], result[1], result[2], result[3]);
    return output;
  }

  public static bool TryMatch(this ASTNode? node, string[] search, out ASTNode[] results)
  {
    results = new ASTNode[search.Length];
    if (node == null)
    {
      return false;
    }
    var children = node.children;
    int i = 0;
    for (int j = 0; i < search.Length && j < children.Length; j++)
    {
      var searchSymbol = search[i];
      var child = children[j];
      if (searchSymbol == child.name)
      {
        results[i] = child;
        i++;
      }
    }
    return i == search.Length;
  }

  public static ASTNode[] MatchAll<LexonType>(this ASTNode node, string search)
  {
    return node.MatchAll([search]);
  }

  public static ASTNode[] MatchAll(this ASTNode node, string[] search)
  {
    if (search.Contains(node.name))
    {
      return [node, .. node.children.FlatMap((child) => child.MatchAll(search))];
    }
    return node.children.FlatMap((child) => child.MatchAll(search));
  }
}
