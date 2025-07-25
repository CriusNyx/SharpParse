using System.Reflection;
using SharpParse.Functional;
using SharpParse.Parsing;

public static class Transformer
{
  private static Dictionary<string, Type> transformCache = new Dictionary<string, Type>();

  static Transformer()
  {
    foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
    {
      if (type.GetCustomAttribute<ASTClassAttribute>() is ASTClassAttribute astClass)
      {
        foreach (var className in astClass.nodeName)
        {
          transformCache.Add(className, type);
        }
      }
    }
  }

  public static object Transform(ASTNode root)
  {
    Dictionary<ASTNode, object> transformationMap = new Dictionary<ASTNode, object>();

    // Transform each AST node into it's corresponding language node.
    root.Crawl(
      x => x.children,
      x =>
      {
        var value = TransformNodeIntoBaseObject(x);
        if (value != null)
        {
          transformationMap.Add(x, value);
        }
      }
    );

    return ResolveNodeToFinalForm(root, transformationMap)!;
  }

  private static void AssignTransformedMembers(object target, ASTNode node)
  {
    foreach (var member in target.GetType().GetMembers())
    {
      if (member.GetCustomAttribute<ASTAttribute>() != null)
      {
        AssignMember(target, member, node);
      }

      if (member.GetCustomAttribute<SourceAttribute>() != null)
      {
        AssignMember(target, member, node.SourceCode());
      }
      if (member.GetCustomAttribute<RangeAttribute>() != null)
      {
        AssignMember(target, member, node.CalculatePosition());
      }
    }
  }

  private static void AssignMember(object target, MemberInfo member, object value)
  {
    if (member is FieldInfo field)
    {
      field.SetValue(target, value);
    }
    if (member is PropertyInfo property)
    {
      property.SetValue(target, value);
    }
  }

  private static void AssignField(
    Dictionary<ASTNode, object> transformationMap,
    object value,
    FieldInfo field,
    ASTNode result
  )
  {
    if (transformationMap.TryGetValue(result, out var transformedValue))
    {
      if (transformedValue is object[] arr)
      {
        var arrType = field.FieldType;
        var arrElementType = arrType.GetElementType();
        var typedArray = arr.ToTypedArray(arrElementType!);
        field.SetValue(value, typedArray);
      }
      else if (field.FieldType == typeof(bool))
      {
        var valueObj = value;
        field.SetValueDirect(__makeref(valueObj), result.children.Count() > 0);
      }
      else
      {
        field.SetValue(value, transformedValue);
      }
    }
    else if (field.FieldType == typeof(bool))
    {
      var valueObj = value;
      field.SetValueDirect(__makeref(valueObj), result.children.Count() > 0);
    }
  }

  private static object? TransformNodeIntoBaseObject(ASTNode node)
  {
    if (node.name.EndsWith("*"))
    {
      return new object[] { };
    }
    var transformType = transformCache.Safe(node.name);
    if (transformType != null)
    {
      var value = (
        transformType.GetConstructor([typeof(ASTNode)])?.Invoke([node])
        ?? transformType.GetConstructor([])?.Invoke([])
      ).NotNull();

      AssignTransformedMembers(value, node);

      return value!;
    }
    return null;
  }

  private static object? ResolveNodeToFinalForm(
    ASTNode node,
    Dictionary<ASTNode, object> transformationMap
  )
  {
    foreach (var child in node.children)
    {
      var childNodeResolution = ResolveNodeToFinalForm(child, transformationMap);
      if (childNodeResolution != null)
      {
        transformationMap[child] = childNodeResolution;
      }
    }

    var nodeValue = transformationMap.Safe(node);

    // If this element has no node return the node belonging to the first child that is defined.
    if (nodeValue == null)
    {
      return node.children.Map(x => transformationMap.Safe(x)).FirstOrDefault(x => x != null);
    }
    // If this node is an array then it must be a star node. Therefore it should be transformed into an array of it's defined children.
    else if (nodeValue is object[])
    {
      return node
        .children.Map(x => transformationMap.Safe(x))
        .FilterDefined()
        .ToTypedArray<object>();
    }
    // Otherwise this node is an object and it's fields must be scanned and assigned.
    else
    {
      FieldInfo[] fieldsWithAttributes = nodeValue
        .GetType()
        .GetFields()
        .Filter(x => x.GetCustomAttribute<ASTFieldAttribute>() != null);

      var grammarElementNames = fieldsWithAttributes.Map(x =>
        x.GetCustomAttribute<ASTFieldAttribute>()!.grammarElementName
      );

      // Resolve all this nodes fields by matching them to child nodes.
      if (node.TryMatch(grammarElementNames, out var results))
      {
        foreach (
          var (field, result, grammarElementName) in fieldsWithAttributes.Zip(
            results,
            grammarElementNames
          )
        )
        {
          AssignField(transformationMap, nodeValue, field, result);
        }
      }

      // If this node is an ASTTransformationNode return it's transformation instead.
      if (nodeValue is ASTTransformer transformer)
      {
        nodeValue = transformer.Transform();
        AssignTransformedMembers(nodeValue.GetType().GetMembers(), node);
      }
      while (nodeValue is ASTSimplifier simplifier && simplifier.TrySimplify(out var simplified))
      {
        nodeValue = simplified;
      }
      return nodeValue;
    }
  }
}
