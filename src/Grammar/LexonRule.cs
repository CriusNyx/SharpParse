using System.Text.RegularExpressions;

public class LexonRule
{
  public readonly string name;
  public readonly bool isSemantic;
  public readonly Regex regex;

  public LexonRule(string name, bool isSemantic, Regex regex)
  {
    this.name = name;
    this.isSemantic = isSemantic;
    this.regex = regex;
  }

  public override string ToString()
  {
    return $"{name} = /{regex}/";
  }
}
