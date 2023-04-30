using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class IDE
{
    static Node baseNode;
    static string codeLine = "";
    static int line = 0;
    static List<FlatHierarchy> flattenHierarchy = new List<FlatHierarchy>();

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

    static IDE()
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
        Draw(drawGUI, node.token.text, depth);
        DrawExpression(drawGUI, node.children[1], depth+1);
    }

    static void DrawValue(DrawGUI drawGUI, Node node, int depth)
    {
        Draw(drawGUI, node.token.text, depth);
    }

    static void DrawExpression(DrawGUI drawGUI, Node node, int depth)
    {
        switch (node.type)
        {
            case NodeType.Number: DrawValue(drawGUI, node, depth + 1); return;
            case NodeType.Varname: DrawValue(drawGUI, node, depth + 1); return;
            case NodeType.Add: DrawBinaryOp(drawGUI, node, depth + 1); return;
            case NodeType.Sub: DrawBinaryOp(drawGUI, node, depth + 1); return;
            case NodeType.Div: DrawBinaryOp(drawGUI, node, depth + 1); return;
            case NodeType.Mul: DrawBinaryOp(drawGUI, node, depth + 1); return;
            case NodeType.LT: DrawBinaryOp(drawGUI, node, depth + 1); return;
            case NodeType.MT: DrawBinaryOp(drawGUI, node, depth + 1); return;
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
                Draw(drawGUI, c.token.text, depth + 1);
                Draw(drawGUI, "(", depth);
                DrawExpression(drawGUI, c.children[0], depth+1);
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
            else if(c.type == NodeType.Var)
            {
                Draw(drawGUI, "var", depth);
                Draw(drawGUI, c.token.text, depth + 1);
                Draw(drawGUI, "=", depth);
                DrawExpression(drawGUI, c.children[0], depth + 1);
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
            if(c.type == NodeType.If)
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
            if (c.type == NodeType.If)
            {
                CountHierarchy(c.children[1], ref count);
            }
            count++;
        }
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
                        if(n.type == NodeType.If)
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
            var startY = 20;
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 20;
            var drawGUI = new DrawGUI { lineSize = 30, style = style, x = 0, y = startY, borderSize=4, indentSize=20 };
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
        codeLine = GUI.TextField(new Rect(0, Screen.height - 20, Screen.width, 20), codeLine);

        if(GUI.Button(new Rect(0,0,100,20), ">"))
        {
            Game.Begin();
            var instructions = GenerateAsm.Generate(baseNode);
            VM.Run(instructions);
        }
    }


}
