using CompilerLabs.Core;
using CompilerLabs.Core.Lexer;
using System.Text;

namespace Lab01.LexerDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var generator = new RandomProgramGenerator();

            string randomCode = generator.Generate(10);

            Console.WriteLine("=== СГЕНЕРИРОВАННЫЙ КОД ===");
            Console.WriteLine(randomCode);
            Console.WriteLine("===========================\n");

            var lexer = new Lexer(randomCode);
            var tokens = lexer.Tokenize();

            Console.WriteLine("=== ТОКЕНЫ ===");
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }

            Console.ReadKey();
        }
    }
}
