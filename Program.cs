using System;
using System.Collections.Generic;

enum TokenType
{
    Keyword,
    Identifier,
    Operator,
    NumericConstant,
    CharacterConstant,
    StringConstant,
    SpecialCharacter,
    Comment,
    Whitespace,
    Newline,
    Unknown
}

class Token
{
    public TokenType Type { get; }
    public string Value { get; }

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString()
    {
        return $"{Type}: {Value}";
    }
}

class LexicalAnalyzer
{
    private string _input;
    private int _position;
    private bool _lastWasDataType = false;

    static readonly string[] KeywordsAndDataTypes = {
        "if", "else", "while", "for", "return", "break", "continue",
        "string", "int", "float", "double", "char", "void"
    };

    static readonly char[] Operators = { '+', '-', '*', '/', '=', '<', '>', '&', '|', '!', '%' };
    static readonly char[] SpecialCharacters = { '(', ')', '{', '}', '[', ']', ';', ',', ':' };

    public LexicalAnalyzer(string input)
    {
        _input = input;
        _position = 0;
    }

    public List<Token> Analyze()
    {
        var tokens = new List<Token>();

        while (_position < _input.Length)
        {
            char current = _input[_position];

            if (char.IsWhiteSpace(current))
            {
                if (current == '\n')
                {
                    tokens.Add(new Token(TokenType.Newline, "\\n"));
                }
                else
                {
                    tokens.Add(new Token(TokenType.Whitespace, " "));
                }
                _position++;
            }
            else if (char.IsLetter(current) || current == '_')
            {
                tokens.Add(ReadIdentifierOrKeyword());
            }
            else if (char.IsDigit(current))
            {
                tokens.Add(ReadNumericConstant());
            }
            else if (Operators.Contains(current))
            {
                tokens.Add(ReadOperator());
            }
            else if (SpecialCharacters.Contains(current))
            {
                tokens.Add(new Token(TokenType.SpecialCharacter, current.ToString()));
                _position++;
            }
            else if (current == '\'')
            {
                tokens.Add(ReadCharacterConstant());
            }
            else if (current == '\"')
            {
                tokens.Add(ReadStringConstant());
            }
            else if (current == '/')
            {
                Token commentToken = ReadComment();
                if (commentToken != null)
                {
                    tokens.Add(commentToken);
                }
                else
                {
                    tokens.Add(new Token(TokenType.Unknown, current.ToString()));
                    _position++;
                }
            }
            else
            {
                tokens.Add(new Token(TokenType.Unknown, current.ToString()));
                _position++;
            }
        }
        return tokens;
    }

    private Token ReadIdentifierOrKeyword()
    {
        int start = _position;
        while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
            _position++;

        string value = _input.Substring(start, _position - start);

        if (Array.Exists(KeywordsAndDataTypes, keyword => keyword == value))
        {
            _lastWasDataType = true;
            return new Token(TokenType.Keyword, value);
        }
        else
        {
            if (_lastWasDataType)
            {
                _lastWasDataType = false;
                return new Token(TokenType.Identifier, value);
            }
            else
            {
                return new Token(TokenType.Unknown, value);
            }
        }
    }

    private Token ReadNumericConstant()
    {
        int start = _position;
        while (_position < _input.Length && (char.IsDigit(_input[_position]) || _input[_position] == '.' || _input[_position] == 'e' || _input[_position] == 'E'))
            _position++;

        return new Token(TokenType.NumericConstant, _input.Substring(start, _position - start));
    }

    private Token ReadOperator()
    {
        char op = _input[_position];
        _position++;
        return new Token(TokenType.Operator, op.ToString());
    }

    private Token ReadCharacterConstant()
    {
        int start = _position++;
        if (_position < _input.Length && _input[_position] != '\'')
        {
            _position++;
            if (_position < _input.Length && _input[_position] == '\'')
            {
                _position++;
            }
        }
        return new Token(TokenType.CharacterConstant, _input.Substring(start, _position - start));
    }

    private Token ReadStringConstant()
    {
        int start = _position++;
        while (_position < _input.Length && _input[_position] != '\"')
        {
            _position++;
        }

        if (_position < _input.Length && _input[_position] == '\"')
        {
            _position++;
        }

        return new Token(TokenType.StringConstant, _input.Substring(start, _position - start));
    }

    private Token ReadComment()
    {
        int start = _position;

        // Check for single-line comment
        if (_position + 1 < _input.Length && _input[_position + 1] == '/')
        {
            _position += 2; // Skip "//"
            while (_position < _input.Length && _input[_position] != '\n') // Read until end of line
            {
                _position++;
            }
            // Capture the entire comment as a single token
            return new Token(TokenType.Comment, _input.Substring(start, _position - start).TrimEnd());
        }
        // Check for multi-line comment
        else if (_position + 1 < _input.Length && _input[_position + 1] == '*')
        {
            _position += 2; // Skip "/*"
            while (_position + 1 < _input.Length && !(_input[_position] == '*' && _input[_position + 1] == '/')) // Read until "*/"
            {
                _position++;
            }
            if (_position + 1 < _input.Length) // Skip "*/"
            {
                _position += 2;
            }
            return new Token(TokenType.Comment, _input.Substring(start, _position - start).TrimEnd());
        }

        return null; // If no valid comment is found
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Enter C code to analyze (type 'SCAN' on a new line to finish input):");

        string code = "";
        string line;
        while ((line = Console.ReadLine()) != "SCAN")
        {
            code += line + "\n";
        }

        var analyzer = new LexicalAnalyzer(code);
        var tokens = analyzer.Analyze();

        Console.WriteLine("\nTokens:");
        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }
}
