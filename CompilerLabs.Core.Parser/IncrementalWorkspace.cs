using CompilerLabs.Core.Lexer;
using CompilerLabs.Core.Parser.Ast;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CompilerLabs.Core.Parser
{
    /// <summary>
    /// Рабочая область инкрементального парсинга.
    /// Хранит кэш распарсенных блоков кода (ячеек, функций или файлов).
    /// </summary>
    public class IncrementalWorkspace
    {
        // Кэш: ID блока (например "File1" или "Cell_3") -> Состояние этого блока
        private readonly Dictionary<string, BlockState> _blocks = new Dictionary<string, BlockState>();

        // Внутренний класс для хранения состояния одного куска кода
        private class BlockState
        {
            public string TextHash { get; set; } = string.Empty;
            public List<Statement> AstNodes { get; set; } = new List<Statement>();
            public List<string> Errors { get; set; } = new List<string>();
        }

        /// <summary>
        /// Обновляет блок кода. Если текст не изменился, парсинг не запускается!
        /// </summary>
        /// <param name="blockId">Уникальный ID куска кода (имя файла, номер ячейки)</param>
        /// <param name="newCode">Новый текст кода</param>
        public void UpdateBlock(string blockId, string newCode)
        {
            var newHash = ComputeHash(newCode);

            // 1. Проверяем, есть ли блок в кэше и изменился ли его текст
            if (_blocks.TryGetValue(blockId, out var existingState))
            {
                if (existingState.TextHash == newHash)
                {
                    // Текст не менялся. Берем старое AST из кэша.
                    // Мы сэкономили массу времени процессора.
                    return;
                }
            }

            // 2. Текст изменился (или блока вообще не было). Парсим ТОЛЬКО этот кусок!
            var lexer = new Lexer.Lexer(newCode);
            var tokens = lexer.Tokenize();

            var parser = new Parser(tokens);
            var ast = parser.Parse();

            // 3. Сохраняем свежий результат в кэш
            _blocks[blockId] = new BlockState
            {
                TextHash = newHash,
                AstNodes = ast,
                Errors = parser.Errors.ToList()
            };
        }

        /// <summary>
        /// Склеивает все кэшированные AST-деревья в одно общее дерево программы.
        /// Вызывается мгновенно, так как ничего заново не парсится.
        /// </summary>
        public List<Statement> GetFullAst()
        {
            var fullAst = new List<Statement>();
            foreach (var state in _blocks.Values)
            {
                fullAst.AddRange(state.AstNodes);
            }
            return fullAst;
        }

        /// <summary>
        /// Собирает все ошибки синтаксиса со всех блоков.
        /// </summary>
        public List<string> GetAllErrors()
        {
            var allErrors = new List<string>();
            foreach (var kvp in _blocks)
            {
                if (kvp.Value.Errors.Any())
                {
                    allErrors.Add($"--- Ошибки в блоке {kvp.Key} ---");
                    allErrors.AddRange(kvp.Value.Errors);
                }
            }
            return allErrors;
        }

        /// <summary>
        /// Простая функция для хэширования текста (чтобы быстро сравнивать, изменился ли код)
        /// </summary>
        private string ComputeHash(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return System.Convert.ToBase64String(hash);
            }
        }
    }
}