using System.Text;

namespace CompilerLabs.Core
{
    public class RandomProgramGenerator
    {
        private readonly Random _random = new();

        //Пул имен для переменных
        private readonly string[] _varNames = { "x", "y", "z", "alpha", "beta", "count", "total", "index", "sum" };

        //Список уже объявленных переменных (чтобы не использовать их до объявления)
        private readonly List<string> _declaredVars = new();

        private readonly string[] _mathOps = { "+", "-", "*", "/" };
        private readonly string[] _compareOps = { "==", "!=", "<", ">", "<=", ">=" };
        private readonly string[] _logicOps = { "&&", "||" };

        /// <summary>
        /// Генерирует случайную программу
        /// </summary>
        /// <param name="statementCount">Количество инструкций на верхнем уровне</param>
        public string Generate(int statementCount = 10)
        {
            _declaredVars.Clear();
            var builder = new StringBuilder();

            //Обязательно объявляем хотя бы пару переменных в самом начале, 
            //чтобы было с чем работать.
            for (int i = 0; i < 3; i++)
            {
                builder.AppendLine(GenerateVarDeclaration(0));
            }

            GenerateBlock(builder, statementCount, 0);

            return builder.ToString();
        }

        private void GenerateBlock(StringBuilder builder, int count, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);

            for (int i = 0; i < count; i++)
            {
                // Выбираем тип следующей инструкции
                // 0: Объявление переменной (var x = ...)
                // 1: Присваивание (x = ...)
                // 2: Print (print ...)
                // 3: If-Else
                // 4: While

                int statementType = _random.Next(5);

                // Ограничиваем вложенность if/while, чтобы код не был слишком глубоким
                if (indentLevel > 2 && statementType > 2)
                    statementType = _random.Next(3);

                switch (statementType)
                {
                    case 0:
                        builder.AppendLine(GenerateVarDeclaration(indentLevel));
                        break;
                    case 1:
                        if (_declaredVars.Count > 0)
                            builder.AppendLine($"{indent}{GetRandomVar()} = {GenerateExpression()};");
                        else
                            builder.AppendLine(GenerateVarDeclaration(indentLevel)); // Fallback
                        break;
                    case 2:
                        builder.AppendLine($"{indent}print {GenerateExpression()};");
                        break;
                    case 3:
                        builder.AppendLine($"{indent}if ({GenerateCondition()}) {{");
                        GenerateBlock(builder, _random.Next(1, 4), indentLevel + 1);

                        if (_random.NextDouble() > 0.5) // 50% шанс на else
                        {
                            builder.AppendLine($"{indent}}} else {{");
                            GenerateBlock(builder, _random.Next(1, 3), indentLevel + 1);
                        }
                        builder.AppendLine($"{indent}}}");
                        break;
                    case 4:
                        builder.AppendLine($"{indent}while ({GenerateCondition()}) {{");
                        GenerateBlock(builder, _random.Next(1, 4), indentLevel + 1);
                        builder.AppendLine($"{indent}}}");
                        break;
                }
            }
        }

        private string GenerateVarDeclaration(int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);

            // Берем случайное имя. Если оно уже есть, просто переопределим (в рамках теста лексера это ок)
            string varName = _varNames[_random.Next(_varNames.Length)];
            if (!_declaredVars.Contains(varName))
                _declaredVars.Add(varName);

            return $"{indent}var {varName} = {GenerateExpression()};";
        }

        private string GenerateExpression()
        {
            // Простые числа или переменные
            if (_random.NextDouble() > 0.6 || _declaredVars.Count == 0)
                return _random.Next(1, 100).ToString();

            if (_random.NextDouble() > 0.5)
                return GetRandomVar();

            // Составное математическое выражение (например: x + 42)
            string left = _random.NextDouble() > 0.5 ? GetRandomVar() : _random.Next(1, 100).ToString();
            string right = _random.NextDouble() > 0.5 ? GetRandomVar() : _random.Next(1, 100).ToString();
            string op = _mathOps[_random.Next(_mathOps.Length)];

            return $"{left} {op} {right}";
        }

        private string GenerateCondition()
        {
            // Например: x <= 10
            string left = GetRandomVarOrNumber();
            string right = GetRandomVarOrNumber();
            string compOp = _compareOps[_random.Next(_compareOps.Length)];

            string condition = $"{left} {compOp} {right}";

            // 30% шанс усложнить условие логическим оператором (например: x <= 10 && y == 5)
            if (_random.NextDouble() > 0.7)
            {
                string logicOp = _logicOps[_random.Next(_logicOps.Length)];
                string extraLeft = GetRandomVarOrNumber();
                string extraRight = GetRandomVarOrNumber();
                string extraComp = _compareOps[_random.Next(_compareOps.Length)];

                condition = $"({condition}) {logicOp} ({extraLeft} {extraComp} {extraRight})";
            }

            return condition;
        }

        private string GetRandomVar()
        {
            if (_declaredVars.Count == 0) return "1"; // Fallback
            return _declaredVars[_random.Next(_declaredVars.Count)];
        }

        private string GetRandomVarOrNumber()
        {
            if (_declaredVars.Count > 0 && _random.NextDouble() > 0.5)
                return GetRandomVar();
            return _random.Next(1, 100).ToString();
        }
    }
}
