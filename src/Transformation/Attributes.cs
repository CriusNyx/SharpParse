using SharpParse.Parsing;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ASTAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class)]
public class ASTClassAttribute(params string[] nodeName) : Attribute
{
  public readonly string[] nodeName = nodeName;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ASTFieldAttribute : Attribute
{
  public readonly string grammarElementName;

  public ASTFieldAttribute(string grammarElementName)
  {
    this.grammarElementName = grammarElementName;
  }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SourceAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class RangeAttribute : Attribute { }

public interface ASTTransformer
{
  object Transform();
}

public interface ASTSimplifier
{
  bool TrySimplify(out object result);
}

public static class ASTSimplifierExtensions
{
  public static T Simplify<T>(this ASTSimplifier source)
  {
    T output = (T)source;
    object temp;
    while (output is ASTSimplifier simplifier && simplifier.TrySimplify(out temp))
    {
      output = (T)temp;
    }
    return output;
  }
}
