using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class CodeEditor
{
    static Node baseNode;
    static string codeLine = "";
    static int line = 0;
    static List<FlatHierarchy> flattenHierarchy = new List<FlatHierarchy>();
    static bool firstFrame;

    class FlatHierarchy
    {
        public Node node;
        public int index;
    }

    class DrawGUI
    {
        public float x;
        public float y;
        public GUIStyle style;
        public float lineSize;
        public float borderSize;
        public float indentSize;
    }

    static Texture2D CreateTex(Color color)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    static bool HasBody(NodeType type)
    {
        switch (type)
        {
            case NodeType.If: return true;
            case NodeType.While: return true;
        }
        return false;
    }

    static CodeEditor()
    {
        baseNode = new Node { type = NodeType.Body, children = new List<Node>() };
        FlattenHierarchy(baseNode, flattenHierarchy);
    }

    static float Wrap(float value)
    {
        while (value > 1)
        {
            value -= 1;
        }
        return value;
    }

    static void Draw(DrawGUI drawGUI, string text, int depth)
    {
        var width = drawGUI.style.CalcSize(new GUIContent(text)).x + drawGUI.borderSize;
        var rect = new Rect(drawGUI.x, drawGUI.y, width, drawGUI.lineSize);
        drawGUI.x += width;
        var t = depth / 20f;
        var c = Color.Lerp(Color.black, new Color(1, 0.5f, 0), t);
        GUI.color = c;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = new Color(0, Wrap(c.g+0.5f), 1);
        GUI.Label(rect, text, drawGUI.style);
    }

    static void DrawBinaryOp(DrawGUI drawGUI, Node node, int depth)
    {
        DrawExpression(drawGUI, node.children[0], depth+1);
        Draw(drawGUI, node.text, depth);
        DrawExpression(drawGUI, node.children[1], depth+1);
    }

    static void DrawValue(DrawGUI drawGUI, Node node, int depth)
    {
        Draw(drawGUI, node.text, depth);
    }

    static void DrawExpression(DrawGUI drawGUI, Node node, int depth)
    {
        switch (node.type)
        {
            case NodeType.True: Draw(drawGUI, "true", depth + 1); return;
            case NodeType.False: Draw(drawGUI, "false", depth + 1); return;
            case NodeType.Number: DrawValue(drawGUI, node, depth + 1); return;
            case NodeType.Varname: DrawValue(drawGUI, node, depth + 1); return;
            case NodeType.Add: DrawBinaryOp(drawGUI, node, depth + 1); return;
            case NodeType.Sub: DrawBinaryOp(drawGUI, node, depth + 1); return;
            case NodeType.Div: DrawBinaryOp(drawGUI, node, depth + 1); return;
            case NodeType.Mul: DrawBinaryOp(drawGUI, node, depth + 1); return;
            case NodeType.LT: DrawBinaryOp(drawGUI, node, depth + 1); return;
            case NodeType.MT: DrawBinaryOp(drawGUI, node, depth + 1); return;
            case NodeType.ExpressionCall:
                {
                    Draw(drawGUI, node.text, depth + 1);
                    Draw(drawGUI, "(", depth);
                    for (var i = 0; i < node.children.Count; i++)
                    {
                        DrawExpression(drawGUI, node.children[i], depth + 1);
                        if (i < node.children.Count - 1)
                        {
                            Draw(drawGUI, ",", depth);
                        }
                    }
                    Draw(drawGUI, ")", depth);
                    return;
                }
        }
        throw new System.Exception("Unexpected node type: " + node.type);
    }

    static void DrawBody(DrawGUI drawGUI, Node parent, int depth)
    {
        foreach (var c in parent.children)
        {
            drawGUI.x = depth*drawGUI.indentSize;
            if (c.type == NodeType.Call)
            {
                Draw(drawGUI, c.text, depth + 1);
                Draw(drawGUI, "(", depth);
                for(var i = 0; i < c.children.Count; i++)
                {
                    DrawExpression(drawGUI, c.children[i], depth + 1);
                    if (i < c.children.Count - 1)
                    {
                        Draw(drawGUI, ",", depth);
                    }
                }
                Draw(drawGUI, ")", depth);
            }
            else if (c.type == NodeType.If)
            {
                Draw(drawGUI, "if", depth);
                Draw(drawGUI, "(", depth);
                DrawExpression(drawGUI, c.children[0], depth + 1);
                Draw(drawGUI, ")", depth);
                Draw(drawGUI, "{", depth);
                drawGUI.y += drawGUI.lineSize;
                DrawBody(drawGUI, c.children[1], depth + 1);
                drawGUI.x = depth * drawGUI.indentSize;
                Draw(drawGUI, "}", depth);
            }
            else if (c.type == NodeType.While)
            {
                Draw(drawGUI, "while", depth);
                Draw(drawGUI, "(", depth);
                DrawExpression(drawGUI, c.children[0], depth + 1);
                Draw(drawGUI, ")", depth);
                Draw(drawGUI, "{", depth);
                drawGUI.y += drawGUI.lineSize;
                DrawBody(drawGUI, c.children[1], depth + 1);
                drawGUI.x = depth * drawGUI.indentSize;
                Draw(drawGUI, "}", depth);
            }
            else if(c.type == NodeType.Var)
            {
                Draw(drawGUI, "var", depth);
                Draw(drawGUI, c.text, depth + 1);
                Draw(drawGUI, "=", depth);
                DrawExpression(drawGUI, c.children[0], depth + 1);
            }
            else if(c.type == NodeType.Assign)
            {
                Draw(drawGUI, c.text, depth + 1);
                Draw(drawGUI, "=", depth);
                DrawExpression(drawGUI, c.children[0], depth + 1);
            }
            else if(c.type == NodeType.Break)
            {
                Draw(drawGUI, "break", depth);
            }
            else if (c.type == NodeType.Yield)
            {
                Draw(drawGUI, "yield", depth);
            }
            drawGUI.y += drawGUI.lineSize;
        }
    }

    static void FlattenHierarchy(Node parent, List<FlatHierarchy> flattenHierarchy, bool first = true)
    {
        if (first)
        {
            flattenHierarchy.Clear();
        }
        flattenHierarchy.Add(new FlatHierarchy { node = parent, index = 0 });
        for (var i = 0; i < parent.children.Count; i++)
        {
            var c = parent.children[i];
            if(HasBody(c.type))
            {
                FlattenHierarchy(c.children[1], flattenHierarchy, false);
            }
            flattenHierarchy.Add(new FlatHierarchy { node = parent, index = i + 1 });
        }
    }

    static void CountHierarchy(Node parent, ref int count)
    {
        count++;
        for (var i = 0; i < parent.children.Count; i++)
        {
            var c = parent.children[i];
            if (HasBody(c.type))
            {
                CountHierarchy(c.children[1], ref count);
            }
            count++;
        }
    }

    public static void SetExample(string code)
    {
        baseNode = new Node { type = NodeType.Body, children = new List<Node>() };
        Stack<Node> nodes = new Stack<Node>();
        nodes.Push(baseNode);
        foreach (var l in code.Split('\n'))
        {
            var tokens = Tokenizer.Tokenize(l);
            if (tokens[0].type == NodeType.CloseCurly)
            {
                nodes.Pop();
            }
            else
            {
                var node = Parser.ParseLine(tokens);
                nodes.Peek().children.Add(node);
                if (HasBody(node.type))
                {
                    nodes.Push(node.children[1]);
                }
            }
        }
        FlattenHierarchy(baseNode, flattenHierarchy);
        Begin();
    }

    public static void Begin()
    {
        Main.gameState = GameState.CodeEditor;
        firstFrame = true;
    }

    public static void OnGUI()
    {
        var evt = Event.current;
        if (evt.type == EventType.KeyDown)
        {
            if ((evt.control || evt.command) && evt.keyCode == KeyCode.Backspace)
            {
                if(line > 0)
                {
                    var f = flattenHierarchy[line];
                    if (f.index > 0)
                    {
                        var n = f.node.children[f.index - 1];
                        if(HasBody(n.type))
                        {
                            var count = 0;
                            CountHierarchy(n.children[1], ref count);
                            line -= count;
                        }
                        f.node.children.RemoveAt(f.index - 1);
                        FlattenHierarchy(baseNode, flattenHierarchy);
                        line--;
                    }
                }
            }
            if (evt.keyCode == KeyCode.UpArrow)
            {
                if (line > 0)
                {
                    line--;
                }
            }
            if (evt.keyCode == KeyCode.DownArrow)
            {
                if (line < flattenHierarchy.Count-1)
                {
                    line++;
                }
            }
        }

        if (evt.type == EventType.Repaint)
        {
            var startY = Main.lineSize;
            var drawGUI = new DrawGUI { lineSize = Main.lineSize, style = Main.labelStyle, x = 0, y = startY, borderSize=Main.lineSize*0.3f, indentSize=Main.lineSize };
            DrawBody(drawGUI, baseNode, 0);
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(0, startY + line * drawGUI.lineSize, 200, 2), CreateTex(Color.red));
        }

        if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Return)
        {
            var tokens = Tokenizer.Tokenize(codeLine);
            var node = Parser.ParseLine(tokens);
            var f = flattenHierarchy[line];
            f.node.children.Insert(f.index, node);
            line++;
            codeLine = "";
            FlattenHierarchy(baseNode, flattenHierarchy);
        }
        GUI.color = Color.white;
        GUI.SetNextControlName("ConsoleTextBox");
        codeLine = GUI.TextField(new Rect(0, Screen.height - Main.lineSize, Screen.width, Main.lineSize), codeLine, Main.textboxStyle);
        
        if(GUI.Button(new Rect(0,0,Main.buttonWidth,Main.lineSize), ">", Main.buttonStyle))
        {
            Game.Begin();
            var function = GenerateAsm.Generate(baseNode);
            VM.Run(function);
        }
        if (GUI.Button(new Rect(Main.buttonWidth, 0, Main.buttonWidth, Main.lineSize), "Instructions", Main.buttonStyle))
        {
            Main.gameState = GameState.Instructions;
        }
        if (firstFrame)
        {
            GUI.FocusControl("ConsoleTextBox");
            firstFrame = false;
        }
    }


}
