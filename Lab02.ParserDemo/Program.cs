using CompilerLabs.Core.Lexer;
using CompilerLabs.Core.Parser;
namespace Lab02.ParserDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var generator = new CompilerLabs.Core.RandomProgramGenerator();

            var randomCode = generator.Generate(10);
            Console.WriteLine(randomCode);

            var lexer = new Lexer(randomCode);
            var tokens = lexer.Tokenize();


            var parser = new Parser(tokens);
            var ast = parser.Parse();

            Console.WriteLine($"Успешно распарсено: {ast.Count} инструкций на верхнем уровне.");
            
            var printer = new AstPrinter();
            printer.Print(ast);

            Console.ReadKey();
        }
    }
}
