using System.Collections.Immutable;
using static Mox.TokenType;

namespace Mox;

public class Scanner
{
    private readonly string _source;
    private readonly List<Token> _tokens = [];
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    public Scanner(string source)
    {
        _source = source;
    }
    
    public ImmutableList<Token> ScanTokens() {
        while (!IsAtEnd()) {
            // We are at the beginning of the next lexeme.
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(EOF, string.Empty, null!, _line));
        return _tokens.ToImmutableList();
    }
    
    private void ScanToken() {
        var c = Advance();
        switch (c) {
            case '(': AddToken(LEFT_PAREN); break;
            case ')': AddToken(RIGHT_PAREN); break;
            case '{': AddToken(LEFT_BRACE); break;
            case '}': AddToken(RIGHT_BRACE); break;
            case ',': AddToken(COMMA); break;
            case '.': AddToken(DOT); break;
            case '-': AddToken(MINUS); break;
            case '+': AddToken(PLUS); break;
            case ';': AddToken(SEMICOLON); break;
            case '*': AddToken(STAR); break; 
            
            case '!':
                AddToken(Match('=') ? BANG_EQUAL : BANG);
                break;
            case '=':
                AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? LESS_EQUAL : LESS);
                break;
            case '>':
                AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                break;
            
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                _line++;
                break;
            
            case '/':
                if (Match('/')) {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd())
                    {
                        Advance();
                    }
                } else {
                    AddToken(SLASH);
                }
                break;
            
            
            case '"': GetString(); break;
            
            default: 
                Mox.Error(_line, "Unexpected character.");
                break;
        }
    }
    
    private void AddToken(TokenType type, object literal = null!) {
        var text = _source.Substring(_start, _current);
        _tokens.Add(new Token(type, text, literal, _line));
    }
    
    private void GetString() 
    {
        while (Peek() != '"' && !IsAtEnd()) {
            if (Peek() == '\n') _line++;
            {
                Advance();
            }
        }

        if (IsAtEnd()) {
            Mox.Error(_line, "Unterminated string.");
            return;
        }

        // The closing ".
        Advance();

        // Trim the surrounding quotes.
        var value = _source.Substring(_start + 1, _current - 1);
        AddToken(STRING, value);
    }
    
    private bool Match(char expected) {
        if (IsAtEnd())
        {
            return false;
        }
        if (_source[_current] != expected)
        {
            return false;
        }

        _current++;
        return true;
    }
    
    private char Peek() => IsAtEnd() ? '\0' : _source[_current];
    
    private char Advance() => _source[_current++];
    
    private bool IsAtEnd() => _current >= _source.Length;
}