namespace Mox;

public record Token(in TokenType Type, in string Lexeme, in object Literal, in int Line);
