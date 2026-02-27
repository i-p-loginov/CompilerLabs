using System.Text;
using Lexer = CompilerLabs.Core.Lexer.Lexer;

namespace Lab01.Lexer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var codeExample = @"var x = 123; print x + 5;";

            var lexer = new Lexer(codeExample);
            
            var tokens = lexer.Tokenize();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("\n--- Generating random test program ---\n");
                var randomProgram = GenerateRandomTestProgram();
                Console.WriteLine(randomProgram);

                var randomLexer = new Lexer(randomProgram);
                var randomTokens = randomLexer.Tokenize();

                foreach (var token in randomTokens)
                {
                    Console.WriteLine(token);
                }
            }

            Console.ReadKey();
        }

        static string GenerateRandomTestProgram()
        {
            var random = new Random();
            var variables = new[] { "a", "b", "c", "x", "y", "z" };
            var operators = new[] { "+", "-", "*", "/" };

            var program = new StringBuilder();

            for (int i = 0; i < 5; i++)
            {
                var varName = variables[random.Next(variables.Length)];
                var number = random.Next(1, 100);
                program.AppendLine($"var {varName} = {number};");
            }

            for (int i = 0; i < 5; i++)
            {
                var var1 = variables[random.Next(variables.Length)];
                var var2 = variables[random.Next(variables.Length)];
                var op = operators[random.Next(operators.Length)];
                program.AppendLine($"print {var1} {op} {var2};");
            }

            return program.ToString();
        }
    }
}
