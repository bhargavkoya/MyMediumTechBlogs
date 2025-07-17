using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliCal
{
    public class ExpressionParser
    {
        public double Evaluate(string expression)
        {
            var tokens = Tokenize(expression);
            return EvaluateTokens(tokens);
        }

        private List<Token> Tokenize(string expression)
        {
            var tokens = new List<Token>();
            var current = "";

            foreach (char c in expression.Replace(" ", ""))
            {
                if (char.IsDigit(c) || c == '.')
                {
                    current += c;
                }
                else if ("+-*/".Contains(c))
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        tokens.Add(new Token(TokenType.Number, current));
                        current = "";
                    }
                    tokens.Add(new Token(TokenType.Operator, c.ToString()));
                }
            }

            if (!string.IsNullOrEmpty(current))
                tokens.Add(new Token(TokenType.Number, current));

            return tokens;
        }

        private double EvaluateTokens(List<Token> tokens)
        {
            // Left-to-right evaluation with proper operator precedence
            var result = double.Parse(tokens[0].Value);

            for (int i = 1; i < tokens.Count; i += 2)
            {
                var operation = tokens[i].Value;
                var operand = double.Parse(tokens[i + 1].Value);

                result = operation switch
                {
                    "+" => result + operand,
                    "-" => result - operand,
                    "*" => result * operand,
                    "/" => operand == 0 ? throw new DivideByZeroException() : result / operand,
                    _ => throw new InvalidOperationException($"Unknown operator: {operation}")
                };
            }

            return result;
        }
    }

    public record Token(TokenType Type, string Value);
    public enum TokenType { Number, Operator }
}
