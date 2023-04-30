
using System.Collections.Generic;
using UnityEngine;

enum TokenType { If, Var, Varname, Number, Add, Sub, Div, Mul, LT, MT, OpenParenthesis, CloseParenthesis, Equals }

class Token
{
    public TokenType type;
    public string text;
}

class CodeReader
{
    public string code;
    public int index;
}

static class Tokenizer
{
    static Token CreateToken(CodeReader reader, int start, TokenType type)
    {
         return new Token { type = type, text = reader.code.Substring(start, reader.index - start) };
    }

    static bool IsLetter(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
    }

    static bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    static bool IsLetterOrDigit(char c)
    {
        return IsLetter(c) || IsDigit(c);
    }

    static Token CreateVarnameToken(CodeReader reader, int start)
    {
        var text = reader.code.Substring(start, reader.index - start);
        switch (text)
        {
            case "if": return new Token { type = TokenType.If, text = text };
            case "var": return new Token { type = TokenType.Var, text = text };
        }
        return new Token { type = TokenType.Varname, text = text };
    }

    static Token GetVarname(CodeReader reader)
    {
        var start = reader.index;
        reader.index++;
        while (true)
        {
            if (reader.index >= reader.code.Length)
            {
                return CreateVarnameToken(reader, start);
            }
            if (!IsLetterOrDigit(reader.code[reader.index]))
            {
                return CreateVarnameToken(reader, start);
            }
            reader.index++;
        }
    }

    static Token GetNumber(CodeReader reader)
    {
        var start = reader.index;
        reader.index++;
        while (true)
        {
            if (reader.index >= reader.code.Length)
            {
                return CreateToken(reader, start, TokenType.Number);
            }
            if (!IsDigit(reader.code[reader.index]))
            {
                return CreateToken(reader, start, TokenType.Number);
            }
            reader.index++;
        }
    }

    static Token GetToken(CodeReader reader, int length, TokenType type)
    {
        var start = reader.index;
        reader.index += length;
        return CreateToken(reader, start, type);
    }

    static Token GetToken(CodeReader reader)
    {
    Start:
        if (reader.index >= reader.code.Length)
        {
            return null;
        }
        var c = reader.code[reader.index];
        if (IsLetter(c))
        {
            return GetVarname(reader);
        }
        if (IsDigit(c))
        {
            return GetNumber(reader);
        }
        switch (c)
        {
            case '+': return GetToken(reader, 1, TokenType.Add);
            case '*': return GetToken(reader, 1, TokenType.Mul);
            case '/': return GetToken(reader, 1, TokenType.Div);
            case '-': return GetToken(reader, 1, TokenType.Sub);
            case '(': return GetToken(reader, 1, TokenType.OpenParenthesis);
            case ')': return GetToken(reader, 1, TokenType.CloseParenthesis);
            case '=': return GetToken(reader, 1, TokenType.Equals);
            case '<': return GetToken(reader, 1, TokenType.LT);
            case '>': return GetToken(reader, 1, TokenType.MT);
            case ' ': reader.index++; goto Start;
            case '\t': reader.index++; goto Start;
            case '\r': reader.index++; goto Start;
            case '\n': reader.index++; goto Start;
        }
        throw new System.Exception("Unknown char: " + c);
    }

    public static List<Token> Tokenize(string code)
    {
        var reader = new CodeReader { code = code, index = 0 };
        List<Token> tokens = new List<Token>();
        while (true)
        {
            var token = GetToken(reader);
            if (token == null)
            {
                return tokens;
            }
            tokens.Add(token);
        }
    }
}