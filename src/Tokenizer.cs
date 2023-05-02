
using System.Collections.Generic;

class CodeReader
{
    public string code;
    public int index;
}

static class Tokenizer
{
    static Node CreateToken(CodeReader reader, int start, NodeType type)
    {
         return new Node { type = type, text = reader.code.Substring(start, reader.index - start) };
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

    static bool IsDigitOrDot(char c)
    {
        return IsDigit(c) || c == '.';
    }

    static Node CreateVarnameToken(CodeReader reader, int start)
    {
        var text = reader.code.Substring(start, reader.index - start);
        switch (text)
        {
            case "if": return new Node { type = NodeType.If, text = text };
            case "while": return new Node { type = NodeType.While, text = text };
            case "var": return new Node { type = NodeType.Var, text = text };
            case "break": return new Node { type = NodeType.Break, text = text };
            case "true": return new Node { type = NodeType.True, text = text };
            case "false": return new Node { type = NodeType.False, text = text };
            case "yield": return new Node { type = NodeType.Yield, text = text };
        }
        return new Node { type = NodeType.Varname, text = text };
    }

    static Node GetVarname(CodeReader reader)
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

    static Node GetNumber(CodeReader reader)
    {
        var start = reader.index;
        reader.index++;
        while (true)
        {
            if (reader.index >= reader.code.Length)
            {
                return CreateToken(reader, start, NodeType.Number);
            }
            if (!IsDigitOrDot(reader.code[reader.index]))
            {
                return CreateToken(reader, start, NodeType.Number);
            }
            reader.index++;
        }
    }

    static Node GetToken(CodeReader reader, int length, NodeType type)
    {
        var start = reader.index;
        reader.index += length;
        return CreateToken(reader, start, type);
    }

    static Node GetToken(CodeReader reader)
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
            case '+': return GetToken(reader, 1, NodeType.Add);
            case '*': return GetToken(reader, 1, NodeType.Mul);
            case '/': return GetToken(reader, 1, NodeType.Div);
            case '-': return GetToken(reader, 1, NodeType.Sub);
            case '(': return GetToken(reader, 1, NodeType.OpenParenthesis);
            case ')': return GetToken(reader, 1, NodeType.CloseParenthesis);
            case '=': return GetToken(reader, 1, NodeType.Equals);
            case '<': return GetToken(reader, 1, NodeType.LT);
            case '>': return GetToken(reader, 1, NodeType.MT);
            case '{': return GetToken(reader, 1, NodeType.OpenCurly);
            case '}': return GetToken(reader, 1, NodeType.CloseCurly);
            case ',': return GetToken(reader, 1, NodeType.Comma);
            case ' ': reader.index++; goto Start;
            case '\t': reader.index++; goto Start;
            case '\r': reader.index++; goto Start;
            case '\n': reader.index++; goto Start;
        }
        throw new System.Exception("Unknown char: " + c);
    }

    public static List<Node> Tokenize(string code)
    {
        var reader = new CodeReader { code = code, index = 0 };
        List<Node> tokens = new List<Node>();
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