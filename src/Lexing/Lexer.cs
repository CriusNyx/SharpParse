// using System.Text.RegularExpressions;
// using SharpParse.Lexing;

// public class Lexer
// {
//   (string ruleType, Regex regex)[] lexonRules;
//   Func<string, string, int, Lexon> lexonConstructor;

//   public Lexer(
//     (string ruleType, Regex regex)[] lexonRules,
//     Func<string, string, int, Lexon> lexonConstructor
//   )
//   {
//     this.lexonRules = lexonRules;
//     this.lexonConstructor = lexonConstructor;
//   }

//   public Lexon[] Lex(string code, int startIndex = 0)
//   {
//     return LexerStatic.Lex(code, lexonRules, lexonConstructor, startIndex);
//   }
// }
