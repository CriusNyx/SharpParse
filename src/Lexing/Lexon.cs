using System.Diagnostics.CodeAnalysis;

namespace SharpParse.Lexing;

/// <summary>
/// A lexon.
/// </summary>
public class Lexon
{
  [NotNull]
  /// <summary>
  /// They type of this lexon.
  /// </summary>
  public readonly string lexonType;

  /// <summary>
  /// The source code for this lexon.
  /// </summary>
  public readonly string sourceCode;

  /// <summary>
  /// Indicates if this lexon is semantic
  /// </summary>
  public readonly bool isSemantic;

  /// <summary>
  /// The index of this lexon in the source code string.
  /// </summary>
  public readonly int index;

  /// <summary>
  /// The length of the lexon in the source code string.
  /// </summary>
  public int length => sourceCode.Length;

  /// <summary>
  /// The end of the lexon in the source code string.
  /// </summary>
  public int end => index + length;

  public Lexon(string lexonType, string sourceCode, bool isSemantic, int index)
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

  /// <summary>
  /// Indicates if the index is inside the lexon (inclusive).
  /// </summary>
  /// <param name="index"></param>
  /// <returns></returns>
  public bool HasIndex(int index)
  {
    return index >= this.index && index <= end;
  }
}
