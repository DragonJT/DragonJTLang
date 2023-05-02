

using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

enum NodeType
{
    If, While, Var, Varname, Number, Add, Sub, Div, Mul, LT, MT, OpenParenthesis, CloseParenthesis, Equals, OpenCurly, CloseCurly,
    Break, True, False, Yield, Comma, Body, Call, Assign, ExpressionCall,
}

class Node
{
    public NodeType type;
    public string text;
    public List<Node> children;
}

static class Parser
{
    static List<Node> GetTokensUntil(List<Node> tokens, int index, NodeType end)
    {
        var result = new List<Node>();
        var depth = 0;
        while (true)
        {
            var t = tokens[index];
            if (t.type == end && depth == 0)
            {
                return result;
            }
            else if (t.type == NodeType.OpenParenthesis)
            {
                depth++;
            }
            else if(t.type == NodeType.CloseParenthesis)
            {
                depth--;
            }
            result.Add(t);
            index++;
        }
    }

    static int Precedence(NodeType type)
    {
        switch (type)
        {
            case NodeType.Add: return 5;
            case NodeType.Sub: return 5;
            case NodeType.Div: return 0;
            case NodeType.Mul: return 0;
            case NodeType.LT: return 10;
            case NodeType.MT: return 10;
        }
        throw new System.Exception("Not an operator");
    }

    static bool IsOperator(NodeType type)
    {
        switch (type)
        {
            case NodeType.Add: return true;
            case NodeType.Sub: return true;
            case NodeType.Mul: return true;
            case NodeType.Div: return true;
            case NodeType.LT: return true;
            case NodeType.MT: return true;
        }
        return false;
    }

    static Node FindBinaryOps(List<Node> tokens)
    {
        if (tokens.Count == 1)
        {
            return tokens[0];
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
            foreach(var n in tokens)
            {
                Debug.Log(n.type + "_" + n.text);
            }
            throw new System.Exception("No operators found");
        }
        var b1 = FindBinaryOps(tokens.GetRange(0, maxIndex));
        var b2 = FindBinaryOps(tokens.GetRange(maxIndex + 1, tokens.Count - maxIndex - 1));
        tokens[maxIndex].children = new List<Node> { b1, b2 };
        return tokens[maxIndex];

    }

    static Node ExpressionToNodes(List<Node> tokens)
    {
        var tokens2 = new List<Node>();
        for(var i = 0; i < tokens.Count; i++)
        {
            if (i+1<tokens.Count && tokens[i].type == NodeType.Varname && tokens[i+1].type == NodeType.OpenParenthesis)
            {
                var paramTokens = GetTokensUntil(tokens, i + 2, NodeType.CloseParenthesis);
                var parameters = GetParams(paramTokens);
                tokens2.Add(new Node
                {
                    type = NodeType.ExpressionCall,
                    text = tokens[i].text,
                    children = parameters,
                });
                i += paramTokens.Count + 2;
            }
            else
            {
                tokens2.Add(tokens[i]);
            }
        }
        return FindBinaryOps(tokens2);
    }

    static List<Node> GetParams(List<Node> tokens)
    {
        var parameters = new List<Node>();
        if(tokens.Count == 0)
        {
            return parameters;
        }
        List<Node> split = new List<Node>();
        foreach(var t in tokens)
        {
            if (t.type == NodeType.Comma)
            {
                parameters.Add(ExpressionToNodes(split));
                split.Clear();
            }
            else
            {
                split.Add(t);
            }
        }
        if (split.Count > 0)
        {
            parameters.Add(ExpressionToNodes(split));
        }
        return parameters;
    }

    public static Node ParseLine(List<Node> tokens)
    {
        var t = tokens[0];
        if (t.type == NodeType.Varname)
        {
            if (tokens[1].type == NodeType.OpenParenthesis)
            {
                var parameters = GetParams(GetTokensUntil(tokens, 2, NodeType.CloseParenthesis));
                return new Node
                {
                    type = NodeType.Call,
                    text = t.text,
                    children = parameters,
                };
            }
            else if (tokens[1].type == NodeType.Equals)
            {
                var expression = ExpressionToNodes(tokens.GetRange(2, tokens.Count - 2));
                return new Node
                {
                    type = NodeType.Assign,
                    text = tokens[0].text,
                    children = new List<Node> { expression }
                };
            }
            throw new System.Exception("Error");
        }
        else if (t.type == NodeType.If)
        {
            if (tokens[1].type == NodeType.OpenParenthesis)
            {
                var expression = ExpressionToNodes(GetTokensUntil(tokens, 2, NodeType.CloseParenthesis));
                return new Node
                {
                    type = NodeType.If,
                    children = new List<Node> { expression, new Node { type = NodeType.Body, children = new List<Node>() } },
                };
            }
            throw new System.Exception("Error");
        }
        else if(t.type == NodeType.While)
        {
            if (tokens[1].type == NodeType.OpenParenthesis)
            {
                var expression = ExpressionToNodes(GetTokensUntil(tokens, 2, NodeType.CloseParenthesis));
                return new Node
                {
                    type = NodeType.While,
                    children = new List<Node> { expression, new Node { type = NodeType.Body, children = new List<Node>() } },
                };
            }
            throw new System.Exception("Error");
        }
        else if (t.type == NodeType.Var)
        {
            if (tokens[1].type == NodeType.Varname)
            {
                if (tokens[2].type == NodeType.Equals)
                {
                    var expression = ExpressionToNodes(tokens.GetRange(3, tokens.Count - 3));
                    return new Node
                    {
                        type = NodeType.Var,
                        text = tokens[1].text,
                        children = new List<Node>{ expression }
                    };
                }
            }
            throw new System.Exception("Error");
        }
        else if(t.type == NodeType.Break)
        {
            return new Node { type = NodeType.Break };
        }
        else if(t.type == NodeType.Yield)
        {
            return new Node { type = NodeType.Yield };
        }
        throw new System.Exception("Error");
    }
}