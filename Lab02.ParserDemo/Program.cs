using CompilerLabs.Core.Interpreter;
using CompilerLabs.Core.Lexer;
using CompilerLabs.Core.Parser;
using CompilerLabs.Core.Semantic;
using System.IO;

namespace Lab02.ParserDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var flag = false;

            while (!flag)
            {
                Console.WriteLine("==================================================================");
                Console.WriteLine("АВТОМАТИЧЕСКИЙ ПРОГОН ТЕСТОВ (ЭНТЕРПРАЙЗ ТЕСТ-СТЕНД)");
                Console.WriteLine("==================================================================");
                Console.WriteLine("1 - Запустить тесты семантики (Ошибки и Warnings)");
                Console.WriteLine("2 - Запустить тесты интерпретатора (Реальное выполнение кода)");
                Console.WriteLine("q - Завершить работу");
                Console.WriteLine("==================================================================");
                Console.Write("Выберите режим: ");

                var key = Console.ReadKey().KeyChar;
                Console.WriteLine("\n");

                if (key == 'q' || key == 'Q')
                {
                    flag = true;
                    continue;
                }
                else if (key == '1')
                {
                    RunTestSuite(runInterpreter: false);
                }
                else if (key == '2')
                {
                    RunTestSuite(runInterpreter: true);
                }
                else
                {
                    Console.WriteLine("Некорректный ввод. Пожалуйста, введите 1, 2 или q.");
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void RunTestSuite(bool runInterpreter)
        {
            string modeName = runInterpreter ? "ИНТЕРПРЕТАТОРА (РАНТАЙМ)" : "СЕМАНТИКИ (СТАТИЧЕСКИЙ АНАЛИЗ)";
            Console.WriteLine($"=== ЗАПУСК ТЕСТОВОГО СТЕНДА {modeName} ИЗ ФАЙЛОВ ===\n");

            string testsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests");

            if (!Directory.Exists(testsDirectory))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ОШИБКА] Папка с тестами не найдена по пути:\n{testsDirectory}");
                Console.ResetColor();
                return;
            }

            var testFiles = Directory.GetFiles(testsDirectory, "*.txt").OrderBy(f => f).ToList();

            if (testFiles.Count == 0)
            {
                Console.WriteLine($"В папке {testsDirectory} нет тестовых файлов (.txt).");
                return;
            }

            foreach (var filePath in testFiles)
            {
                string fileName = Path.GetFileName(filePath);

                if (runInterpreter && !fileName.Contains("Interp")) continue;
                if (!runInterpreter && fileName.Contains("Interp")) continue;


                Console.WriteLine($"\n> ЗАПУСК ТЕСТА: {fileName}");
                Console.WriteLine("--------------------------------------------------");

                string code = File.ReadAllText(filePath);
                if (!runInterpreter) Console.WriteLine(code.Trim());
                Console.WriteLine("--------------------------------------------------");

                var lexer = new Lexer(code);
                var tokens = lexer.Tokenize();

                var parser = new Parser(tokens);
                var ast = parser.Parse();

                if (parser.Errors.Any())
                {
                    Console.WriteLine("ОШИБКИ ПАРСИНГА (Синтаксис):");
                    foreach (var err in parser.Errors) Console.WriteLine($"  - {err}");
                    continue;
                }

                var semanticAnalyzer = new SemanticAnalyzer();
                semanticAnalyzer.Analyze(ast);

                if (!runInterpreter)
                {
                    PrintSemanticResults(semanticAnalyzer);
                }
                else
                {
                    if (semanticAnalyzer.Errors.Any())
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Семантический анализ выявил ошибки. Выполнение отменено.");
                        foreach (var error in semanticAnalyzer.Errors)
                        {
                            Console.WriteLine($"  - {error}");
                        }
                        Console.ResetColor();
                        continue;
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(">>> ВЫВОД ПРОГРАММЫ:");

                    var interpreter = new TreeInterpreter();
                    interpreter.Interpret(ast);

                    Console.ResetColor();
                }
            }

            Console.WriteLine("\n=== ТЕСТИРОВАНИЕ ЗАВЕРШЕНО ===");
        }

        static void PrintSemanticResults(SemanticAnalyzer analyzer)
        {
            bool hasIssues = false;

            if (analyzer.Errors.Any())
            {
                hasIssues = true;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ОШИБКИ КОМПИЛЯЦИИ (Errors):");
                foreach (var error in analyzer.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
                Console.ResetColor();
            }

            if (analyzer.Warnings.Any())
            {
                hasIssues = true;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("ПРЕДУПРЕЖДЕНИЯ (Warnings):");
                foreach (var warning in analyzer.Warnings)
                {
                    Console.WriteLine($"  - {warning}");
                }
                Console.ResetColor();
            }

            if (!hasIssues)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Семантический анализ прошел успешно! Код чист.");
                Console.ResetColor();
            }
        }
    }
}