using System.Collections.Generic;
using UnityEngine;

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
                        var func = (string)i.value;
                        switch (func)
                        {
                            case "Print": Game.Print(stack.Pop().ToString()); break;
                            default: throw new System.Exception("Unknown function");
                        }
                        index++;
                        break;
                    }
                case ByteCode.ConstI32:
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
                        int a = (int)stack.Pop();
                        int b = (int)stack.Pop();
                        stack.Push(b + a);
                        index++;
                        break;
                    }
                case ByteCode.Sub:
                    {
                        int a = (int)stack.Pop();
                        int b = (int)stack.Pop();
                        stack.Push(b - a);
                        index++;
                        break;
                    }
                case ByteCode.Mul:
                    {
                        int a = (int)stack.Pop();
                        int b = (int)stack.Pop();
                        stack.Push(b * a);
                        index++;
                        break;
                    }
                case ByteCode.Div:
                    {
                        int a = (int)stack.Pop();
                        int b = (int)stack.Pop();
                        stack.Push(b / a);
                        index++;
                        break;
                    }
                case ByteCode.LT:
                    {
                        int a = (int)stack.Pop();
                        int b = (int)stack.Pop();
                        stack.Push(b < a);
                        index++;
                        break;
                    }
                case ByteCode.MT:
                    {
                        int a = (int)stack.Pop();
                        int b = (int)stack.Pop();
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