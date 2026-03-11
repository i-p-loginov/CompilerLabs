using CompilerLabs.Core.Parser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerLabs.Core.Semantic
{
    public class SemanticAnalyzer
    {
        private SemanticEnvironment _environment = new SemanticEnvironment();
        private readonly List<string> _errors = new List<string>();

        public void Analyze(IEnumerable<Statement> statements)
        {
            foreach (var statement in statements)
            {
                VisitStatement(statement);
            }
        }

        public void VisitStatement(Statement statement)
        {
            switch (statement)
            {
                case VarStatement varStatement:
                    if (varStatement.Initializer != null)
                    {
                        VisitExpression(varStatement.Initializer);
                    }

                    if (!_environment.DefineVariable(varStatement.Name))
                    {
                        _errors.Add($"Variable '{varStatement.Name}' is already defined.");
                    }

                    break;
                case PrintStatement printStatement:
                    VisitExpression(printStatement.Expression);
                    break;
                case ExpressionStatement expressionStatement:
                    VisitExpression(expressionStatement.Expression);
                    break;
                case BlockStatement blockStatement:
                    var previousEnvironment = _environment;
                    _environment = new SemanticEnvironment(previousEnvironment);

                    foreach (var innerStatement in blockStatement.Statements)
                    {
                        VisitStatement(innerStatement);
                    }

                    _environment = previousEnvironment;

                    break;
                case IfStatement ifStatement:
                    VisitExpression(ifStatement.Condition);
                    VisitStatement(ifStatement.ThenBranch);
                    if (ifStatement.ElseBranch != null)
                    {
                        VisitStatement(ifStatement.ElseBranch);
                    }
                    break;

                case WhileStatement whileStatement:
                    VisitExpression(whileStatement.Condition);
                    VisitStatement(whileStatement.Body);
                    break;

                default:
                    _errors.Add($"Unsupported statement type: {statement.GetType().Name}");
                    break;
            }
        }


        public void VisitExpression(Expression expression)
        {
            switch (expression)
            {
                case NumberExpression n:
                case StringExpression s:
                    break;

                case VariableExpression v:
                    if (!_environment.IsVariableDefined(v.Name))
                    {
                        _errors.Add($"Variable '{v.Name}' is not defined.");
                    }
                    break;
                case AssignExpression a:
                    VisitExpression(a.Value);
                    if (!_environment.IsVariableDefined(a.Name))
                    {
                        _errors.Add($"Variable '{a.Name}' is not defined.");
                    }
                    break;
                case BinaryExpression b:
                    VisitExpression(b.Left);
                    VisitExpression(b.Right);
                    break;
                case UnaryExpression u:
                    VisitExpression(u.Right);
                    break;
                default:
                    _errors.Add($"Unsupported expression type: {expression.GetType().Name}");
                    break;
            }
        }

        public IEnumerable<string> Errors => _errors;
    }
}
