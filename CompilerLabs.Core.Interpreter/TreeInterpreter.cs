using CompilerLabs.Core.Lexer;
using CompilerLabs.Core.Parser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerLabs.Core.Interpreter
{
    public class TreeInterpreter
    {
        private RuntimeEnvironment _environment = new RuntimeEnvironment();

        public void Interpret(IEnumerable<Statement> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[CRITICAL RUNTIME ERROR]: {ex.Message}");
                Console.ResetColor();
            }
        }


        private void Execute(Statement stmt)
        {
            switch (stmt)
            {
                case PrintStatement p:
                    object? value = Evaluate(p.Expression);
                    Console.WriteLine(value);
                    break;

                case VarStatement v:
                    object? initValue = null;
                    if (v.Initializer != null)
                    {
                        initValue = Evaluate(v.Initializer);
                    }
                    _environment.Define(v.Name, initValue);
                    break;

                case ExpressionStatement e:
                    Evaluate(e.Expression);
                    break;

                case BlockStatement b:
                    var previousEnv = _environment;
                    _environment = new RuntimeEnvironment(previousEnv);
                    try
                    {
                        foreach (var innerStmt in b.Statements)
                        {
                            Execute(innerStmt);
                        }
                    }
                    finally
                    {
                        _environment = previousEnv;
                    }
                    break;

                case IfStatement i:
                    if (IsTruthy(Evaluate(i.Condition)))
                    {
                        Execute(i.ThenBranch);
                    }
                    else if (i.ElseBranch != null)
                    {
                        Execute(i.ElseBranch);
                    }
                    break;

                case WhileStatement w:
                    while (IsTruthy(Evaluate(w.Condition)))
                    {
                        Execute(w.Body);
                    }
                    break;
                case FunctionStatement f: 
                    _environment.DefineFunction(f.Name, f);
                    break;
                case ReturnStatement r:
                    object? returnValue = null;
                    if (r.Value != null)
                    {
                        returnValue = Evaluate(r.Value);
                    }
                    throw new ReturnException(returnValue);
                default:
                    throw new Exception($"Неизвестная инструкция: {stmt.GetType().Name}");
            }
        }

        private object? Evaluate(Expression expr)
        {
            switch (expr)
            {
                case NumberExpression n: return n.Value;
                case StringExpression s: return s.Value;

                case VariableExpression v:
                    return _environment.Get(v.Name);

                case AssignExpression a:
                    object? val = Evaluate(a.Value);
                    _environment.Assign(a.Name, val);
                    return val;

                case BinaryExpression b:
                    // Ленивые вычисления для && и ||
                    if (b.Operator == TokenType.OR)
                    {
                        object? leftOr = Evaluate(b.Left);
                       
                        if (IsTruthy(leftOr)) 
                            return true; // Если слева true, правое даже не вычисляем!
                      
                        return IsTruthy(Evaluate(b.Right));
                    }
                    if (b.Operator == TokenType.AND)
                    {
                        object? leftAnd = Evaluate(b.Left);
                      
                        if (!IsTruthy(leftAnd)) 
                            return false; // Если слева false, правое не вычисляем!
                       
                        return IsTruthy(Evaluate(b.Right));
                    }

                    // Для остального вычисляем обе части
                    object? left = Evaluate(b.Left);
                    object? right = Evaluate(b.Right);

                    switch (b.Operator)
                    {
                        // Математика (мы точно знаем, что тут double, т.к. SemanticAnalyzer проверил)
                        case TokenType.MINUS: 
                            return (double)left! - (double)right!;
                      
                        case TokenType.SLASH:
                           
                            if ((double)right! == 0) 
                                throw new Exception("Деление на ноль!");
                           
                            return (double)left! / (double)right!;
                        case TokenType.STAR: return (double)left! * (double)right!;

                        case TokenType.PLUS:
                         
                            if (left is double l && right is double r) 
                                return l + r;
                         
                            if (left is string || right is string) 
                                return left?.ToString() + right?.ToString();
                        
                            break;

                        // Сравнения
                        case TokenType.GT: return (double)left! > (double)right!;
                        case TokenType.GTEQ: return (double)left! >= (double)right!;
                        case TokenType.LT: return (double)left! < (double)right!;
                        case TokenType.LTEQ: return (double)left! <= (double)right!;

                        // Равенство
                        case TokenType.EQEQ: return IsEqual(left, right);
                        case TokenType.NEQ: return !IsEqual(left, right);
                    }
                    throw new Exception("Неизвестный бинарный оператор.");

                case UnaryExpression u:
                    object? rightVal = Evaluate(u.Right);
                    if (u.Operator == TokenType.MINUS) return -(double)rightVal!;
                    if (u.Operator == TokenType.EXCL) return !IsTruthy(rightVal);
                    return rightVal;
                
                case CallExpression call:
                    var functionDecl = _environment.GetFunction(call.CalleeName);
                    var args = new List<object?>();
                    foreach(var argExpr in call.Arguments)
                    {
                        args.Add(Evaluate(argExpr));
                    }
                    
                    var callEnv = new RuntimeEnvironment(_environment);
                    for (int i = 0; i < functionDecl.Parameters.Count; i++)
                    {
                        string paramName = functionDecl.Parameters[i];
                        object? argValue = i < args.Count ? args[i] : null;
                        callEnv.Define(paramName, argValue);
                    }

                    var previousEnv = _environment;
                    _environment = callEnv;
                    try
                    {
                        foreach(var stmt in functionDecl.Body.Statements)
                        {
                            Execute(stmt);
                        }
                    }
                    catch (ReturnException ret)
                    {
                        return ret.Value;
                    }
                    finally
                    {
                        _environment = previousEnv;
                    }

                    return null;

                default:
                    throw new Exception($"Неизвестное выражение: {expr.GetType().Name}");
            }
        }

        // Вспомогательный метод: проверка на Истину
        private bool IsTruthy(object? obj)
        {
            if (obj == null) return false;
            if (obj is bool b) return b;
            return true; // В нашем языке всё, что не null и не false - это true
        }

        // Вспомогательный метод: проверка на равенство
        private bool IsEqual(object? a, object? b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }
    }
}
