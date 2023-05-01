

using System.Collections.Generic;
using UnityEngine;

enum NodeType { Body, Varname, Number, Add, Sub, Div, Mul, LT, MT, Call, If, While, Assign, Var }

class Node
{
    public NodeType type;
    public Token token;
    public List<Node> children;
}

static class Parser
{
    static List<Token> GetTokensUntil(List<Token> tokens, int index, TokenType end)
    {
        var result = new List<Token>();
        while (true)
        {
            var t = tokens[index];
            if (t.type == end)
            {
                return result;
            }
            result.Add(t);
            index++;
        }
    }

    static int Precedence(TokenType type)
    {
        switch (type)
        {
            case TokenType.Add: return 5;
            case TokenType.Sub: return 5;
            case TokenType.Div: return 0;
            case TokenType.Mul: return 0;
            case TokenType.LT: return 10;
            case TokenType.MT: return 10;
        }
        throw new System.Exception("Not an operator");
    }

    static bool IsOperator(TokenType type)
    {
        switch (type)
        {
            case TokenType.Add: return true;
            case TokenType.Sub: return true;
            case TokenType.Mul: return true;
            case TokenType.Div: return true;
            case TokenType.LT: return true;
            case TokenType.MT: return true;
        }
        return false;
    }

    static NodeType GetValueNodeType(TokenType type)
    {
        switch (type)
        {
            case TokenType.Varname: return NodeType.Varname;
            case TokenType.Number: return NodeType.Number;
            default: throw new System.Exception("Not a value type");
        }
    }

    static NodeType GetOperatorNodeType(TokenType type)
    {
        switch (type)
        {
            case TokenType.Add: return NodeType.Add;
            case TokenType.Sub: return NodeType.Sub;
            case TokenType.Mul: return NodeType.Mul;
            case TokenType.Div: return NodeType.Div;
            case TokenType.LT: return NodeType.LT;
            case TokenType.MT: return NodeType.MT;
            default: throw new System.Exception("Not a operator type");
        }
    }

    static Node ExpressionToNodes(List<Token> tokens)
    {
        if (tokens.Count == 1)
        {
            return new Node { type = GetValueNodeType(tokens[0].type), token = tokens[0] };
        }
        var max = -1;
        var maxIndex = -1;
        for (var i = 0; i < tokens.Count; i++)
        {
            var t = tokens[i];
            if (IsOperator(t.type))
            {
                var p = Precedence(tokens[i].type);
                if (p >= max)
                {
                    maxIndex = i;
                    max = p;
                }
            }
        }
        if (maxIndex == -1)
        {
            foreach(var t in tokens)
            {
                Debug.Log(t.type);
            }
            throw new System.Exception("No operators found");
        }
        var b1 = ExpressionToNodes(tokens.GetRange(0, maxIndex));
        var b2 = ExpressionToNodes(tokens.GetRange(maxIndex + 1, tokens.Count - maxIndex - 1));
        var opNodeType = GetOperatorNodeType(tokens[maxIndex].type);
        return new Node { type = opNodeType, token = tokens[maxIndex], children = new List<Node> { b1, b2 } };

    }

    public static Node ParseLine(List<Token> tokens)
    {
        var t = tokens[0];
        if (t.type == TokenType.Varname)
        {
            if (tokens[1].type == TokenType.OpenParenthesis)
            {
                var expression = ExpressionToNodes(GetTokensUntil(tokens, 2, TokenType.CloseParenthesis));
                return new Node
                {
                    type = NodeType.Call,
                    token = t,
                    children = new List<Node> { expression }
                };
            }
            else if (tokens[1].type == TokenType.Equals)
            {
                var expression = ExpressionToNodes(tokens.GetRange(2, tokens.Count - 2));
                return new Node
                {
                    type = NodeType.Assign,
                    token = tokens[0],
                    children = new List<Node> { expression }
                };
            }
            throw new System.Exception("Error");
        }
        else if (t.type == TokenType.If)
        {
            if (tokens[1].type == TokenType.OpenParenthesis)
            {
                var expression = ExpressionToNodes(GetTokensUntil(tokens, 2, TokenType.CloseParenthesis));
                return new Node
                {
                    type = NodeType.If,
                    children = new List<Node> { expression, new Node { type = NodeType.Body, children = new List<Node>() } },
                };
            }
            throw new System.Exception("Error");
        }
        else if(t.type == TokenType.While)
        {
            if (tokens[1].type == TokenType.OpenParenthesis)
            {
                var expression = ExpressionToNodes(GetTokensUntil(tokens, 2, TokenType.CloseParenthesis));
                return new Node
                {
                    type = NodeType.While,
                    children = new List<Node> { expression, new Node { type = NodeType.Body, children = new List<Node>() } },
                };
            }
            throw new System.Exception("Error");
        }
        else if (t.type == TokenType.Var)
        {
            if (tokens[1].type == TokenType.Varname)
            {
                if (tokens[2].type == TokenType.Equals)
                {
                    var expression = ExpressionToNodes(tokens.GetRange(3, tokens.Count - 3));
                    return new Node
                    {
                        type = NodeType.Var,
                        token = tokens[1],
                        children = new List<Node>{ expression }
                    };
                }
            }
            throw new System.Exception("Error");
        }
        throw new System.Exception("Error");
    }
}