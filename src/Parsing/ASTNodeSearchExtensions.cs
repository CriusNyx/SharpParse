using SharpParse.Functional;

namespace SharpParse.Parsing;

public static class ASTNodeSearchExtensions
{
  /// <summary>
  /// Attempt to traverse the AST nodes children according to the search string. Search strings are separated by dots.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="search">Used to traverse the node. Ex "Parameter.Type" will find the first child of type Parameter, then a child of Parameter of type Type</param>
  /// <returns></returns>
  public static ASTNode? MatchPath(this ASTNode? node, string search)
  {
    return MatchPath(node, search.Split('.'));
  }

  /// <summary>
  /// Will search the node recursively for nodes of the same type as the search path.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="search"></param>
  /// <returns></returns>
  public static ASTNode? MatchPath(this ASTNode? node, string[] search)
  {
    return search.Reduce(node, (search, node) => node.Match(search));
  }

  /// <summary>
  /// Find the first child of type "search" and return it.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="search"></param>
  /// <returns></returns>
  public static ASTNode? Match(this ASTNode? node, string search)
  {
    if (TryMatch(node, search, out var output))
    {
      return output;
    }
    return null;
  }

  /// <summary>
  /// Find two elements matching the search and return them.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="search"></param>
  /// <returns></returns>
  public static (ASTNode?, ASTNode?) Match(this ASTNode? node, (string, string) search)
  {
    if (TryMatch(node, search, out var output))
    {
      return output;
    }
    return (null, null);
  }

  /// <summary>
  /// Find three elements matching the search and return them.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="(string"></param>
  /// <param name="search"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Attempt to match the search string and return true if found.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="search"></param>
  /// <param name="result"></param>
  /// <returns></returns>
  public static bool TryMatch(this ASTNode? node, string search, out ASTNode result)
  {
    var output = TryMatch(node, [search], out var arr);
    result = arr[0];
    return output;
  }

  /// <summary>
  /// Try to find two matching elements and return true if you find them.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="(string"></param>
  /// <param name="search"></param>
  /// <param name="(ASTNode"></param>
  /// <param name="nodes"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Try matching 3 children and return true if they're found.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="(string"></param>
  /// <param name="search"></param>
  /// <param name="(ASTNode"></param>
  /// <param name="nodes"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Try matching 4 children and return true if they're found.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="(string"></param>
  /// <param name="search"></param>
  /// <param name="(ASTNode"></param>
  /// <param name="nodes"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Try to match some number of elements and return true if they're found.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="search"></param>
  /// <param name="results"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Match all elements of the same type as search.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="search"></param>
  /// <typeparam name="LexonType"></typeparam>
  /// <returns></returns>
  public static ASTNode[] MatchAll<LexonType>(this ASTNode node, string search)
  {
    return node.MatchAll([search]);
  }

  /// <summary>
  /// Match all elements that match any of the search types.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="search"></param>
  /// <returns></returns>
  public static ASTNode[] MatchAll(this ASTNode node, string[] search)
  {
    if (search.Contains(node.name))
    {
      return [node, .. node.children.FlatMap((child) => child.MatchAll(search))];
    }
    return node.children.FlatMap((child) => child.MatchAll(search));
  }
}
