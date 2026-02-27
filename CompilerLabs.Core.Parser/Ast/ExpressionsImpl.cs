using CompilerLabs.Core.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerLabs.Core.Parser.Ast
{
    // ==========================================
    // ВЫРАЖЕНИЯ (Expressions)
    // ==========================================

    /// <summary>
    /// Числовой литерал (например: 42, 3.14)
    /// </summary>
    public class NumberExpression : Expression
    {
        public double Value { get; }
        public NumberExpression(double value) => Value = value;
    }

    /// <summary>
    /// Строковый литерал
    /// </summary>
    public class StringExpression : Expression
    {
        public string Value { get; }
        public StringExpression(string value) => Value = value;
    }

    /// <summary>
    /// Обращение к переменной по имени (например: x)
    /// </summary>
    public class VariableExpression : Expression
    {
        public string Name { get; }
        public VariableExpression(string name) => Name = name;
    }

    /// <summary>
    /// Бинарная операция (математика, логика, сравнение).
    /// Например: x + 5, y == 10, a && b
    /// </summary>
    public class BinaryExpression : Expression
    {
        public Expression Left { get; }
        public TokenType Operator { get; } // Храним тип оператора (PLUS, MINUS, EQEQ, AND и т.д.)
        public Expression Right { get; }

        public BinaryExpression(Expression left, TokenType op, Expression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }

    /// <summary>
    /// Унарная операция (оператор перед одним выражением).
    /// Например: -x, !isValid
    /// </summary>
    public class UnaryExpression : Expression
    {
        public TokenType Operator { get; } // MINUS или EXCL (!)
        public Expression Right { get; }

        public UnaryExpression(TokenType op, Expression right)
        {
            Operator = op;
            Right = right;
        }
    }

    /// <summary>
    /// Выражение присваивания (например: x = 10 + 5)
    /// Почему это Expression? Чтобы можно было делать так: a = b = 5;
    /// </summary>
    public class AssignExpression : Expression
    {
        public string Name { get; } // Имя переменной, в которую записываем
        public Expression Value { get; } // То, что записываем

        public AssignExpression(string name, Expression value)
        {
            Name = name;
            Value = value;
        }
    }
}
