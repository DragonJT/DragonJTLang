using System.Collections.Generic;
using UnityEngine;

static class CallableFunctions
{
    public static void Print(object value)
    {
        Game.Print(value.ToString());
    }

    public static void DrawTriangle(float x, float y, float radius, float r, float g, float b)
    {
        Game.DrawTriangle(x, y, radius, new Color(r, g, b));
    }
}

static class VM
{
    static Stack<object> stack;
    static object[] locals;
    public static bool _continue;
    static int index;
    static Function function;

    public static void Run(Function function)
    {
        VM.function = function;
        stack = new Stack<object>();
        locals = new object[function.localCount];
        index = 0;
        _continue = true;
        Update();
    }

    public static void Update()
    {
        if (!_continue)
        {
            return;
        }
        while (true)
        {
            var i = function.instructions[index];
            switch (i.type)
            {
                case ByteCode.Yield:
                    {
                        _continue = true;
                        index++;
                        return;
                    }
                case ByteCode.Goto:
                    {
                        index = (int)i.value;
                        break;
                    }
                case ByteCode.If:
                    {
                        index = ((bool)stack.Pop()) ? index + 1 : (int)i.value;
                        break;
                    }
                case ByteCode.SetLocal:
                    {
                        locals[(int)i.value] = stack.Pop();
                        index++;
                        break;
                    }
                case ByteCode.GetLocal:
                    {
                        stack.Push(locals[(int)i.value]);
                        index++;
                        break;
                    }
                case ByteCode.Call:
                    {
                        var call = (Call)i.value;
                        var parameters = new object[call.paramCount];
                        for(var pi = call.paramCount - 1; pi >= 0; pi--)
                        {
                            parameters[pi] = stack.Pop();
                        }
                        call.method.Invoke(null, parameters);
                        index++;
                        break;
                    }
                case ByteCode.ConstF32:
                    {
                        stack.Push(i.value);
                        index++;
                        break;
                    }
                case ByteCode.ConstBool:
                    {
                        stack.Push(i.value);
                        index++;
                        break;
                    }
                case ByteCode.Add:
                    {
                        var a = (float)stack.Pop();
                        var b = (float)stack.Pop();
                        stack.Push(b + a);
                        index++;
                        break;
                    }
                case ByteCode.Sub:
                    {
                        var a = (float)stack.Pop();
                        var b = (float)stack.Pop();
                        stack.Push(b - a);
                        index++;
                        break;
                    }
                case ByteCode.Mul:
                    {
                        var a = (float)stack.Pop();
                        var b = (float)stack.Pop();
                        stack.Push(b * a);
                        index++;
                        break;
                    }
                case ByteCode.Div:
                    {
                        var a = (float)stack.Pop();
                        var b = (float)stack.Pop();
                        stack.Push(b / a);
                        index++;
                        break;
                    }
                case ByteCode.LT:
                    {
                        var a = (float)stack.Pop();
                        var b = (float)stack.Pop();
                        stack.Push(b < a);
                        index++;
                        break;
                    }
                case ByteCode.MT:
                    {
                        var a = (float)stack.Pop();
                        var b = (float)stack.Pop();
                        stack.Push(b > a);
                        index++;
                        break;
                    }
                case ByteCode.Ret:
                    {
                        _continue = false;
                        return;
                    }
                default: throw new System.Exception("Unknown instruction: " + i.type);
            }
        }
    }
}