using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 
 fun add(a, b) {
     return a + b;
 }
var result = add(5, 10);
print result;
 */
namespace CompilerLabs.Core.Lexer
{
    public class Lexer
    {
        private readonly string _input;
        private int _position;
        private int _line = 1;
        private int _column = 1;

        private static readonly Dictionary<string, TokenType> Keywords = new()
        {
            ["var"] = TokenType.VAR,
            ["print"] = TokenType.PRINT,
            ["if"] = TokenType.IF,
            ["else"] = TokenType.ELSE,
            ["while"] = TokenType.WHILE,
            ["fun"] = TokenType.FUNC,
            ["return"] = TokenType.RETURN
        };

        private static readonly Dictionary<string, TokenType> Operators = new()
        {
            ["=="] = TokenType.EQEQ,
            ["!="] = TokenType.NEQ,
            ["<="] = TokenType.LTEQ,
            [">="] = TokenType.GTEQ,
            ["&&"] = TokenType.AND,
            ["||"] = TokenType.OR,
            ["+"] = TokenType.PLUS,
            ["-"] = TokenType.MINUS,
            ["*"] = TokenType.STAR,
            ["/"] = TokenType.SLASH,
            ["="] = TokenType.EQ,
            ["<"] = TokenType.LT,
            [">"] = TokenType.GT,
            ["!"] = TokenType.EXCL,
            ["("] = TokenType.LPAREN,
            [")"] = TokenType.RPAREN,
            ["{"] = TokenType.LBRACE,
            ["}"] = TokenType.RBRACE,
            [";"] = TokenType.SEMICOLON,
            [","] = TokenType.COMMA
        };

        public Lexer(string input)
        {
            _input = input ?? "";
        }

        public IEnumerable<Token> Tokenize()
        {
            while (_position < _input.Length)
            {
                var current = Peek();

                if (char.IsWhiteSpace(current))
                {
                    Next();
                    continue;
                }

                if (char.IsDigit(current))
                {
                    yield return ReadNumber();
                    continue;
                }

                if (char.IsLetter(current))
                {
                    yield return ReadWord();
                    continue;
                }

                if (current == '"')
                {
                    yield return ReadString();
                    continue;
                }

                yield return ReadOperatorOrPunctuation();
            }

            yield return new Token(TokenType.EOF, "\0", _position, _line, _column);
        }

        private Token ReadString()
        {
            var startPos = _position;
            var startLine = _line;
            var startCol = _column;

            Next(); // Съедаем открывающую кавычку '"'

            var sb = new StringBuilder();
            while (Peek() != '"' && Peek() != '\0')
            {
                sb.Append(Next()); // Читаем всё подряд до следующей кавычки
            }

            if (Peek() == '\0')
            {
                throw new Exception($"[Lexer Error] Незакрытая строка (Unterminated string) начиная с Line {startLine}, Column {startCol}");
            }

            Next(); // Съедаем закрывающую кавычку '"'

            return new Token(TokenType.STRING, sb.ToString(), startPos, startLine, startCol);
        }

        private Token ReadNumber()
        {
            var startPos = _position;
            var startLine = _line;
            var startCol = _column;

            while (char.IsDigit(Peek())) 
                Next();

            var text = _input.Substring(startPos, _position - startPos);

            return new Token(TokenType.NUMBER, text, startPos, startLine, startCol);
        }

        private Token ReadWord()
        {
            var startPos = _position;
            var startLine = _line;
            var startCol = _column;

            while (char.IsLetterOrDigit(Peek())) 
                Next();

            var text = _input.Substring(startPos, _position - startPos);
            var type = Keywords.TryGetValue(text, out var kwType) ? kwType : TokenType.ID;

            return new Token(type, text, startPos, startLine, startCol);
        }

        private Token ReadOperatorOrPunctuation()
        {
            var startPos = _position;
            var startLine = _line;
            var startCol = _column;

            if (_position + 1 < _input.Length)
            {
                var twoChars = _input.Substring(_position, 2);

                //пробуем считать операторы вида ==, !=
                if (Operators.TryGetValue(twoChars, out var opType))
                {
                    Next(); 
                    Next();
                    
                    return new Token(opType, twoChars, startPos, startLine, startCol);
                }
            }

            var oneChar = _input[_position].ToString();
            if (Operators.TryGetValue(oneChar, out var type))
            {
                Next();
                return new Token(type, oneChar, startPos, startLine, startCol);
            }

            var badChar = Peek();

            throw new Exception($"[Lexer Error] Unexpected character '{badChar}' at Line {startLine}, Column {startCol}");
        }

        private char Peek() => _position >= _input.Length ? '\0' : _input[_position];

        private char Next()
        {
            if (_position >= _input.Length) return '\0';

            char current = _input[_position++];

            if (current == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }

            return current;
        }
    }
}
