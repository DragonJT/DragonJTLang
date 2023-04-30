﻿using System.Collections.Generic;

enum ByteCode { Call, Add, Sub, Mul, Div, LT, MT, Const, Ret, SetLocal, GetLocal, IfBr }

class Instruction
{
    public ByteCode type;
    public object value;
}

class Function
{
    public List<Instruction> instructions;
    public int localCount;
}

static class GenerateAsm
{
    static Dictionary<string, int> locals = new Dictionary<string, int>();

    static void BinaryOp(Node node, List<Instruction> instructions, ByteCode type)
    {
        Generate(node.children[0], instructions);
        Generate(node.children[1], instructions);
        instructions.Add(new Instruction { type = type });
    }

    static void Generate(Node node, List<Instruction> instructions)
    {
        switch (node.type)
        {
            case NodeType.If:
                {
                    Generate(node.children[0], instructions);
                    var ifInstruction = new Instruction { type = ByteCode.IfBr };
                    instructions.Add(ifInstruction);
                    foreach(var c in node.children[1].children)
                    {
                        Generate(c, instructions);
                    }
                    ifInstruction.value = instructions.Count;
                    break;
                }
            case NodeType.Var:
                {
                    Generate(node.children[0], instructions);
                    locals.Add(node.token.text, locals.Count);
                    instructions.Add(new Instruction { type = ByteCode.SetLocal, value = locals[node.token.text] });
                    break;
                }
            case NodeType.Call:
                {
                    Generate(node.children[0], instructions);
                    instructions.Add(new Instruction { type = ByteCode.Call, value = node.token.text });
                    break;
                }
            case NodeType.Add: BinaryOp(node, instructions, ByteCode.Add); break;
            case NodeType.Sub: BinaryOp(node, instructions, ByteCode.Sub); break;
            case NodeType.Mul: BinaryOp(node, instructions, ByteCode.Mul); break;
            case NodeType.Div: BinaryOp(node, instructions, ByteCode.Div); break;
            case NodeType.LT: BinaryOp(node, instructions, ByteCode.LT); break;
            case NodeType.MT: BinaryOp(node, instructions, ByteCode.MT); break;
            case NodeType.Number:
                {
                    instructions.Add(new Instruction { type = ByteCode.Const, value = int.Parse(node.token.text) });
                    break;
                }
            case NodeType.Varname:
                {
                    instructions.Add(new Instruction { type = ByteCode.GetLocal, value = locals[node.token.text] });
                    break;
                }
            default: throw new System.Exception("Unexpected type: " + node.type);
        }
    }

    public static Function Generate(Node node)
    {
        locals.Clear();
        var instructions = new List<Instruction>();
        foreach(var c in node.children)
        {
            Generate(c, instructions);
        }
        instructions.Add(new Instruction { type = ByteCode.Ret });
        /*
        foreach(var i in instructions)
        {
            UnityEngine.Debug.Log(i.type + "_" + i.value);
        }
        */
        return new Function { instructions = instructions, localCount = locals.Count };
    }
}