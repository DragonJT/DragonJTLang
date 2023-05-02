using System.Collections.Generic;
using System.Reflection;

enum ByteCode { Call, Add, Sub, Mul, Div, LT, MT, ConstF32, ConstBool, Ret, SetLocal, GetLocal, If, Goto, Yield }

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

class Call
{
    public MethodInfo method;
    public int paramCount;
}

static class GenerateAsm
{
    static Dictionary<string, int> locals = new Dictionary<string, int>();
    static Instruction breakInstruction;

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
            case NodeType.Yield:
                {
                    instructions.Add(new Instruction { type = ByteCode.Yield });
                    break;
                }
            case NodeType.Break:
                {
                    breakInstruction = new Instruction { type = ByteCode.Goto };
                    instructions.Add(breakInstruction);
                    break;
                }
            case NodeType.Assign:
                {
                    Generate(node.children[0], instructions);
                    instructions.Add(new Instruction { type = ByteCode.SetLocal, value = locals[node.token.text] });
                    break;
                }
            case NodeType.If:
                {
                    Generate(node.children[0], instructions);
                    var ifInstruction = new Instruction { type = ByteCode.If };
                    instructions.Add(ifInstruction);
                    foreach(var c in node.children[1].children)
                    {
                        Generate(c, instructions);
                    }
                    ifInstruction.value = instructions.Count;
                    break;
                }
            case NodeType.While:
                {
                    var start = instructions.Count;
                    Generate(node.children[0], instructions);
                    var ifInstruction = new Instruction { type = ByteCode.If };
                    instructions.Add(ifInstruction);
                    foreach (var c in node.children[1].children)
                    {
                        Generate(c, instructions);
                    }
                    instructions.Add(new Instruction { type = ByteCode.Goto, value = start });
                    ifInstruction.value = instructions.Count;
                    if (breakInstruction != null)
                    {
                        breakInstruction.value = instructions.Count;
                        breakInstruction = null;
                    }
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
                    foreach(var c in node.children)
                    {
                        Generate(c, instructions);
                    }
                    instructions.Add(new Instruction
                    {
                        type = ByteCode.Call,
                        value = new Call { method = typeof(CallableFunctions).GetMethod(node.token.text), paramCount = node.children.Count },
                    });
                    break;
                }
            case NodeType.Add: BinaryOp(node, instructions, ByteCode.Add); break;
            case NodeType.Sub: BinaryOp(node, instructions, ByteCode.Sub); break;
            case NodeType.Mul: BinaryOp(node, instructions, ByteCode.Mul); break;
            case NodeType.Div: BinaryOp(node, instructions, ByteCode.Div); break;
            case NodeType.LT: BinaryOp(node, instructions, ByteCode.LT); break;
            case NodeType.MT: BinaryOp(node, instructions, ByteCode.MT); break;
            case NodeType.True:
                {
                    instructions.Add(new Instruction { type = ByteCode.ConstBool, value = true });
                    break;
                }
            case NodeType.False:
                {
                    instructions.Add(new Instruction { type = ByteCode.ConstBool, value = false });
                    break;
                }
            case NodeType.Number:
                {
                    instructions.Add(new Instruction { type = ByteCode.ConstF32, value = float.Parse(node.token.text) });
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