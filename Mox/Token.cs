namespace Mox;

public readonly record struct Token(in TokenType Type, in string Lexeme, in object Literal, in int Line);
