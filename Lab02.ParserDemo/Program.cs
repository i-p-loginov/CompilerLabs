using CompilerLabs.Core.Lexer;
using CompilerLabs.Core.Parser;
using CompilerLabs.Core.Semantic;
using System.IO;

namespace Lab02.ParserDemo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var generator = new CompilerLabs.Core.RandomProgramGenerator();
            var flag = false;

            while (!flag)
            {
                Console.WriteLine("1 - Стандартный парсинг (анализ из оперативной памяти)");
                Console.WriteLine("2 - Инкрементальный парсинг (работа с файловой системой и кэшем)");
                Console.WriteLine("q - Завершить работу");
                Console.Write("\nВыберите режим: ");

                var key = Console.ReadKey().KeyChar;
                Console.WriteLine("\n");

                if (key == 'q' || key == 'Q')
                {
                    flag = true;
                    continue;
                }
                else if (key == '1')
                {
                    RunRegular(generator);
                }
                else if (key == '2')
                {
                    await RunIncremental(generator);
                }
                else
                {
                    Console.WriteLine("Некорректный ввод. Пожалуйста, введите 1, 2 или q.");
                }
            }
        }
        static void RunRegular(CompilerLabs.Core.RandomProgramGenerator generator)
        {
            Console.WriteLine("-- обычный парсер -- ");
            var randomCode = generator.Generate(10);
            Console.WriteLine(randomCode);

            var lexer = new Lexer(randomCode);
            var tokens = lexer.Tokenize();

            var parser = new Parser(tokens);
            var ast = parser.Parse();

            Console.WriteLine($"Успешно распознано: {ast.Count} инструкций на верхнем уровне.");

            var printer = new AstPrinter();
            printer.Print(ast);

            var mermaidGen = new MermaidAstGenerator();
            string mermaidCode = mermaidGen.Generate(ast);

            File.WriteAllText("AST_Visualization.md", mermaidCode);
            Console.WriteLine("\nГраф сохранен в файл AST_Visualization.mermaid");

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
      
        static async Task RunIncremental(CompilerLabs.Core.RandomProgramGenerator generator)
        {
            Console.WriteLine("- инкрементальный парсер -");

            var workspace = new IncrementalWorkspace();

            string tempDir = Path.Combine(Path.GetTempPath(), "CompilerLabs_Workspace");
            Directory.CreateDirectory(tempDir);
            Console.WriteLine($"Рабочая директория создана: {tempDir}");

            Console.WriteLine("\n1. Генерация файлов с исходным кодом...");
            for (int i = 1; i <= 3; i++)
            {
                string fileName = $"File_{i}.txt";
                string path = Path.Combine(tempDir, fileName);
                string code = generator.Generate(4);

                File.WriteAllText(path, code);
                Console.WriteLine($"> Сохранен файл {fileName}");

                await workspace.UpdateBlockAsync(fileName, code);
            }

            var fullAst = workspace.GetFullAst();
            Console.WriteLine($"\nВсе файлы успешно загружены в Workspace. Общее число узлов: {fullAst.Count}");

            Console.WriteLine("\n2. Имитация работы в редакторе. Добавление кода в File_2.txt...");
            string path2 = Path.Combine(tempDir, "File_2.txt");
            string existingCode = File.ReadAllText(path2);
            string newCode = existingCode + "\nvar testVariable = 100;";
            File.WriteAllText(path2, newCode);

            await workspace.UpdateBlockAsync("File_1.txt", File.ReadAllText(Path.Combine(tempDir, "File_1.txt")));
            await workspace.UpdateBlockAsync("File_2.txt", File.ReadAllText(path2));
            await workspace.UpdateBlockAsync("File_3.txt", File.ReadAllText(Path.Combine(tempDir, "File_3.txt")));

            Console.WriteLine("Рабочая область обновлена (File_1 и File_3 загружены из кэша, File_2 проанализирован повторно).");

            var updatedAst = workspace.GetFullAst();
            Console.WriteLine($"\nСинтаксическое дерево обновлено. Всего узлов: {updatedAst.Count}");

            Console.WriteLine("\n3. Семантический анализ обновленного проекта:");
            var semanticAnalyzer = new SemanticAnalyzer();
            semanticAnalyzer.Analyze(updatedAst);

            if (semanticAnalyzer.Errors.Any())
            {
                Console.WriteLine("Выявлены следующие семантические ошибки:");
                foreach (var error in semanticAnalyzer.Errors)
                {
                    Console.WriteLine($"- {error}");
                }
            }
            else
            {
                Console.WriteLine("Семантический анализ завершен без ошибок.");
            }
        }
    }
}