using CompilerLabs.Core.Lexer;
using CompilerLabs.Core.Parser.Ast;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CompilerLabs.Core.Semantic
{
    public class SemanticAnalyzer
    {
        private SemanticEnvironment _environment = new SemanticEnvironment();

        // Разделяем критичные ошибки и предупреждения (Warnings)
        private readonly List<string> _errors = new List<string>();
        private readonly List<string> _warnings = new List<string>();

        public IEnumerable<string> Errors => _errors;
        public IEnumerable<string> Warnings => _warnings;

        public void Analyze(IEnumerable<Statement> statements)
        {
            foreach (var statement in statements)
            {
                VisitStatement(statement);
            }
            // Проверяем "мертвые души" в глобальном скоупе
            CheckUnusedVariables();
        }

        public void VisitStatement(Statement statement)
        {
            switch (statement)
            {
                case VarStatement v: AnalyzeVarStatement(v); break;
                case PrintStatement p: AnalyzePrintStatement(p); break;
                case ExpressionStatement e: AnalyzeExpressionStatement(e); break;
                case BlockStatement b: AnalyzeBlockStatement(b); break;
                case IfStatement i: AnalyzeIfStatement(i); break;
                case WhileStatement w: AnalyzeWhileStatement(w); break;
                default:
                    _errors.Add($"[{statement.Line}:{statement.Column}] Неподдерживаемая инструкция: {statement.GetType().Name}");
                    break;
            }
        }

        private void AnalyzeVarStatement(VarStatement stmt)
        {
            DataType initType = DataType.Unknown;

            // Если есть инициализатор, вычисляем его тип
            if (stmt.Initializer != null)
            {
                initType = VisitExpression(stmt.Initializer);
            }

            if (!_environment.DefineVariable(stmt.Name, stmt.Initializer != null, initType))
            {
                _errors.Add($"[{stmt.Line}:{stmt.Column}] Переменная '{stmt.Name}' уже объявлена в этой области видимости.");
            }
        }

        private void AnalyzePrintStatement(PrintStatement stmt)
        {
            VisitExpression(stmt.Expression);
        }

        private void AnalyzeExpressionStatement(ExpressionStatement stmt)
        {
            VisitExpression(stmt.Expression);
        }

        private void AnalyzeBlockStatement(BlockStatement stmt)
        {
            var previousEnvironment = _environment;
            _environment = new SemanticEnvironment(previousEnvironment);

            foreach (var innerStatement in stmt.Statements)
            {
                VisitStatement(innerStatement);
            }

            // Выходим из блока - проверяем, не забыли ли использовать переменные
            CheckUnusedVariables();
            _environment = previousEnvironment;
        }

        private void AnalyzeIfStatement(IfStatement stmt)
        {
            DataType conditionType = VisitExpression(stmt.Condition);

            // Проверка типов: Условие IF должно быть логическим!
            if (conditionType != DataType.Bool && conditionType != DataType.Unknown)
            {
                _errors.Add($"[{stmt.Line}:{stmt.Column}] Условие 'if' должно быть логическим выражением (Bool), а получено: {conditionType}.");
            }

            // Анализ достижимости кода (Dead Code Analysis)
            if (IsAlwaysFalse(stmt.Condition))
            {
                _warnings.Add($"[{stmt.Line}:{stmt.Column}] Обнаружен недостижимый код: ветка 'then' (if) никогда не выполнится.");
            }

            VisitStatement(stmt.ThenBranch);

            if (stmt.ElseBranch != null)
            {
                VisitStatement(stmt.ElseBranch);
            }
        }

        private void AnalyzeWhileStatement(WhileStatement stmt)
        {
            DataType conditionType = VisitExpression(stmt.Condition);

            // Проверка типов: Условие WHILE должно быть логическим!
            if (conditionType != DataType.Bool && conditionType != DataType.Unknown)
            {
                _errors.Add($"[{stmt.Line}:{stmt.Column}] Условие 'while' должно быть логическим выражением (Bool), а получено: {conditionType}.");
            }

            // Анализ достижимости кода
            if (IsAlwaysFalse(stmt.Condition))
            {
                _warnings.Add($"[{stmt.Line}:{stmt.Column}] Обнаружен недостижимый код: тело цикла 'while' никогда не выполнится.");
            }

            VisitStatement(stmt.Body);
        }

        private void CheckUnusedVariables()
        {
            foreach (var symbol in _environment.GetLocalVariables())
            {
                if (!symbol.IsUsed)
                {
                    _warnings.Add($"[Semantic Warning] Переменная '{symbol.Name}' объявлена, но ни разу не использована.");
                }
            }
        }

        public DataType VisitExpression(Expression expression)
        {
            switch (expression)
            {
                case NumberExpression n: return DataType.Number;
                case StringExpression s: return DataType.String;
                case VariableExpression v: return AnalyzeVariableExpression(v);
                case AssignExpression a: return AnalyzeAssignExpression(a);
                case BinaryExpression b: return AnalyzeBinaryExpression(b);
                case UnaryExpression u: return AnalyzeUnaryExpression(u);
                default:
                    _errors.Add($"[{expression.Line}:{expression.Column}] Неподдерживаемое выражение: {expression.GetType().Name}");
                    return DataType.Unknown;
            }
        }

        private DataType AnalyzeVariableExpression(VariableExpression expr)
        {
            var symbol = _environment.GetVariable(expr.Name);
            if (symbol == null)
            {
                _errors.Add($"[{expr.Line}:{expr.Column}] Использование необъявленной переменной '{expr.Name}'.");
                return DataType.Unknown;
            }

            symbol.IsUsed = true; // Снимаем метку "неиспользуемая"

            if (!symbol.IsInitialized)
            {
                _errors.Add($"[{expr.Line}:{expr.Column}] Использование неинициализированной переменной '{expr.Name}'.");
            }

            return symbol.Type;
        }

        private DataType AnalyzeAssignExpression(AssignExpression expr)
        {
            DataType valueType = VisitExpression(expr.Value);
            var symbol = _environment.GetVariable(expr.Name);

            if (symbol == null)
            {
                _errors.Add($"[{expr.Line}:{expr.Column}] Попытка записи в необъявленную переменную '{expr.Name}'.");
                return valueType;
            }

            symbol.IsInitialized = true;

            // Строгая типизация: нельзя менять тип переменной "на лету"
            if (symbol.Type != DataType.Unknown && valueType != DataType.Unknown && symbol.Type != valueType)
            {
                _errors.Add($"[{expr.Line}:{expr.Column}] Ошибка типов: нельзя присвоить значение типа {valueType} переменной '{expr.Name}' (ожидался тип {symbol.Type}).");
            }
            else if (symbol.Type == DataType.Unknown && valueType != DataType.Unknown)
            {
                // Если переменная была объявлена без типа (var x;), фиксируем тип при первом присваивании
                symbol.Type = valueType;
            }

            return symbol.Type;
        }

        private DataType AnalyzeBinaryExpression(BinaryExpression expr)
        {
            DataType leftType = VisitExpression(expr.Left);
            DataType rightType = VisitExpression(expr.Right);

            // Прокидываем Unknown наверх, чтобы не спамить каскадными ошибками
            if (leftType == DataType.Unknown || rightType == DataType.Unknown)
                return DataType.Unknown;

            switch (expr.Operator)
            {
                case TokenType.PLUS:
                    if (leftType == DataType.String || rightType == DataType.String) return DataType.String; // Конкатенация
                    if (leftType == DataType.Number && rightType == DataType.Number) return DataType.Number; // Математика
                    _errors.Add($"[{expr.Line}:{expr.Column}] Ошибка типов: нельзя применить оператор '+' к {leftType} и {rightType}.");
                    return DataType.Unknown;

                case TokenType.MINUS:
                case TokenType.STAR:
                case TokenType.SLASH:
                    if (leftType == DataType.Number && rightType == DataType.Number) return DataType.Number;
                    _errors.Add($"[{expr.Line}:{expr.Column}] Ошибка типов: оператор '{expr.Operator}' работает только с числами (Number). Получено: {leftType} и {rightType}.");
                    return DataType.Unknown;

                case TokenType.LT:
                case TokenType.GT:
                case TokenType.LTEQ:
                case TokenType.GTEQ:
                    if (leftType == DataType.Number && rightType == DataType.Number) return DataType.Bool;
                    _errors.Add($"[{expr.Line}:{expr.Column}] Ошибка типов: операторы сравнения работают только с числами (Number). Получено: {leftType} и {rightType}.");
                    return DataType.Unknown;

                case TokenType.EQEQ:
                case TokenType.NEQ:
                    if (leftType != rightType)
                    {
                        _warnings.Add($"[{expr.Line}:{expr.Column}] Сравнение на равенство разных типов ({leftType} и {rightType}) всегда будет ложным.");
                    }
                    return DataType.Bool;

                case TokenType.AND:
                case TokenType.OR:
                    if (leftType == DataType.Bool && rightType == DataType.Bool) return DataType.Bool;
                    _errors.Add($"[{expr.Line}:{expr.Column}] Ошибка типов: логические операторы (&&, ||) требуют тип Bool. Получено: {leftType} и {rightType}.");
                    return DataType.Unknown;

                default:
                    return DataType.Unknown;
            }
        }

        private DataType AnalyzeUnaryExpression(UnaryExpression expr)
        {
            DataType rightType = VisitExpression(expr.Right);
            if (rightType == DataType.Unknown) return DataType.Unknown;

            if (expr.Operator == TokenType.MINUS)
            {
                if (rightType != DataType.Number)
                {
                    _errors.Add($"[{expr.Line}:{expr.Column}] Ошибка типов: унарный минус применяется только к числам. Получено: {rightType}.");
                    return DataType.Unknown;
                }
                return DataType.Number;
            }

            if (expr.Operator == TokenType.EXCL) // Логическое НЕ (!)
            {
                if (rightType != DataType.Bool)
                {
                    _errors.Add($"[{expr.Line}:{expr.Column}] Ошибка типов: оператор '!' применяется только к Bool. Получено: {rightType}.");
                    return DataType.Unknown;
                }
                return DataType.Bool;
            }

            return rightType;
        }

        private bool IsAlwaysFalse(Expression expr)
        {
            if (expr is BinaryExpression bin)
            {
                if (bin.Operator == TokenType.EQEQ)
                {
                    if (bin.Left is NumberExpression numL && bin.Right is NumberExpression numR)
                        return numL.Value != numR.Value; // 1 == 2 -> false
                }
                else if (bin.Operator == TokenType.NEQ)
                {
                    if (bin.Left is NumberExpression numL && bin.Right is NumberExpression numR)
                        return numL.Value == numR.Value; // 1 != 1 -> false
                }
            }
            return false;
        }
    }
}