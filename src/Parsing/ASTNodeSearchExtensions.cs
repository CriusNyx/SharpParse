using GenParse.Functional;

namespace GenParse.Parsing;

public static class ASTNodeSearchExtensions
{
  public static ASTNode<LexonType>? MatchPath<LexonType>(
    this ASTNode<LexonType>? node,
    string search
  )
  {
    return MatchPath(node, search.Split('.'));
  }

  public static ASTNode<LexonType>? MatchPath<LexonType>(
    this ASTNode<LexonType>? node,
    string[] search
  )
  {
    return search.Reduce(node, (search, node) => node.Match(search));
  }

  public static ASTNode<LexonType>? Match<LexonType>(this ASTNode<LexonType>? node, string search)
  {
    if (TryMatch(node, search, out var output))
    {
      return output;
    }
    return null;
  }

  public static (ASTNode<LexonType>?, ASTNode<LexonType>?) Match<LexonType>(
    this ASTNode<LexonType>? node,
    (string, string) search
  )
  {
    if (TryMatch(node, search, out var output))
    {
      return output;
    }
    return (null, null);
  }

  public static (ASTNode<LexonType>?, ASTNode<LexonType>?, ASTNode<LexonType>?) Match<LexonType>(
    this ASTNode<LexonType>? node,
    (string, string, string) search
  )
  {
    if (TryMatch(node, search, out var output))
    {
      return output;
    }
    return (null, null, null);
  }

  public static bool TryMatch<LexonType>(
    this ASTNode<LexonType>? node,
    string search,
    out ASTNode<LexonType> result
  )
  {
    var output = TryMatch(node, [search], out var arr);
    result = arr[0];
    return output;
  }

  public static bool TryMatch<LexonType>(
    this ASTNode<LexonType>? node,
    (string, string) search,
    out (ASTNode<LexonType>, ASTNode<LexonType>) nodes
  )
  {
    var output = TryMatch(node, [search.Item1, search.Item2], out var result);
    nodes = (result[0], result[1]);
    return output;
  }

  public static bool TryMatch<LexonType>(
    this ASTNode<LexonType>? node,
    (string, string, string) search,
    out (ASTNode<LexonType>, ASTNode<LexonType>, ASTNode<LexonType>) nodes
  )
  {
    var output = TryMatch(node, [search.Item1, search.Item2, search.Item3], out var result);
    nodes = (result[0], result[1], result[2]);
    return output;
  }

  public static bool TryMatch<LexonType>(
    this ASTNode<LexonType>? node,
    (string, string, string, string) search,
    out (ASTNode<LexonType>, ASTNode<LexonType>, ASTNode<LexonType>, ASTNode<LexonType>) nodes
  )
  {
    var output = TryMatch(node, [search.Item1, search.Item2, search.Item3], out var result);
    nodes = (result[0], result[1], result[2], result[3]);
    return output;
  }

  public static bool TryMatch<LexonType>(
    this ASTNode<LexonType>? node,
    string[] search,
    out ASTNode<LexonType>[] results
  )
  {
    results = new ASTNode<LexonType>[search.Length];
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

  public static ASTNode<LexonType>[] MatchAll<LexonType>(
    this ASTNode<LexonType> node,
    string search
  )
  {
    return node.MatchAll([search]);
  }

  public static ASTNode<LexonType>[] MatchAll<LexonType>(
    this ASTNode<LexonType> node,
    string[] search
  )
  {
    if (search.Contains(node.name))
    {
      return [node, .. node.children.FlatMap((child) => child.MatchAll(search))];
    }
    return node.children.FlatMap((child) => child.MatchAll(search));
  }
}
