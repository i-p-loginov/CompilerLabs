namespace CompilerLabs.Core.Lexer
{
    public enum TokenType
    {
        NUMBER,
        ID,
        STRING,
        VAR,

        PRINT,
        IF, ELSE,
        WHILE,      // while
        FUNC, RETURN,
        // Operators
        PLUS, MINUS, STAR, SLASH,   // + - * /
        EQ, EQEQ, EXCL, NEQ,        // = == ! !=
        LT, GT, LTEQ, GTEQ,         // < > <= >=
        AND, OR,                    // && ||

        // Grouping & Punctuation
        LPAREN, RPAREN, // ( )
        LBRACE, RBRACE, // { }
        SEMICOLON,      // ;
        COMMA,
        EOF
    }
}