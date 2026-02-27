using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerLabs.Core.Parser.Ast
{
    /// <summary>
    /// Инструкция-обертка над выражением.
    /// Позволяет использовать выражение там, где ожидается инструкция (например: "x = 5;" или "foo();")
    /// </summary>
    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; }
        public ExpressionStatement(Expression expression) => Expression = expression;
    }

    /// <summary>
    /// Вывод в консоль: print x + 2;
    /// </summary>
    public class PrintStatement : Statement
    {
        public Expression Expression { get; }
        public PrintStatement(Expression expression) => Expression = expression;
    }

    /// <summary>
    /// Объявление новой переменной: var x = 5;
    /// </summary>
    public class VarStatement : Statement
    {
        public string Name { get; }
        public Expression Initializer { get; } // Может быть null, если разрешено писать просто "var x;"

        public VarStatement(string name, Expression initializer)
        {
            Name = name;
            Initializer = initializer;
        }
    }

    /// <summary>
    /// Блок кода. Группирует несколько инструкций в одну (всё, что внутри { ... }).
    /// Жизненно необходимо для if и while.
    /// </summary>
    public class BlockStatement : Statement
    {
        public List<Statement> Statements { get; }
        public BlockStatement(List<Statement> statements) => Statements = statements;
    }

    /// <summary>
    /// Инструкция ветвления: if (условие) { ... } else { ... }
    /// </summary>
    public class IfStatement : Statement
    {
        public Expression Condition { get; }
        public Statement ThenBranch { get; } // То, что выполняется, если true
        public Statement ElseBranch { get; } // Может быть null, если блока else нет

        public IfStatement(Expression condition, Statement thenBranch, Statement elseBranch = null)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }
    }

    /// <summary>
    /// Цикл: while (условие) { ... }
    /// </summary>
    public class WhileStatement : Statement
    {
        public Expression Condition { get; }
        public Statement Body { get; }

        public WhileStatement(Expression condition, Statement body)
        {
            Condition = condition;
            Body = body;
        }
    }
}
