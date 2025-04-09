using System.Diagnostics.CodeAnalysis;

namespace GenParse.Lexing;

public class Lexon<LexonType>
{
  [NotNull]
  public readonly LexonType lexonType;
  public readonly string sourceCode;
  public readonly bool isSemantic;
  public readonly int index;
  public int length => sourceCode.Length;
  public int end => index + length;

  public Lexon(LexonType lexonType, string sourceCode, bool isSemantic, int index)
  {
    if (lexonType == null)
    {
      throw new ArgumentNullException(nameof(lexonType));
    }
    this.lexonType = lexonType;
    this.sourceCode = sourceCode;
    this.isSemantic = isSemantic;
    this.index = index;
  }

  public override string ToString()
  {
    return $"{lexonType} \"{sourceCode}\"";
  }

  public bool HasIndex(int index)
  {
    return index >= this.index && index <= this.index + length;
  }
}
