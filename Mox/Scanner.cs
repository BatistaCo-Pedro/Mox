using System.Collections.Frozen;
using System.Collections.Immutable;
using static Mox.TokenType;

namespace Mox;

public class Scanner
{
    private static readonly FrozenDictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
    {
        { "and", AND },
        { "class", CLASS },
        { "else", ELSE },
        { "false", FALSE },
        { "for", FOR },
        { "fun", FUN },
        { "if", IF },
        { "null", NULL },
        { "or", OR },
        { "print", PRINT },
        { "return", RETURN },
        { "base", BASE },
        { "this", THIS },
        { "true", TRUE },
        { "var", VAR },
        { "while", WHILE }
    }.ToFrozenDictionary();
    
    private readonly string _source;
    private readonly List<Token> _tokens = [];
    private int _start;
    private int _current;
    private int _line = 1;

    public Scanner(string source)
    {
        _source = source;
    }
    
    public ImmutableList<Token> ScanTokens() {
        while (!IsAtEnd()) {
            // The beginning of the next lexeme.
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
                   GetSlashComment();
                } else if (Match('*')) {
                    GetSlashStarComment();
                } else {
                    AddToken(SLASH);
                }
                break;
            
            
            case '"': GetString(); break;
            
            default:
                if (IsDigit(c))
                {
                    GetNumber();
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    Mox.Error(_line, _current, "Unexpected character.");
                }
                break;
        }
    }

    private void AddToken(TokenType type, object literal = null!) {
        var text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private void GetSlashComment()
    {
        // A comment goes until the end of the line.
        while (Peek() != '\n' && !IsAtEnd())
        {
            Advance();
        }
    }
    
    private void GetSlashStarComment()
    {
        var nextChar = Peek();
        while (!(nextChar == '*' && PeekNext() == '\\') && !IsAtEnd())
        {
            if (nextChar == '\n')
            {
                _line++;
            }
            
            Advance();
        }
    }
    
    private void GetNumber()
    {
        while (IsDigit(Peek()))
        {
            Advance();
        }

        // Look for a fractional part.
        if (Peek() == '.' && IsDigit(PeekNext())) {
            // Consume the "."
            Advance();

            while (IsDigit(Peek()))
            {
                Advance();
            }
        }

        AddToken(NUMBER,
            float.Parse(_source.Substring(_start, _current)));
    }
    
    private void GetString() 
    {
        while (Peek() != '"' && !IsAtEnd()) {
            Advance();
        }

        if (IsAtEnd()) {
            Mox.Error(_line, _current, "Unterminated string.");
            return;
        }

        // The closing ".
        Advance();

        // Trim the surrounding quotes.
        var value = _source.Substring(_start + 1, _current - _start - 1);
        AddToken(STRING, value);
    }
    
    private static bool IsDigit(char c) => c is >= '0' and <= '9';
    private static bool IsAlpha(char c) =>  c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    private static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);
    
    private void Identifier()
    {
        while (IsAlphaNumeric(Peek()))
        {
            Advance();
        }
        
        var text = _source.Substring(_start, _current - _start);
        var gotValue = Keywords.TryGetValue(text, out var type);
        
        // If the identifier is not a keyword, it must be an identifier.
        if (!gotValue)
        {
            type = IDENTIFIER;
        }
        
        AddToken(type);
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
    
    private char PeekNext() => _current + 1 >= _source.Length ? '\0' : _source[_current + 1];
    
    private char Advance() => _source[_current++];
    
    private bool IsAtEnd() => _current >= _source.Length;
}