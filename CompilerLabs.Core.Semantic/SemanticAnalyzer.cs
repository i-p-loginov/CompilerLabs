using CompilerLabs.Core.Parser.Ast;
using System.Collections.Generic;
using System.Linq;

namespace CompilerLabs.Core.Semantic
{
    public class SemanticAnalyzer
    {
        private SemanticEnvironment _environment = new SemanticEnvironment();
        private readonly List<string> _errors = new List<string>();
        public IEnumerable<string> Errors => _errors;

        public void Analyze(IEnumerable<Statement> statements)
        {
            foreach (var statement in statements)
            {
                VisitStatement(statement);
            }
        }

        // Главный диспетчер теперь выглядит чисто
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

        public void VisitExpression(Expression expression)
        {
            switch (expression)
            {
                case NumberExpression n: break;
                case StringExpression s: break;
                case VariableExpression v: AnalyzeVariableExpression(v); break;
                case AssignExpression a: AnalyzeAssignExpression(a); break;
                case BinaryExpression b: AnalyzeBinaryExpression(b); break;
                case UnaryExpression u: AnalyzeUnaryExpression(u); break;
                default:
                    _errors.Add($"[{expression.Line}:{expression.Column}] Неподдерживаемое выражение: {expression.GetType().Name}");
                    break;
            }
        }

        private void AnalyzeVarStatement(VarStatement stmt)
        {
            if (!_environment.DefineVariable(stmt.Name, false))
            {
                _errors.Add($"[{stmt.Line}:{stmt.Column}] Переменная '{stmt.Name}' уже объявлена в этой области видимости.");
            }

            if (stmt.Initializer != null)
            {
                VisitExpression(stmt.Initializer);
                _environment.SetInitialized(stmt.Name);
            }
        }

        private void AnalyzePrintStatement(PrintStatement stmt)
        {
            VisitExpression(stmt.Expression);
            CheckUnusedVariables();
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

            CheckUnusedVariables();
            _environment = previousEnvironment;
        }

        private void AnalyzeIfStatement(IfStatement stmt)
        {
            VisitExpression(stmt.Condition);
            VisitStatement(stmt.ThenBranch);

            if (stmt.ElseBranch != null)
            {
                VisitStatement(stmt.ElseBranch);
            }
        }

        private void AnalyzeWhileStatement(WhileStatement stmt)
        {
            VisitExpression(stmt.Condition);
            VisitStatement(stmt.Body);
        }

        private void CheckUnusedVariables()
        {
            foreach (var symbol in _environment.GetLocalVariables())
            {
                if (!symbol.IsUsed)
                {
                    _errors.Add($"[Semantic Warning] Переменная '{symbol.Name}' объявлена, но ни разу не использовалась.");
                }
            }
        }

        private void AnalyzeVariableExpression(VariableExpression expr)
        {
            var symbol = _environment.GetVariable(expr.Name);
            if (symbol == null)
            {
                _errors.Add($"[{expr.Line}:{expr.Column}] Использование необъявленной переменной '{expr.Name}'.");
            }
            else
            {
                symbol.IsUsed = true;

                if (!symbol.IsInitialized)
                {
                    _errors.Add($"[{expr.Line}:{expr.Column}] Использование неинициализированной переменной '{expr.Name}'.");
                }
            }
        }

        private void AnalyzeAssignExpression(AssignExpression expr)
        {
            VisitExpression(expr.Value);

            if (!_environment.IsVariableDefined(expr.Name))
            {
                _errors.Add($"[{expr.Line}:{expr.Column}] Попытка записи в необъявленную переменную '{expr.Name}'.");
            }
            else
            {
                _environment.SetInitialized(expr.Name);
            }
        }

        private void AnalyzeBinaryExpression(BinaryExpression expr)
        {
            VisitExpression(expr.Left);
            VisitExpression(expr.Right);
        }

        private void AnalyzeUnaryExpression(UnaryExpression expr)
        {
            VisitExpression(expr.Right);
        }
    }
}