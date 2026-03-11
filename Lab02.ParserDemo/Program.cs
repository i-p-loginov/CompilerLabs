using CompilerLabs.Core.Lexer;
using CompilerLabs.Core.Parser;
using CompilerLabs.Core.Semantic;
namespace Lab02.ParserDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var generator = new CompilerLabs.Core.RandomProgramGenerator();

            var flag = false;

            while (!flag)
            {
                Console.WriteLine("Нажмите любую клавишу для генерации новой программы или 'q' для выхода.");
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Q)
                {
                    flag = true;
                    continue;
                }
                
                var randomCode = generator.Generate(10);

                Console.WriteLine(randomCode);


                var lexer = new Lexer(randomCode);
              
                var tokens = lexer.Tokenize();
              
                var parser = new Parser(tokens);
              
                var ast = parser.Parse();            
                Console.WriteLine($"Успешно распарсено: {ast.Count} инструкций на верхнем уровне.");
           
                var printer = new AstPrinter();           
                printer.Print(ast);            


                var semanticAnalyzer = new SemanticAnalyzer();
                semanticAnalyzer.Analyze(ast);

                if (semanticAnalyzer.Errors.Any())
                {
                    Console.WriteLine("Обнаружены ошибки семантического анализа:");
                    foreach (var error in semanticAnalyzer.Errors)
                    {
                        Console.WriteLine($"- {error}");
                    }
                }
                else
                {
                    Console.WriteLine("Семантический анализ прошел успешно, ошибок не обнаружено.");
                }
            }

        }
    }
}
