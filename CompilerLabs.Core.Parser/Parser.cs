using CompilerLabs.Core.Lexer;
using CompilerLabs.Core.Parser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerLabs.Core.Parser
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _position;

        public Parser(IEnumerable<Token> tokens)
        {
            _tokens = tokens.ToList();
            _position = 0;
        }

        public List<Statement> Parse()
        {
            var statements = new List<Statement>();
            while (!IsAtEnd())
            {
                statements.Add(ParseDeclaration());
            }
            return statements;
        }

        private Statement ParseDeclaration()
        {
            if (Match(TokenType.VAR)) return ParseVarDeclaration();

            return ParseStatement();
        }

        private Statement ParseStatement()
        {
            if (Match(TokenType.IF)) return ParseIfStatement();
            if (Match(TokenType.WHILE)) return ParseWhileStatement();
            if (Match(TokenType.PRINT)) return ParsePrintStatement();
            if (Match(TokenType.LBRACE)) return new BlockStatement(ParseBlock());

            return ParseExpressionStatement();
        }

        private Statement ParseVarDeclaration()
        {
            Token name = Consume(TokenType.ID, "Ожидается имя переменной.");
            Expression initializer = null;

            if (Match(TokenType.EQ))
            {
                initializer = ParseExpression();
            }

            Consume(TokenType.SEMICOLON, "Ожидается ';' после объявления переменной.");
            return new VarStatement(name.Value, initializer);
        }

        private Statement ParseIfStatement()
        {
            Consume(TokenType.LPAREN, "Ожидается '(' после 'if'.");
            Expression condition = ParseExpression();
            Consume(TokenType.RPAREN, "Ожидается ')' после условия 'if'.");

            Statement thenBranch = ParseStatement();
            Statement elseBranch = null;

            if (Match(TokenType.ELSE))
            {
                elseBranch = ParseStatement();
            }

            return new IfStatement(condition, thenBranch, elseBranch);
        }

        private Statement ParseWhileStatement()
        {
            Consume(TokenType.LPAREN, "Ожидается '(' после 'while'.");
            Expression condition = ParseExpression();
            Consume(TokenType.RPAREN, "Ожидается ')' после условия 'while'.");

            Statement body = ParseStatement();
            return new WhileStatement(condition, body);
        }

        private Statement ParsePrintStatement()
        {
            Expression value = ParseExpression();
            Consume(TokenType.SEMICOLON, "Ожидается ';' после значения.");
            return new PrintStatement(value);
        }

        private Statement ParseExpressionStatement()
        {
            Expression expr = ParseExpression();
            Consume(TokenType.SEMICOLON, "Ожидается ';' после выражения.");
            return new ExpressionStatement(expr);
        }

        private List<Statement> ParseBlock()
        {
            var statements = new List<Statement>();

            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                statements.Add(ParseDeclaration());
            }

            Consume(TokenType.RBRACE, "Ожидается '}' после блока.");
            return statements;
        }

        private Expression ParseExpression()
        {
            return ParseAssignment();
        }

        // 1. Присваивание (самый низкий приоритет)
        private Expression ParseAssignment()
        {
            // Сначала парсим левую часть так, как будто это обычное логическое выражение
            Expression expr = ParseLogicalOr();

            if (Match(TokenType.EQ))
            {
                Token equals = Previous();
                Expression value = ParseAssignment(); // Рекурсия для a = b = 5

                if (expr is VariableExpression varExpr)
                {
                    return new AssignExpression(varExpr.Name, value);
                }

                throw new Exception($"[Parser Error] Line {equals.Line}: Недопустимая цель для присваивания.");
            }

            return expr;
        }

        // 2. Логическое ИЛИ (||)
        private Expression ParseLogicalOr()
        {
            Expression expr = ParseLogicalAnd();

            while (Match(TokenType.OR))
            {
                TokenType op = Previous().Type;
                Expression right = ParseLogicalAnd();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        // 3. Логическое И (&&)
        private Expression ParseLogicalAnd()
        {
            Expression expr = ParseEquality();

            while (Match(TokenType.AND))
            {
                TokenType op = Previous().Type;
                Expression right = ParseEquality();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        // 4. Сравнения на равенство (==, !=)
        private Expression ParseEquality()
        {
            Expression expr = ParseComparison();

            while (Match(TokenType.EQEQ, TokenType.NEQ))
            {
                TokenType op = Previous().Type;
                Expression right = ParseComparison();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        // 5. Меньше, больше (<, >, <=, >=)
        private Expression ParseComparison()
        {
            Expression expr = ParseTerm();

            while (Match(TokenType.LT, TokenType.LTEQ, TokenType.GT, TokenType.GTEQ))
            {
                TokenType op = Previous().Type;
                Expression right = ParseTerm();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        // 6. Сложение и вычитание (+, -)
        private Expression ParseTerm()
        {
            Expression expr = ParseFactor();

            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                TokenType op = Previous().Type;
                Expression right = ParseFactor();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        // 7. Умножение и деление (*, /)
        private Expression ParseFactor()
        {
            Expression expr = ParseUnary();

            while (Match(TokenType.STAR, TokenType.SLASH))
            {
                TokenType op = Previous().Type;
                Expression right = ParseUnary();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        // 8. Унарные операции (!, -)
        private Expression ParseUnary()
        {
            if (Match(TokenType.EXCL, TokenType.MINUS))
            {
                TokenType op = Previous().Type;
                Expression right = ParseUnary();
                return new UnaryExpression(op, right);
            }

            return ParsePrimary();
        }

        // 9. Примитивы (Числа, Переменные, Скобки) - наивысший приоритет
        private Expression ParsePrimary()
        {
            if (Match(TokenType.NUMBER))
            {
                double value = double.Parse(Previous().Value, System.Globalization.CultureInfo.InvariantCulture);
                return new NumberExpression(value);
            }

            if (Match(TokenType.ID))
            {
                return new VariableExpression(Previous().Value);
            }

            if (Match(TokenType.LPAREN))
            {
                Expression expr = ParseExpression();
                Consume(TokenType.RPAREN, "Ожидается ')' после выражения.");
                return expr;
            }

            throw new Exception($"[Parser Error] Line {Peek().Line}, Col {Peek().Column}: Ожидается выражение.");
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _position++;
            return Previous();
        }

        private bool IsAtEnd() => Peek().Type == TokenType.EOF;
        private Token Peek() => _tokens[_position];
        private Token Previous() => _tokens[_position - 1];

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            Token token = Peek();
            throw new Exception($"[Parser Error] Line {token.Line}, Col {token.Column}: {message}");
        }
    }
}
